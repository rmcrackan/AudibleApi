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
		public Api _api;

		private AudibleApiClient() { }
		public async static Task<AudibleApiClient> CreateClientAsync()
		{
			Localization.SetLocale("us");
			var api = await EzApiCreator.GetApiAsync(AudibleApiStorage.AccountsSettingsFile, AudibleApiStorage.TEST_GetFirstIdentityTokensJsonPath(), new LoginCallback());
			return new AudibleApiClient { _api = api };
		}

		#region api call examples
		// Mimi's Adventure (3m)
		public const string TINY_BOOK_ASIN = "B079DZ8YMP";

		// Harry Potter 1 (8h 33m)
		public const string MEDIUM_BOOK_ASIN = "B017V4IM1G";

		// Sherlock Holmes (62h 52m)
		public const string HUGE_BOOK_ASIN = "B06WLMWF2S";

		public const string AD_HOC_ASIN = "B07D4KZVXL";

		public Task PrintLibraryAsync() => wrapCallAsync(printLibraryAsync);
		private async Task printLibraryAsync()
		{
			// test ad hoc api calls

			string url;
			string allGroups = "";

			url
				= "/1.0/library"
				+ "?purchaseAfterDate=01/01/1970&page=23";
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

		public Task AccountInfoAsync() => wrapCallAsync(accountInfoAsync);
		private async Task accountInfoAsync()
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

		public Task DownloadBookAsync() => wrapCallAsync(downloadBookAsync);
		private async Task downloadBookAsync()
		{
			using var progressBar = new Dinah.Core.ConsoleLib.ProgressBar();
			var progress = new Progress<DownloadProgress>();
			progress.ProgressChanged += (_, e) => progressBar.Report(Math.Round((double)(100 * e.BytesReceived) / e.TotalBytesToReceive.Value) / 100);

			Console.Write("Download book");
			var finalFile = await _api.DownloadAaxWorkaroundAsync(TINY_BOOK_ASIN, "downloadExample.xyz", progress);

			Console.WriteLine(" Done!");
			Console.WriteLine("final file: " + Path.GetFullPath(finalFile));

			// benefit of this small delay:
			// - if you try to delete a file too soon after it's created, the OS isn't done with the creation and you can get an unexpected error
			// - give progressBar's internal timer time to finish. if timer is disposed before the final message is processed, "100%" will never get a chance to be displayed
			await Task.Delay(100);

			File.Delete(finalFile);
		}

		/// <summary>Generate report. Summarizes which fields are exposed by each library ResponseGroupOption enum</summary>
		public Task DocumentLibraryResponseGroupOptionsAsync() => wrapCallAsync(documentLibraryResponseGroupOptionsAsync);
		private async Task documentLibraryResponseGroupOptionsAsync()
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

		public Task DeserializeSingleBookAsync() => wrapCallAsync(deserializeSingleBookAsync);
		private async Task deserializeSingleBookAsync()
		{
			var bookResult = await _api.GetLibraryBookAsync(AD_HOC_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
			var bookResultString = bookResult.ToString();
			var bookResultJson = AudibleApiDTOs.BookDtoV10.FromJson(bookResultString);
			var bookResultItem = bookResultJson.Item;
		}

		private async Task wrapCallAsync(Func<Task> fn)
		{
			try
			{
				await fn();
			}
			catch (AudibleApiException aex)
			{
				Console.WriteLine("ERROR:");
				Console.WriteLine(aex.Message);
				Console.WriteLine(aex.JsonMessage.ToString());
				Console.WriteLine(aex.RequestUri.ToString());
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR:");
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
		}
		#endregion
    }
}