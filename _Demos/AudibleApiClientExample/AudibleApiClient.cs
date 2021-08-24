using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using Dinah.Core;
using Dinah.Core.Net.Http;
//using InternalUtilities; // uncomment for use demo app to use live libation settings. must have the rest of libation codebase set up
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApiClientExample
{
	public class AudibleApiClient
	{
		private Api _api;

		private AudibleApiClient() { }
		public async static Task<AudibleApiClient> CreateClientAsync()
		{
			/* // uncomment for use demo app to use live libation settings. must have the rest of libation codebase set up
			var account = AudibleApiStorage
				.GetAccountsSettingsPersister()
				.AccountsSettings
				.GetAll()
				.FirstOrDefault();
			var api = await EzApiCreator.GetApiAsync(
				account.Locale,
				AudibleApiStorage.AccountsSettingsFile,
				account.GetIdentityTokensJsonPath(),
				new LoginCallback());
			 */

			// see locales.json for choices
			var localeName = "canada";
			var identityFilePath = "myIdentity.json";
			// if your json file is complex, you can specify the jsonPath within that file where identity is/should be stored.
			// else: null
			string jsonPath = null;
			var api = await EzApiCreator.GetApiAsync(
				Localization.Get(localeName),
				identityFilePath,
				jsonPath,
				new LoginCallback());
			return new AudibleApiClient { _api = api };
		}

		#region api call examples
		// Mimi's Adventure (3m)
		public const string TINY_BOOK_ASIN = "B079DZ8YMP";

		// Harry Potter 1 (8h 33m)
		public const string MEDIUM_BOOK_ASIN = "B017V4IM1G";

		// Sherlock Holmes (62h 52m)
		public const string HUGE_BOOK_ASIN = "B06WLMWF2S";

		public const string AD_HOC_ASIN = "B00FKAHZ62";

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
			//	//AD_HOC_ASIN
			//	;

			url += url.Contains("?") ? "&" : "?";

			allGroups = "response_groups=badge_types,category_ladders,claim_code_url,contributors,is_downloaded,is_returnable,media,origin_asin,pdf_url,percent_complete,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,product_plans,provided_review,rating,relationships,review_attrs,reviews,sample,series,sku";
			//allGroups = "response_groups=series,category_ladders,contributors";

			url += allGroups;
			var responseMsg = await _api.AdHocAuthenticatedGetAsync(url);
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
			var responseMsg = await _api.AdHocAuthenticatedGetAsync(url);
			var jObj = await responseMsg.Content.ReadAsJObjectAsync();
			var str = jObj.ToString(Formatting.Indented);
			Console.WriteLine(str);


			var str2 = (await _api.UserProfileAsync()).ToString(Formatting.Indented);
			Console.WriteLine(str2);
		}

		public async Task DeserializeSingleBookInfoAsync()
		{
			var bookResult = await _api.GetLibraryBookAsync(AD_HOC_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			var bookResultString = bookResult.ToString();
			var bookResultJson = AudibleApiDTOs.BookDtoV10.FromJson(bookResultString);
			var bookResultItem = bookResultJson.Item;
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
			var responseMsg = await _api.AdHocAuthenticatedGetAsync(url);
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