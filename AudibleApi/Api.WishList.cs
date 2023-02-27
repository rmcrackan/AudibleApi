using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public partial class Api
	{
		const string WISHLIST_PATH = "/1.0/wishlist";

		public async Task<bool> IsInWishListAsync(string asin)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

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
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var body = JObject.Parse($@"{{""asin"":""{asin}""}}");

			var response = await AdHocAuthenticatedRequestAsync(WISHLIST_PATH, HttpMethod.Post, Client, body);
			var responseString = await response.Content.ReadAsStringAsync();

			// same return values whether it already existed in wish list or newly added
			if (response.StatusCode != HttpStatusCode.Created)
				throw new ApiErrorException(
					WISHLIST_PATH,
					JObject.Parse(responseString),
					$"Add to Wish List failed. Invalid status code. Code: {response.StatusCode}"
					);

			var location = response.Headers.Location.ToString();
			if (location != $"{WISHLIST_PATH}/{asin}")
				throw new ApiErrorException(
					WISHLIST_PATH,
					JObject.Parse(responseString),
					$"Add to Wish List failed. Bad location. Location: {location}"
					);
		}

		public async Task DeleteFromWishListAsync(string asin)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var requestUri = $"{WISHLIST_PATH}/{asin}";

			var response = await AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Delete, Client);
			var responseString = await response.Content.ReadAsStringAsync();

			// same return values whether it already existed in wish list or newly added
			if (response.StatusCode != HttpStatusCode.NoContent)
				throw new ApiErrorException(
					requestUri,
					JObject.Parse(responseString),
					$"Delete from Wish List failed. Invalid status code. Code: {response.StatusCode}. Asin: {asin}"
					);
		}
	}
}
