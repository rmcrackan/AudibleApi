using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public class CustomerOptions
	{
		public static CustomerOptions All => new CustomerOptions { ResponseGroups = ResponseGroupOptions.ALL_OPTIONS };

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
			ALL_OPTIONS = (1 << 5) - 1
		}
		public ResponseGroupOptions ResponseGroups { get; set; }
		public string ToQueryString()
		{
			var parameters = new List<string>();

			if (ResponseGroups != ResponseGroupOptions.None)
				parameters.Add(ResponseGroups.ToResponseGroupsQueryString());

			if (!parameters.Any())
				return "";

			return parameters.Aggregate((a, b) => $"{a}&{b}");
		}
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
			var locale = Localization.Locales.First(l => l.MarketPlaceId == marketPlace);
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
