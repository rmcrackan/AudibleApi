using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public static class QueryStringBuilderExtensions
	{
		public static string ToQueryString(this LibraryOptions.ResponseGroupOptions responseGroupOptions)
		{
			if (responseGroupOptions == LibraryOptions.ResponseGroupOptions.None)
				return "";

			var descriptions = responseGroupOptions
				.ToValues()
				.Select(e => e.GetDescription())
				.ToList();
			if (!descriptions.Any() || descriptions.Any(d => d is null))
				throw new Exception("Unexpected value in response group");
			var str = "response_groups=" + descriptions.Aggregate((a, b) => $"{a},{b}");
			return str;
		}

		public static string ToQueryString(this LibraryOptions.SortByOptions sortByOptions)
		{
			if (sortByOptions == LibraryOptions.SortByOptions.None)
				return "";

			var description = sortByOptions.GetDescription();
			if (description is null)
				throw new Exception("Unexpected value for sort by");
			var str = "sort_by=" + description;
			return str;
		}

		public static string ToQueryString(this LibraryOptions libraryOptions)
		{
			var parameters = new List<string>();

			if (libraryOptions.NumberOfResultPerPage.HasValue)
				parameters.Add("num_results=" + libraryOptions.NumberOfResultPerPage.Value);

			if (libraryOptions.PageNumber.HasValue)
				parameters.Add("page=" + libraryOptions.PageNumber.Value);

			if (libraryOptions.PurchasedAfter.HasValue)
				parameters.Add("purchased_after=" + libraryOptions.PurchasedAfter.Value.ToRfc3339String());

			if (libraryOptions.ResponseGroups != LibraryOptions.ResponseGroupOptions.None)
				parameters.Add(libraryOptions.ResponseGroups.ToQueryString());

			if (libraryOptions.SortBy != LibraryOptions.SortByOptions.None)
				parameters.Add(libraryOptions.SortBy.ToQueryString());

			if (!parameters.Any())
				return "";

			return parameters.Aggregate((a, b) => $"{a}&{b}");
		}
	}
	public class LibraryOptions
	{
		private int? _numResults;
		public int? NumberOfResultPerPage
		{
			get => _numResults;
			set
			{
				if (!value.HasValue)
				{
					_numResults = null;
					return;
				}

				if (value > 1000 || value < 1)
					throw new ArgumentException($"{nameof(NumberOfResultPerPage)} must be between 1-1000");
				_numResults = value;
			}
		}

		private int? _page;
		public int? PageNumber
		{
			get => _page;
			set
			{
				if (!value.HasValue)
				{
					_page = null;
					return;
				}

				if (value < 1)
					throw new ArgumentException($"{nameof(PageNumber)} must be 1 or greater");

				_page = value;
			}
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
			// https://stackoverflow.com/questions/7467722
			ALL_OPTIONS = ~(1 << 24)
		}
		public ResponseGroupOptions ResponseGroups { get; set; }

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
	}

	public partial class Api
    {
        const string LIBRARY_PATH = "/1.0/library";

		#region GetLibraryAsync
		public Task<JObject> GetLibraryAsync()
			=> getLibAsync("purchaseAfterDate=01/01/1970");

		public async Task<JObject> GetLibraryAsync(LibraryOptions libraryOptions)
		{
			if (libraryOptions is null)
				return await GetLibraryAsync();

			return await getLibAsync(libraryOptions.ToQueryString());
		}

		public async Task<JObject> GetLibraryAsync(string libraryOptions)
		{
			if (libraryOptions is null)
				return await GetLibraryAsync();

			libraryOptions = libraryOptions.Trim().Trim('?');

			if (string.IsNullOrWhiteSpace(libraryOptions))
				return await GetLibraryAsync();

			return await getLibAsync(libraryOptions);
		}

		// all strings passed here are assumed to be unconditionally valid
		private async Task<JObject> getLibAsync(string parameters)
		{
			var url = $"{LIBRARY_PATH}?{parameters}";
			var response = await AdHocAuthenticatedGetAsync(url);
			var obj = await response.Content.ReadAsJObjectAsync();
			return obj;
		}
		#endregion

		#region GetLibraryBookAsync
		public Task<JObject> GetLibraryBookAsync(string asin, LibraryOptions.ResponseGroupOptions responseGroups)
			=> GetLibraryBookAsync(asin, responseGroups.ToQueryString());

		public async Task<JObject> GetLibraryBookAsync(string asin, string responseGroups)
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
			return obj;
		}
		#endregion
	}
}
