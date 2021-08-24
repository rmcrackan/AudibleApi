using System;
using System.Text;
using Dinah.Core;

namespace AudibleApi
{
	public static class Resources
	{
		public const string UserAgent = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0";
		public const string DeviceType = "A2CZJZGLK2JJVM";
		public static string DeviceSerialNumber { get; private set; }
		private static string _audibleApiUrl(this Locale locale) => $"https://api.audible.{locale.TopDomain}";
		public static Uri AudibleApiUri(this Locale locale) => new Uri(locale._audibleApiUrl());

		private static string _audibleLoginUrl(this Locale locale) => $"https://www.audible.{locale.TopDomain}";
		public static Uri AudibleLoginUri(this Locale locale) => new Uri(locale._audibleLoginUrl());

		private static string _amazonApiUrl(this Locale locale) => $"https://api.amazon.{locale.TopDomain}";
		public static Uri AmazonApiUri(this Locale locale) => new Uri(locale._amazonApiUrl());

		private static string _amazonLoginUrl(this Locale locale) => $"https://www.{locale.LoginDomain}.{locale.TopDomain}";
		public static Uri AmazonLoginUri(this Locale locale) => new Uri(locale._amazonLoginUrl());

		public static string OAuthUrl(this Locale locale) => locale._amazonLoginUrl() + "/ap/signin?" + locale.buildOauth();
		private static string buildOauth(this Locale locale)
		{
			// this helps dramatically with debugging

			bool with_username = locale.LoginDomain == "audible";

			var return_to = $"{locale.AmazonLoginUri().GetOrigin()}/ap/maplanding";
			var assoc_handle = with_username? $"amzn_audible_ios_lap_{locale.CountryCode}" : $"amzn_audible_ios_{locale.CountryCode}";
			var marketPlaceId = locale.MarketPlaceId;
			var page_id = with_username ? "amzn_audible_ios_privatepool" : "amzn_audible_ios";

			//https://github.com/mkb79/Audible/blob/master/src/audible/login.py#L133
			DeviceSerialNumber ??= build_device_serial();
			var client_id = build_client_id(DeviceSerialNumber);

			var q = System.Web.HttpUtility.ParseQueryString("");

			// these are NOT dependent on locale and do NOT use https
			q["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select";
			q["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select";
			q["openid.ns.oa2"] = "http://www.amazon.com/ap/ext/oauth/2";
			q["openid.ns.pape"] = "http://specs.openid.net/extensions/pape/1.0";
			q["openid.ns"] = "http://specs.openid.net/auth/2.0";

			q["openid.oa2.response_type"] = "token";
			q["openid.return_to"] = return_to;
			q["openid.assoc_handle"] = assoc_handle;
			q["pageId"] = page_id;
			q["accountStatusPolicy"] = "P1";
			q["openid.mode"] = "checkid_setup";
			q["openid.oa2.client_id"] = $"device:{client_id}";
			q["language"] = locale.Language;
			q["marketPlaceId"] = marketPlaceId;
			q["openid.oa2.scope"] = "device_auth_access";
			q["forceMobileLayout"] = "true";
			q["openid.pape.max_auth_age"] = "0";

			#region debug
			var str = q.ToString();
			#endregion

			return str;
		}

		//https://github.com/mkb79/Audible/blob/master/src/audible/login.py
		private static string build_device_serial() => Guid.NewGuid().ToString("N").ToUpper();
		private static string build_client_id(string deviceSerialNumber)
		{
			var client_id_bytes = Encoding.UTF8.GetBytes($"{deviceSerialNumber}#{DeviceType}");
			return BitConverter.ToString(client_id_bytes).Replace("-", "");
		}

		public static string RegisterDomain(this Locale locale) => $".amazon.{locale.TopDomain}";
	}
}
