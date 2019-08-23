using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public partial class Api
    {
        const string WISHLIST_PATH = "/1.0/wishlist";

        public async Task<bool> IsInWishListAsync(string asin)
        {
            if (asin is null)
                throw new ArgumentNullException(nameof(asin));
            if (string.IsNullOrWhiteSpace(asin))
                throw new ArgumentException();

            // test with page results = 10. for production => 50
            var num_results = 50;

            // pages are 0 indexed
            var page = 0;
            var accum = 0;
            int total_results;

            // iterate through all pages
            do
            {
                var url = $"{WISHLIST_PATH}?num_results={num_results}&page={page}&sort_by=-DateAdded";
                var response = await AdHocAuthenticatedGetAsync(url);
				var obj = await response.Content.ReadAsJObjectAsync();

				var products = obj["products"] as JArray;

                page++;
                accum += products.Count;
                total_results = (int)obj["total_results"];

                if (products.Any(p => p.Value<string>("asin") == asin))
                    return true;
            }
            while (accum < total_results);

            return false;
        }

        public async Task AddToWishListAsync(string asin)
        {
            if (asin is null)
                throw new ArgumentNullException(nameof(asin));
            if (string.IsNullOrWhiteSpace(asin))
                throw new ArgumentException();

            var body = JObject.Parse($@"{{""asin"":""{asin}""}}");

            var request = new HttpRequestMessage(HttpMethod.Post, WISHLIST_PATH);
            // POST body: see AddContent overloads
            request.AddContent(body);

            await signRequestAsync(request);

            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            // same return values whether it already existed in wish list or newly added
            if (response.StatusCode != HttpStatusCode.Created)
                throw new ApiErrorException(
                    request.RequestUri,
                    JObject.Parse(responseString),
                    $"Add to Wish List failed. Invalid status code. Code: {response.StatusCode}"
                    );

            var location = response.Headers.Location.ToString();
            if (location != $"{WISHLIST_PATH}/{asin}")
                throw new ApiErrorException(
                    request.RequestUri,
                    JObject.Parse(responseString),
                    $"Add to Wish List failed. Bad location. Location: {location}"
                    );
        }

        public async Task DeleteFromWishListAsync(string asin)
        {
            if (asin is null)
                throw new ArgumentNullException(nameof(asin));
            if (string.IsNullOrWhiteSpace(asin))
                throw new ArgumentException();

            var requestUri = $"{WISHLIST_PATH}/{asin}";
            var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);

            await signRequestAsync(request);

            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            // same return values whether it already existed in wish list or newly added
            if (response.StatusCode != HttpStatusCode.NoContent)
                throw new ApiErrorException(
                    request.RequestUri,
                    JObject.Parse(responseString),
                    $"Delete from Wish List failed. Invalid status code. Code: {response.StatusCode}. Asin: {asin}"
                    );
        }
    }
}
