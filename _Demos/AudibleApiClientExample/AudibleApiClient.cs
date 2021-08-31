using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Common;
using Dinah.Core;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApiClientExample
{
	public class AudibleApiClient
	{
		public Api Api { get; }
		public AudibleApiClient(Api api) => Api = api;

		#region sample books
		/// <summary>Mimi's Adventure. 3 minutes. Book I own</summary>
		public const string TINY_BOOK_ASIN = "B079DZ8YMP";

		/// <summary>Harry Potter #1. 8h 33m). Book I own</summary>
		public const string MEDIUM_BOOK_ASIN = "B017V4IM1G";

		/// <summary>Sherlock Holmes. 62h 52m. Book I own</summary>
		public const string HUGE_BOOK_ASIN = "B06WLMWF2S";

		/// <summary>Book I do not own from skeezy publisher</summary>
		public const string DO_NOT_OWN_ASIN = "2291090836";
		#endregion

		#region api call examples
		public async Task PrintLibraryAsync()
		{
			// test ad hoc api calls

			string url;
			string allGroups = "";

			url
				= "/1.0/library"
				+ "?purchased_after=1980-01-01T00:00:00Z"
				+ "&num_results=1000"
				+ "&page=1"
				;
			//url = "/1.0/library/" +
			//	//TINY_BOOK_ASIN
			//	MEDIUM_BOOK_ASIN
			//	//HUGE_BOOK_ASIN
			//	;

			url += url.Contains("?") ? "&" : "?";

			allGroups = "response_groups=badge_types,category_ladders,claim_code_url,contributors,is_downloaded,is_returnable,media,origin_asin,pdf_url,percent_complete,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,product_plans,provided_review,rating,relationships,review_attrs,reviews,sample,series,sku";
			//allGroups = "response_groups=series,category_ladders,contributors";

			url += allGroups;
			var responseMsg = await Api.AdHocAuthenticatedGetAsync(url);
			var jObj = await responseMsg.Content.ReadAsJObjectAsync();
			var str = jObj.ToString(Formatting.Indented);
			Console.WriteLine(str);
		}

		public async Task AccountInfoAsync()
		{
			string groups = "";

			var url = "/1.0/customer/information";
			groups = "migration_details,subscription_details_rodizio,subscription_details_premium,customer_segment,subscription_details_channels";


			if (!string.IsNullOrWhiteSpace(groups))
				url += (url.Contains("?") ? "&" : "?") + "response_groups=" + groups.Replace(" ", "").Replace("[", "").Replace("]", "");
			var responseMsg = await Api.AdHocAuthenticatedGetAsync(url);
			var jObj = await responseMsg.Content.ReadAsJObjectAsync();
			var str = jObj.ToString(Formatting.Indented);
			Console.WriteLine(str);


			var str2 = (await Api.UserProfileAsync()).ToString(Formatting.Indented);
			Console.WriteLine(str2);
		}
		#endregion

		#region handing podcasts
		public async Task PodcastTestsAsync()
		{
			// when 'following' this podcast series, this is the parent which shows up in my library
			var podcastParent = "B08DCRTX5K";
			var parentLibBook = await Api.GetLibraryBookAsync(podcastParent, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			var isEpisodes = parentLibBook.IsEpisodes; // true

			// as a library "item"
			var parentLibBookInfo = await Api.GetLibraryBookAsync(podcastParent, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			// as a catalog "product". much less info
			var parentBookInfo = await Api.GetCatalogProductAsync(podcastParent, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);

			// the episodes are not considered part of my library. GetLibraryBookAsync() will throw. however, GetBookInfoAsync() and all download mechanics succeed
			var children = parentLibBook.Relationships
				.Where(r => r.RelationshipToProduct == RelationshipToProduct.Child && r.RelationshipType == RelationshipType.Episode)
				.ToList();
			var childRelationshipEntry = children.First();
			var childId = childRelationshipEntry.Asin;
			//// throws: not present in customer library
			//try { await client.Api.GetLibraryBookAsync(childId, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS); }
			//catch (Exception ex) { }

			// get 1
			var childProductInfo = await Api.GetCatalogProductAsync(childId, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
			// or get multiples at once
			var childrenIds = children.Select(c => c.Asin).ToList();
			var results = await Api.GetCatalogProductsAsync(childrenIds, CatalogOptions.ResponseGroupOptions.ALL_OPTIONS);
		}
		#endregion

		#region reports, research, diagnostics
		const string LIBRARY_JSON = "lib.json";
		public async Task DownloadLibraryToFileAsync()
		{
			var url
				= "/1.0/library"
				+ "?purchased_after=1980-01-01T00:00:00Z"
				+ "&num_results=1000"
				+ "&page=1"
				;
			url += url.Contains("?") ? "&" : "?";
			var allGroups
				= "response_groups=badge_types,category_ladders,claim_code_url,contributors,is_downloaded,is_returnable,media,"
				+ "origin_asin,pdf_url,percent_complete,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,"
				+ "product_plans,provided_review,rating,relationships,review_attrs,reviews,sample,series,sku";

			url += allGroups;
			var responseMsg = await Api.AdHocAuthenticatedGetAsync(url);
			var jObj = await responseMsg.Content.ReadAsJObjectAsync();
			var str = jObj.ToString(Formatting.Indented);
			File.WriteAllText(LIBRARY_JSON, str);
			Console.WriteLine(str);
		}

		public static void AnaylzeLibrary()
		{
			// if no current lib.json file, run DownloadLibraryToFileAsync() above

			var contents = File.ReadAllText(LIBRARY_JSON);
			var o = JObject.Parse(contents);

			var asins = o.SelectTokens("$.items[?(@.asin != null)].asin").ToList();
			var nonnullplans = o.SelectTokens("$.items[?(@.plans != null)].asin").ToList();
			var nullplans = o.SelectTokens("$.items[?(@.plans == null)].asin").ToList();

			// plan_name "US Minerva" == "Audible Plus"
			// plan_name "SpecialBenefit" has a lot of overlap but doesn't fully contain 'plus' titles
			var minervaTokens = o.SelectTokens("$.items[?(@.plans[?(@.plan_name == 'US Minerva')])].title").ToList();
			var minervaTitles = minervaTokens.Select(t => t.Value<string>()).ToList();
			var minervaTitlesStr = minervaTitles.Aggregate((a, b) => $"{a}\r\n{b}");

			// full entries for minerva items
			var minervaItems = o
				.SelectTokens("$.items[?(@.plans[?(@.plan_name == 'US Minerva')])]")
				.Select(t => t.ToString(Formatting.Indented))
				.ToList();
			var m = minervaItems.Aggregate((a, b) => $"{a}\r\n{b}");

			// how to tell ones that are ALSO in my main library:
			// these are 'plus' and NOT owned by me
			var AYCL = o.SelectTokens("$.items[?(@.benefit_id == 'AYCL')].title").ToList();
			var ayce = o.SelectTokens("$.items[?(@.is_ayce == true)].title").ToList();
			var AYCL_ayce = o.SelectTokens("$.items[?(@.benefit_id == 'AYCL' && @.is_ayce == true)].title").ToList();			
		}
		#endregion
	}
}