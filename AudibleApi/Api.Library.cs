using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi.Common;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public class LibraryOptions
	{
		public const int NUMBER_OF_RESULTS_PER_PAGE_MIN = 1;
		public const int NUMBER_OF_RESULTS_PER_PAGE_MAX = 1000;

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
				: ArgumentValidator.EnsureGreaterThan(value.Value, nameof(value), 0);
		}

		public DateTime? PurchasedAfter { get; set; }

		[Flags]
		public enum ResponseGroupOptions
		{
			None = 0,
			[Description("badge_types")]
			BadgeTypes = 1 << 0,
			[Description("category_ladders")]
			CategoryLadders = 1 << 1,
			[Description("claim_code_url")]
			ClaimCodeUrl = 1 << 2,
			[Description("contributors")]
			Contributors = 1 << 3,
			[Description("is_downloaded")]
			IsDownloaded = 1 << 4,
			[Description("is_returnable")]
			IsReturnable = 1 << 5,
			[Description("media")]
			Media = 1 << 6,
			[Description("origin_asin")]
			OriginAsin = 1 << 7,
			[Description("pdf_url")]
			PdfUrl = 1 << 8,
			[Description("percent_complete")]
			PercentComplete = 1 << 9,
			[Description("price")]
			Price = 1 << 10,
			[Description("product_attrs")]
			ProductAttrs = 1 << 11,
			[Description("product_desc")]
			ProductDesc = 1 << 12,
			[Description("product_extended_attrs")]
			ProductExtendedAttrs = 1 << 13,
			[Description("product_plan_details")]
			ProductPlanDetails = 1 << 14,
			[Description("product_plans")]
			ProductPlans = 1 << 15,
			/// <summary>"provided_review" is null unless used with "rating"</summary>
			[Description("provided_review")]
			ProvidedReview = 1 << 16,
			[Description("rating")]
			Rating = 1 << 17,
			[Description("relationships")]
			Relationships = 1 << 18,
			[Description("review_attrs")]
			ReviewAttrs = 1 << 19,
			[Description("reviews")]
			Reviews = 1 << 20,
			[Description("sample")]
			Sample = 1 << 21,
			[Description("series")]
			Series = 1 << 22,
			[Description("sku")]
			Sku = 1 << 23,
			ALL_OPTIONS = (1 << 24) - 1
		}
		public ResponseGroupOptions ResponseGroups { get; set; }

		[Flags]
		public enum ImageSizeOptions
		{
			None = 0,
			[Description("252")]
			_252 = 1 << 0,
			[Description("315")]
			_315 = 1 << 1,
			[Description("360")]
			_360 = 1 << 2,
			[Description("408")]
			_408 = 1 << 3,
			[Description("500")]
			_500 = 1 << 4,
			[Description("558")]
			_558 = 1 << 5,
			[Description("570")]
			_570 = 1 << 6,
			[Description("882")]
			_882 = 1 << 7,
			[Description("900")]
			_900 = 1 << 8,
			[Description("1215")]
			_1215 = 1 << 9,
			ALL_OPTIONS = (1 << 10) - 1
		}
		public ImageSizeOptions ImageSizes { get; set; }

		public enum SortByOptions
		{
			None,
			[Description("-Author")]
			AuthorDesc,
			[Description("-Length")]
			LengthDesc,
			[Description("-Narrator")]
			NarratorDesc,
			[Description("-PurchaseDate")]
			PurchaseDateDesc,
			[Description("-Title")]
			TitleDesc,
			[Description("Author")]
			Author,
			[Description("Length")]
			Length,
			[Description("Narrator")]
			Narrator,
			[Description("PurchaseDate")]
			PurchaseDate,
			[Description("Title")]
			Title
		}
		public SortByOptions SortBy { get; set; }

		public string ToQueryString()
		{
			var parameters = new List<string>();

			if (NumberOfResultPerPage.HasValue)
				parameters.Add("num_results=" + NumberOfResultPerPage.Value);

			if (PageNumber.HasValue)
				parameters.Add("page=" + PageNumber.Value);

			if (PurchasedAfter.HasValue)
				parameters.Add("purchased_after=" + PurchasedAfter.Value.ToRfc3339String());

			if (ResponseGroups != ResponseGroupOptions.None)
				parameters.Add(ResponseGroups.ToResponseGroupsQueryString());

			if (ImageSizes != ImageSizeOptions.None)
				parameters.Add(ImageSizes.ToImageSizesQueryString());

			if (SortBy != SortByOptions.None)
				parameters.Add(SortBy.ToSortByQueryString());

			if (!parameters.Any())
				return "";

			return parameters.Aggregate((a, b) => $"{a}&{b}");
		}
	}

	public partial class Api
	{
		const string LIBRARY_PATH = "/1.0/library";

		#region Add Item to Library

		public async Task<bool> AddItemToLibraryAsync(string asin)
		{
			const string itemUrl = LIBRARY_PATH + "/item";
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			var requestBody = new JObject
			{
				{ "asin", asin }
			};

			try
			{
				var response = await AdHocAuthenticatedRequestAsync(itemUrl, HttpMethod.Put, Client, requestBody);

				if (response.IsSuccessStatusCode)
				{
					var message = await response.Content.ReadAsJObjectAsync();

					return message.TryGetValue("error_code", out var token) && token.Type is JTokenType.Null;
				}
				return false;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error adding item [{asin}] from library to library", asin);
				return false;
			}
		}

		public async Task<bool> RemoveItemFromLibraryAsync(string asin)
		{
			const string itemUrl = LIBRARY_PATH + "/item/{0}/default_iphone_loan_id";
			ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin));

			try
			{
				var response = await AdHocAuthenticatedRequestAsync(string.Format(itemUrl, asin), HttpMethod.Delete, Client);

				if (response.IsSuccessStatusCode)
				{
					var message = await response.Content.ReadAsJObjectAsync();

					return message.TryGetValue("error_code", out var token) && token.Type is JTokenType.Null;
				}
				return false;
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error deleting item [{asin}] from library", asin);
				return false;
			}
		}

		#endregion

		#region GetLibraryAsync
		public Task<JObject> GetLibraryAsync()
			=> getLibraryAsync(new LibraryOptions { PurchasedAfter = new DateTime(1970, 1, 1) }.ToQueryString());

		public async Task<JObject> GetLibraryAsync(LibraryOptions libraryOptions)
		{
			if (libraryOptions is null)
				return await GetLibraryAsync();

			return await getLibraryAsync(libraryOptions.ToQueryString());
		}

		public async Task<JObject> GetLibraryAsync(string libraryOptions)
		{
			if (libraryOptions is null)
				return await GetLibraryAsync();

			libraryOptions = libraryOptions.Trim().Trim('?');

			if (string.IsNullOrWhiteSpace(libraryOptions))
				return await GetLibraryAsync();

			return await getLibraryAsync(libraryOptions);
		}

		//
		// state token for library requests: https://github.com/mkb79/Audible/issues/93
		// You will only get new items or removed items since the state-token was created.
		// Removed items will be in the 'items' list with 'status' : 'Revoked'
		//

		// all strings passed here are assumed to be unconditionally valid
		private async Task<JObject> getLibraryAsync(string parameters)
		{
			var response = await getLibraryResponseAsync(parameters);
			var obj = await response.Content.ReadAsJObjectAsync();
			return obj;
		}

		internal async Task<HttpResponseMessage> getLibraryResponseAsync(string parameters)
		{
			var url = $"{LIBRARY_PATH}?{parameters}";
			var response = await AdHocAuthenticatedGetAsync(url);
			return response;
		}
		#endregion

		#region GetLibraryBookAsync
		public Task<Item> GetLibraryBookAsync(string asin, LibraryOptions.ResponseGroupOptions responseGroups)
			=> GetLibraryBookAsync(asin, responseGroups.ToResponseGroupsQueryString());

		public async Task<Item> GetLibraryBookAsync(string asin, string responseGroups)
		{
			asin = ArgumentValidator.EnsureNotNullOrWhiteSpace(asin, nameof(asin)).ToUpper().Trim();

			responseGroups = responseGroups?.Trim().Trim('?');

			var url = $"{LIBRARY_PATH}/{asin}";
			if (!string.IsNullOrWhiteSpace(responseGroups))
				url += "?" + responseGroups;
			var response = await AdHocAuthenticatedGetAsync(url);

			BookDtoV10 dto = await response.Content.ReadAsDtoAsync<BookDtoV10>();

			return dto.Item;
		}
		#endregion

		#region GetAllLibraryItemsAsync
		public async Task<List<Item>> GetAllLibraryItemsAsync()
			=> await GetAllLibraryItemsAsync(
				new LibraryOptions
				{
					ResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS,
					ImageSizes = LibraryOptions.ImageSizeOptions._500 | LibraryOptions.ImageSizeOptions._1215
				});

		public Task<List<Item>> GetAllLibraryItemsAsync(LibraryOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
			=> GetAllLibraryItemsAsync(new LibraryOptions { ResponseGroups = responseGroups }, numItemsPerRequest, maxConcurrentRequests);
		
		public Task<List<Item>> GetAllLibraryItemsAsync(LibraryOptions.ResponseGroupOptions responseGroups, int numItemsPerRequest, SemaphoreSlim semaphore)
			=> GetAllLibraryItemsAsync(new LibraryOptions { ResponseGroups = responseGroups }, numItemsPerRequest, semaphore);

		public async Task<List<Item>> GetAllLibraryItemsAsync(LibraryOptions libraryOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			using var semaphoreSlim = new SemaphoreSlim(maxConcurrentRequests);

			return await GetAllLibraryItemsAsync(libraryOptions, numItemsPerRequest, semaphoreSlim);
		}

		public async Task<List<Item>> GetAllLibraryItemsAsync(LibraryOptions libraryOptions, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			var allItems = new List<Item>();

			await foreach (var items in GetLibraryItemsPagesAsync(libraryOptions, numItemsPerRequest, semaphore))
				allItems.AddRange(items);

			return allItems;
		}

		public async IAsyncEnumerable<Item> GetLibraryItemAsyncEnumerable(LibraryOptions libraryOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			await foreach (var page in GetLibraryItemsPagesAsync(libraryOptions, numItemsPerRequest, maxConcurrentRequests))
				foreach (var item in page)
					yield return item;
		}

		public async IAsyncEnumerable<Item> GetLibraryItemAsyncEnumerable(LibraryOptions libraryOptions, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			await foreach (var page in GetLibraryItemsPagesAsync(libraryOptions, numItemsPerRequest, semaphore))
				foreach (var item in page)
					yield return item;
		}

		public async IAsyncEnumerable<Item[]> GetLibraryItemsPagesAsync(LibraryOptions libraryOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			using var semaphore = new SemaphoreSlim(maxConcurrentRequests);

			await foreach (var item in GetLibraryItemsPagesAsync(libraryOptions, numItemsPerRequest, semaphore))
				yield return item;
		}

		public async IAsyncEnumerable<Item[]> GetLibraryItemsPagesAsync(LibraryOptions libraryOptions, int numItemsPerRequest, SemaphoreSlim semaphore)
		{
			if (!libraryOptions.PurchasedAfter.HasValue || libraryOptions.PurchasedAfter.Value < new DateTime(1970, 1, 1))
				libraryOptions.PurchasedAfter = new DateTime(1970, 1, 1);

			libraryOptions.NumberOfResultPerPage = numItemsPerRequest;

			List<Task<LibraryDtoV10>> pageDlTasks = new();

			int page = 0;
			int totalItems = await GetItemsCountAsync(libraryOptions);
			int totalPages = totalItems / numItemsPerRequest;
			if (totalPages * numItemsPerRequest < totalItems)
				totalPages++;

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

				yield return completed.Result.Items;
			}

			async Task spinupPageRequestAsync()
			{
				libraryOptions.PageNumber = ++page;
				await semaphore.WaitAsync();
				var pageItemTask = downloadItemPage(semaphore, libraryOptions.ToQueryString(), page);
				if (pageItemTask is not null)
                    pageDlTasks.Add(pageItemTask);
			}
		}

		private async Task<LibraryDtoV10> downloadItemPage(SemaphoreSlim semaphore, string queryString, int pageNumber)
		{
			try
			{
				var response = await getLibraryResponseAsync(queryString);
				var	libResult = await response.Content.ReadAsDtoAsync<LibraryDtoV10>();

				// Per audible's api, this shouldn't be possible. However when page number reaches ~400, their api acts weird
				if (libResult?.Items is null)
				{
                    Serilog.Log.Logger.Information($"Page {pageNumber}: 0 results");
					return null;
                }

				Serilog.Log.Logger.Information($"Page {pageNumber}: {libResult.Items.Length} results");
				return libResult;
			}
			finally { semaphore?.Release(); }
		}


		/// <summary>Gets the total number of Items in the account's library</summary>
		public async Task<int> GetItemsCountAsync(LibraryOptions libraryOptions)
		{
			var orig = libraryOptions.NumberOfResultPerPage;

			try
			{
				libraryOptions.NumberOfResultPerPage = 1;

				var response = await getLibraryResponseAsync(libraryOptions.ToQueryString());

				if (response.Headers.TryGetValues("Total-Count", out var values))
					return int.Parse(values.First());

				return -1;
			}
			finally
			{
				libraryOptions.NumberOfResultPerPage = orig;
			}
		}
		#endregion
	}
}
