using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public static class CustomerQueryStringBuilderExtensions
	{
		public static string ToQueryString(this CustomerOptions.ResponseGroupOptions responseGroupOptions)
		{
			if (responseGroupOptions == CustomerOptions.ResponseGroupOptions.None)
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

		public static string ToQueryString(this CustomerOptions customerOptions)
		{
			var parameters = new List<string>();

			if (customerOptions.ResponseGroups != CustomerOptions.ResponseGroupOptions.None)
				parameters.Add(customerOptions.ResponseGroups.ToQueryString());

			if (!parameters.Any())
				return "";

			return parameters.Aggregate((a, b) => $"{a}&{b}");
		}
	}
	public class CustomerOptions
	{
		[Flags]
		public enum ResponseGroupOptions
		{
			None = 0,
			[Description("migration_details")]
			MigrationDetails = 1 << 0,
			[Description("subscription_details_rodizio")]
			SubscriptionDetailsRodizio = 1 << 1,
			[Description("subscription_details_premium")]
			SubscriptionDetailsPremium = 1 << 2,
			[Description("customer_segment")]
			CustomerSegment = 1 << 3,
			[Description("subscription_details_channels")]
			SubscriptionDetailsChannels = 1 << 4,
			// https://stackoverflow.com/questions/7467722
			ALL_OPTIONS = ~(1 << 5)
		}
		public ResponseGroupOptions ResponseGroups { get; set; }
	}

	public partial class Api
	{
		const string CUSTOMER_PATH = "/1.0/customer";

		#region GetInformationAsync
		const string INFORMATION_PATH = CUSTOMER_PATH + "/information";

		/// <summary>Get locale from: /1.0/customer/information</summary>
		public async Task<Locale> GetLocaleAsync(CustomerOptions customerOptions)
		{
			var customer = await GetCustomerInformationAsync(customerOptions);
			var debugStr = customer.ToString(Formatting.Indented);
			var marketPlace = customer["customer_details"]["migration_details"][0]["to_marketplaceId"].Value<string>();
			var locale = Localization.Locales.SingleOrDefault(l => l.MarketPlaceId == marketPlace);
			return locale;
		}

		public Task<JObject> GetCustomerInformationAsync(CustomerOptions customerOptions)
			=> GetCustomerInformationAsync(customerOptions.ToQueryString());

		public async Task<JObject> GetCustomerInformationAsync(string customerOptions)
		{
			customerOptions = customerOptions?.Trim().Trim('?');

			var url = $"{INFORMATION_PATH}?{customerOptions}";
			var response = await AdHocAuthenticatedGetAsync(url);
			var obj = await response.Content.ReadAsJObjectAsync();
			return obj;
		}
		#endregion
	}
}
