using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
		public const int NUMBER_OF_RESULTS_PER_PAGE_MIN = 0;
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

		internal async Task<System.Net.Http.HttpResponseMessage> getLibraryResponseAsync(string parameters)
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
			if (asin is null)
				throw new ArgumentNullException(nameof(asin));
			if (string.IsNullOrWhiteSpace(asin))
				throw new ArgumentException("asin may not be blank", nameof(asin));

			asin = asin.ToUpper().Trim();

			responseGroups = responseGroups?.Trim().Trim('?');

			var url = $"{LIBRARY_PATH}/{asin}";
			if (!string.IsNullOrWhiteSpace(responseGroups))
				url += "?" + responseGroups;
			var response = await AdHocAuthenticatedGetAsync(url);
			var obj = await response.Content.ReadAsJObjectAsync();
			var objStr = obj.ToString();

			BookDtoV10 dto;
			try
			{
				// important! use this convert/deser method
				dto = BookDtoV10.FromJson(objStr);
			}
			catch (Exception ex)
			{
				Serilog.Log.Logger.Error(ex, "Error converting catalog product. Full json:\r\n" + objStr);
				throw;
			}

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

		public async Task<List<Item>> GetAllLibraryItemsAsync(LibraryOptions.ResponseGroupOptions responseGroups)
			=> await getAllLibraryItemsAsync_gated(new LibraryOptions { ResponseGroups = responseGroups });

		public async Task<List<Item>> GetAllLibraryItemsAsync(LibraryOptions libraryOptions)
			=> await getAllLibraryItemsAsync_gated(libraryOptions);
		
		public async IAsyncEnumerable<Item> GetLibraryItemAsyncEnumerable(LibraryOptions libraryOptions, int numItemsPerRequest = 50, int maxConcurrentRequests = 10)
		{
			if (!libraryOptions.PurchasedAfter.HasValue || libraryOptions.PurchasedAfter.Value < new DateTime(1970, 1, 1))
				libraryOptions.PurchasedAfter = new DateTime(1970, 1, 1);

			libraryOptions.NumberOfResultPerPage = 0;
			int totalCount = await GetItemsCountAsync(this, libraryOptions);
			libraryOptions.NumberOfResultPerPage = numItemsPerRequest;

			int numPages = totalCount / libraryOptions.NumberOfResultPerPage.Value;
			if (numPages * libraryOptions.NumberOfResultPerPage.Value < totalCount)
				numPages++;

			List<Task<Item[]>> allDlTasks = new();
			using SemaphoreSlim concurrencySemaphore = new(maxConcurrentRequests);

			for (int page = 1; page <= numPages; page++)
			{
				libraryOptions.PageNumber = page;
				var queryString = libraryOptions.ToQueryString();

				allDlTasks.Add(downloadItemPage(concurrencySemaphore, queryString, page));
			}

			while (allDlTasks.Count > 0)
			{
				var completed = await Task.WhenAny(allDlTasks);
				allDlTasks.Remove(completed);
				foreach (var item in completed.Result)
					yield return item;
			}
		}

		private async Task<Item[]> downloadItemPage(SemaphoreSlim concurrencySemaphore, string queryString, int pageNumber)
		{
			await concurrencySemaphore.WaitAsync();
			try
			{
				var response = await getLibraryResponseAsync($"{queryString}");

				var page = await response.Content.ReadAsStringAsync();
				LibraryDtoV10 libResult;
				try
				{
					// important! use this convert/deser method
					libResult = LibraryDtoV10.FromJson(page);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error converting library for importing use. Full library:\r\n" + page);
					throw;
				}

				Serilog.Log.Logger.Information($"Page {pageNumber}: {libResult.Items.Length} results");
				return libResult.Items;
			}
			finally
			{
				concurrencySemaphore.Release();
			}
		}

		private async Task<int> GetItemsCountAsync(Api api, LibraryOptions libraryOptions)
		{
			var response = await api.getLibraryResponseAsync(libraryOptions.ToQueryString());

			int totalCount = -1;
			if (response.Headers.TryGetValues("Total-Count", out var values))
				totalCount = int.Parse(values.First());

			return totalCount;
		}

		private async Task<List<Item>> getAllLibraryItemsAsync_gated(LibraryOptions libraryOptions)
        {
			if (!libraryOptions.PurchasedAfter.HasValue || libraryOptions.PurchasedAfter.Value < new DateTime(1970, 1, 1))
				libraryOptions.PurchasedAfter = new DateTime(1970, 1, 1);

			try
            {
                // max 1000 however with higher numbers it stops returning 'provided_review' and 'reviews' groups.
                // Sometimes this threshold is as high as 900, sometimes as low as 400.
                // I've never had problems at 300. Another user had nearly constant problems at 300.
                libraryOptions.NumberOfResultPerPage = 250;
                return await getAllLibraryItemsAsync_internal(libraryOptions);
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is TimeoutException)
            {
                // if it times out with 250, try 50. This takes about 5 seconds longer for a library of 1,100
                //
                // For each batch size, I ran 3 tests. Results in milliseconds
                //   1000    19389 , 17760 , 19256
                //    500    19099 , 19905 , 18553
                //    250    20288 , 19163 , 19605
                //    100    22156 , 22058 , 22438
                //     50    25017 , 24292 , 24491
                //     25    28627 , 30006 , 31201
                //     10    45006 , 46717 , 44924
                libraryOptions.NumberOfResultPerPage = 50;
                return await getAllLibraryItemsAsync_internal(libraryOptions);
            }
            catch (Exception)
            {
                throw;
            }
        }

		private async Task<List<Item>> getAllLibraryItemsAsync_internal(LibraryOptions libraryOptions)
		{
			var allItems = new List<Item>();

			for (var pageNumber = 1; ; pageNumber++)
			{
				// if attempting to paginate more than 10,000 titles : {"error_code":null,"message":"Implied library size is unsupported"}"
				if (pageNumber * libraryOptions.NumberOfResultPerPage > 10000)
				{
					Serilog.Log.Logger.Information($"Maximum reached. Cannot retrieve more than 10,000 titles");
					break;
				}

				libraryOptions.PageNumber = pageNumber;
				var page = await GetLibraryAsync(libraryOptions);

				var pageStr = page.ToString();

				LibraryDtoV10 libResult;
				try
				{
					// important! use this convert/deser method
					libResult = LibraryDtoV10.FromJson(pageStr);
				}
				catch (Exception ex)
				{
					Serilog.Log.Logger.Error(ex, "Error converting library for importing use. Full library:\r\n" + pageStr);
					throw;
				}

				if (!libResult.Items.Any())
					break;

				Serilog.Log.Logger.Information($"Page {pageNumber}: {libResult.Items.Length} results");
				allItems.AddRange(libResult.Items);
			}

			return allItems;
		}
		#endregion
	}
}
