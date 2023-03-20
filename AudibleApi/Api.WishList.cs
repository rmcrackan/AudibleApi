using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi.Common;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public class WishListOptions
	{
		public const int NUMBER_OF_RESULTS_PER_PAGE_MIN = 0;
		public const int NUMBER_OF_RESULTS_PER_PAGE_MAX = 50;

		private int? _numResults;
		public int? NumberOfResultPerPage
		{
			get => _numResults;
			set => _numResults
				= value is null
				? null
				: ArgumentValidator.EnsureBetweenInclusive(value.Value, nameof(value), NUMBER_OF_RESULTS_PER_PAGE_MIN, NUMBER_OF_RESULTS_PER_PAGE_MAX);
		}

		private int? _page;
		public int? PageNumber
		{
			get => _page;
			set => _page
				= value is null
				? null
				: ArgumentValidator.EnsureGreaterThan(value.Value, nameof(value), -1);
		}

		[Flags]
		public enum ResponseGroupOptions
		{
			None = 0,
			[Description("contributors")]
			Contributors = 1 << 0,
			[Description("media")]
			Media = 1 << 1,
			[Description("price")]
			Price = 1 << 2,
			[Description("product_attrs")]
			ProductAttrs = 1 << 3,
			[Description("product_desc")]
			ProductDesc = 1 << 4,
			[Description("product_extended_attrs")]
			ProductExtendedAttrs = 1 << 5,
			[Description("product_plan_details")]
			ProductPlanDetails = 1 << 6,
			[Description("product_plans")]
			ProductPlans = 1 << 7,
			[Description("rating")]
			Rating = 1 << 8,
			[Description("sample")]
			Sample = 1 << 9,
			[Description("sku")]
			Sku = 1 << 10,
			[Description("customer_rights")]
			CustomerRights = 1 << 11,
			[Description("relationships")]
			Relationships = 1 << 12,
			ALL_OPTIONS = (1 << 13) - 1
		}

		public ResponseGroupOptions ResponseGroups { get; set; }

		public enum SortByOptions
		{
			None,
			[Description("-Author")]
			AuthorDesc,
			[Description("-DateAdded")]
			DateAddedDesc,
			[Description("-Price")]
			PriceDesc,
			[Description("-Rating")]
			RatingDesc,
			[Description("-Title")]
			TitleDesc,
			[Description("Author")]
			Author,
			[Description("DateAdded")]
			DateAdded,
			[Description("Price")]
			Price,
			[Description("Rating")]
			Rating,
			[Description("Title")]
			Title
		}
		public SortByOptions SortBy { get; set; }

		public string ToQueryString(Locale locale)
		{
			var parameters = new List<string>
			{
				"locale=" + locale.Language
			};

			if (NumberOfResultPerPage.HasValue)
				parameters.Add("num_results=" + NumberOfResultPerPage.Value);

			if (PageNumber.HasValue)
				parameters.Add("page=" + PageNumber.Value);

			if (ResponseGroups != ResponseGroupOptions.None)
				parameters.Add(ResponseGroups.ToResponseGroupsQueryString());

			if (SortBy != SortByOptions.None)
				parameters.Add(SortBy.ToSortByQueryString());

			if (!parameters.Any())
				return "";

			return string.Join("&", parameters);
		}
	}
	public partial class Api
	{
		const string WISHLIST_PATH = "/1.0/wishlist";

		public async Task<List<Product>> GetWishListProductsAsync(WishListOptions wishlistOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			using var semaphoreSlim = new SemaphoreSlim(maxConcurrentRequests);

			return await GetWishListProductsAsync(wishlistOptions, numItemsPerRequest, semaphoreSlim);
		}

		public async Task<List<Product>> GetWishListProductsAsync(WishListOptions wishlistOptions, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			var allItems = new List<Product>();

			await foreach (var items in GetWishListPagesAsync(wishlistOptions, numItemsPerRequest, semaphore))
				allItems.AddRange(items);

			return allItems;
		}

		public async IAsyncEnumerable<Product> GetWishListProductsAsyncEnumerable(WishListOptions wishlistOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			await foreach (var page in GetWishListPagesAsync(wishlistOptions, numItemsPerRequest, maxConcurrentRequests))
				foreach (var item in page)
					yield return item;
		}

		public async IAsyncEnumerable<Product> GetWishListProductsAsyncEnumerable(WishListOptions wishlistOptions, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			await foreach (var page in GetWishListPagesAsync(wishlistOptions, numItemsPerRequest, semaphore))
				foreach (var item in page)
					yield return item;
		}

		public async IAsyncEnumerable<Product[]> GetWishListPagesAsync(WishListOptions wishlistOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			using var semaphore = new SemaphoreSlim(maxConcurrentRequests);

			await foreach (var item in GetWishListPagesAsync(wishlistOptions, numItemsPerRequest, semaphore))
				yield return item;
		}

		public async IAsyncEnumerable<Product[]> GetWishListPagesAsync(WishListOptions wishlistOptions, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			ArgumentValidator.EnsureNotNull(wishlistOptions, nameof(wishlistOptions));
			ArgumentValidator.EnsureNotNull(semaphore, nameof(semaphore));

			wishlistOptions.NumberOfResultPerPage = ArgumentValidator.EnsureBetweenInclusive(
				numItemsPerRequest,
				nameof(numItemsPerRequest),
				WishListOptions.NUMBER_OF_RESULTS_PER_PAGE_MIN,
				WishListOptions.NUMBER_OF_RESULTS_PER_PAGE_MAX);

			List<Task<WishListDtoV10>> pageDlTasks = new();

			int page = 0;
			int totalItems = await GetWishListItemsCountAsync(wishlistOptions);
			int totalPages = totalItems / numItemsPerRequest;
			if (totalPages * numItemsPerRequest < totalItems) totalPages++;

			//Spin up as many concurrent downloads as we can/need. Minimum 1.
			do
				await spinupPageRequestAsync();
			while (semaphore.CurrentCount > 0 && page < totalPages);

			while (pageDlTasks.Count > 0)
			{
				var completed = await Task.WhenAny(pageDlTasks);
				pageDlTasks.Remove(completed);

				//Request new page(s)
				while (semaphore.CurrentCount > 0 && page < totalPages)
					await spinupPageRequestAsync();

				yield return completed.Result.Products;
			}

			async Task spinupPageRequestAsync()
			{
				wishlistOptions.PageNumber = page;
				await semaphore.WaitAsync();
				pageDlTasks.Add(getWishListPageAsync(semaphore, wishlistOptions.ToQueryString(Locale), page));
				page++;
			}
		}

		private async Task<WishListDtoV10> getWishListPageAsync(SemaphoreSlim semaphore, string queryString, int pageNumber)
		{
			try
			{
				var url = $"{WISHLIST_PATH}?{queryString}";
				var response = await AdHocAuthenticatedGetAsync(url);

				var libResult = await response.Content.ReadAsDtoAsync<WishListDtoV10>();

				Serilog.Log.Logger.Information($"Page {pageNumber}: {libResult.Products.Length} results");
				return libResult;
			}
			finally { semaphore?.Release(); }
		}


		/// <summary>Gets the total number of Items in the account's wish list</summary>
		public async Task<int> GetWishListItemsCountAsync(WishListOptions wishlistOptions)
		{
			var orig = wishlistOptions.NumberOfResultPerPage;

			try
			{
				wishlistOptions.PageNumber = 0;
				wishlistOptions.NumberOfResultPerPage = 0;

				var url = $"{WISHLIST_PATH}?{wishlistOptions.ToQueryString(Locale)}";
				var response = await AdHocAuthenticatedGetAsync(url);

				var dto = await response.Content.ReadAsDtoAsync<WishListDtoV10>();

				return dto.TotalResults;
			}
			finally
			{
				wishlistOptions.NumberOfResultPerPage = orig;
			}
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
