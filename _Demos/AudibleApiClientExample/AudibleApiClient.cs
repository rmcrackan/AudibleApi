using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AudibleApi;
using Dinah.Core;
using Dinah.Core.Net.Http;
using InternalUtilities;
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

		public async Task DeserializeSingleBookAsync()
		{
			var bookResult = await _api.GetLibraryBookAsync(AD_HOC_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			var bookResultString = bookResult.ToString();
			var bookResultJson = AudibleApiDTOs.BookDtoV10.FromJson(bookResultString);
			var bookResultItem = bookResultJson.Item;
		}
		#endregion

		#region reports, research, diagnostics
		/// <summary>Generate report. Summarizes which fields are exposed by each library ResponseGroupOption enum</summary>
		public async Task DocumentLibraryResponseGroupOptionsAsync()
		{
			using var sharedReportStringWriter = new StreamWriter("report.txt");

			//// test each enum in isolation
			//var enums = ((LibraryOptions.ResponseGroupOptions[])Enum.GetValues(typeof(LibraryOptions.ResponseGroupOptions))).ToList();
			//enums.Remove(LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			//foreach (var option in enums)
			//	await responseGroupReport(sharedReportStringWriter, option);

			// ad hoc
			await responseGroupReport(sharedReportStringWriter, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS, true);
			//await responseGroupReport(sharedReportStringWriter, LibraryOptions.ResponseGroupOptions.ProvidedReview | LibraryOptions.ResponseGroupOptions.Rating);
		}
		private async Task responseGroupReport(
			StreamWriter sw,
			LibraryOptions.ResponseGroupOptions option,
			bool saveFullJsonToFile = false,
			bool omitCommonFields = false)
		{
			var desc = option.GetDescription() ?? "[none]";

			var page = await _api.GetLibraryAsync(new LibraryOptions
			{
				NumberOfResultPerPage = 1000,
				PurchasedAfter = new DateTime(2000, 1, 1),
				ResponseGroups = option
			});

			if (saveFullJsonToFile)
				File.WriteAllText(
					"report_" + desc.Replace("|", "-").Replace(" ", "").Truncate(50) + ".txt",
					desc + "\r\n\r\n" + page.ToString(Formatting.Indented));

			var properties = new Dictionary<string, int>();

			var items = page["items"].Cast<JObject>();
			foreach (var item in items)
			{
				foreach (var p in item.Properties())
				{
					var propName = p.Name;
					var v = p.Value;

					switch (v.Type)
					{
						case JTokenType.Null:
							continue;
						case JTokenType.String:
							var s = v.Value<string>();
							if (string.IsNullOrWhiteSpace(s))
								continue;
							break;
						case JTokenType.Object:
							if (!v.Value<JObject>().Properties().Any())
								continue;
							break;
						case JTokenType.Array:
							if (!v.Value<JArray>().Any())
								continue;
							break;
						case JTokenType.Date: _ = v.Value<DateTime>(); break;
						case JTokenType.Boolean: _ = v.Value<bool>(); break;
						case JTokenType.Integer: _ = v.Value<int>(); break;
						case JTokenType.Float: _ = v.Value<float>(); break;
						default:
							throw new Exception("not handled JTokenType");
					}

					if (!properties.ContainsKey(p.Name))
						properties[p.Name] = 0;
					properties[p.Name]++;
				}
			}

			if (omitCommonFields)
			{
				// don't show fields common to all response_groups
				properties.Remove("asin");
				properties.Remove("purchase_date");
				properties.Remove("sku_lite");
				properties.Remove("status");
			}

			sw.WriteLine(desc);
			sw.WriteLine("==========");
			foreach (var kvp in properties)
				sw.WriteLine($"{kvp.Key}");
			sw.WriteLine();
		}

		// what signafies that a book is part of 'audible plus'?
		public async Task CompareProductsAsync()
		{
			var asins = new (string asin, string note)[] {
				("B00FKAHZ62", "borrowing from plus"),
				("B017V4IM1G", "i own. not in plus"),
				("B082MQ5TDB", "i own. was a free original. is available in plus"),
				("B002V19RO6", "i own. was not free. is available in plus")
			};

			var topLevelFields = new[] { "asin", "title", "subtitle" };

			// write headers
			Console.Write("note\t");
			foreach (var f in topLevelFields)
				Console.Write(f + "\t");
			Console.Write("isMinerva");
			Console.WriteLine();

			foreach (var (asin, note) in asins)
			{
				Console.Write(note + "\t");

				var url = "/1.0/library/" + asin;
				url += url.Contains("?") ? "&" : "?";
				var allGroups = "response_groups=badge_types,category_ladders,claim_code_url,contributors,is_downloaded,is_returnable,media,origin_asin,pdf_url,percent_complete,price,product_attrs,product_desc,product_extended_attrs,product_plan_details,product_plans,provided_review,rating,relationships,review_attrs,reviews,sample,series,sku";
				url += allGroups;

				var responseMsg = await _api.AdHocAuthenticatedGetAsync(url);
				var jObj = await responseMsg.Content.ReadAsJObjectAsync();
				var debugStr = jObj.ToString(Formatting.Indented);

				foreach (var f in topLevelFields)
				{
					var s = jObj["item"][f].ToString(Formatting.Indented);
					Console.Write(s + "\t");
				}

				// plans
				var minerva = jObj["item"]["plans"].ToArray().SingleOrDefault(x => x["plan_name"].Value<string>() == "US Minerva");
				Console.Write(minerva?["start_date"].ToString(Formatting.Indented));

				Console.WriteLine();
			}
		}

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


			var _ = true;
		}
		#endregion
	}
}