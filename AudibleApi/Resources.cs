using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dinah.Core;

namespace AudibleApi
{
	public static class Resources
	{
		public const string USER_AGENT = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0";
		public const string DEVICE_TYPE = "A2CZJZGLK2JJVM";
		public static string DeviceSerialNumber { get; private set; }

		public static string LoginDomain(this Locale locale) => locale.WithUsername ? "audible" : "amazon";

		public static string RegisterDomain(this Locale locale) => $".amazon.{locale.TopDomain}";

		private static string _audibleApiUrl(this Locale locale) => $"https://api.audible.{locale.TopDomain}";
		public static Uri AudibleApiUri(this Locale locale) => new Uri(locale._audibleApiUrl());

		private static string _audibleLoginUrl(this Locale locale) => $"https://www.audible.{locale.TopDomain}";
		public static Uri AudibleLoginUri(this Locale locale) => new Uri(locale._audibleLoginUrl());

		private static string _amazonApiUrl(this Locale locale) => $"https://api.amazon.{locale.TopDomain}";
		public static Uri AmazonApiUri(this Locale locale) => new Uri(locale._amazonApiUrl());

		private static string _loginUrl(this Locale locale) => $"https://www.{locale.LoginDomain()}.{locale.TopDomain}";
		public static Uri LoginUri(this Locale locale) => new Uri(locale._loginUrl());

		private static string _registrationUrl(this Locale locale) => $"https://api.{locale.LoginDomain()}.{locale.TopDomain}";
		public static Uri RegistrationUri(this Locale locale) => new Uri(locale._registrationUrl());

		public static string OAuthUrl(this Locale locale) => locale._loginUrl() + "/ap/signin?" + locale.buildOauth();
		private static string buildOauth(this Locale locale)
		{
			// this helps dramatically with debugging
			var return_to = $"{locale.LoginUri().GetOrigin()}/ap/maplanding";
			var assoc_handle = locale.WithUsername ? $"amzn_audible_ios_lap_{locale.CountryCode}" : $"amzn_audible_ios_{locale.CountryCode}";
			var page_id = locale.WithUsername ? "amzn_audible_ios_privatepool" : "amzn_audible_ios";

			// https://github.com/mkb79/Audible/blob/master/src/audible/login.py#L133
			DeviceSerialNumber ??= build_device_serial();
			var client_id = build_client_id(DeviceSerialNumber);

			var oauth_params = new Dictionary<string, string>
			{
				// these are NOT dependent on locale and do NOT use https
				["openid.identity"] = "http://specs.openid.net/auth/2.0/identifier_select",
				["openid.claimed_id"] = "http://specs.openid.net/auth/2.0/identifier_select",
				["openid.ns.oa2"] = "http://www.amazon.com/ap/ext/oauth/2",
				["openid.ns.pape"] = "http://specs.openid.net/extensions/pape/1.0",
				["openid.ns"] = "http://specs.openid.net/auth/2.0",

				["openid.oa2.response_type"] = "token",
				["openid.return_to"] = return_to,
				["openid.assoc_handle"] = assoc_handle,
				["pageId"] = page_id,
				["accountStatusPolicy"] = "P1",
				["openid.mode"] = "checkid_setup",
				["openid.oa2.client_id"] = $"device:{client_id}",
				["language"] = locale.Language,
				["marketPlaceId"] = locale.MarketPlaceId,
				["openid.oa2.scope"] = "device_auth_access",
				["forceMobileLayout"] = "true",
				["openid.pape.max_auth_age"] = "0"
			};

			var encoded = urlencode(oauth_params);
			return encoded;
		}
		static string urlencode(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
			=> nameValuePairs
			.Select(kvp => $"{System.Web.HttpUtility.UrlEncode(kvp.Key)}={System.Web.HttpUtility.UrlEncode(kvp.Value)}")
			.Aggregate((a, b) => $"{a}&{b}");

		//https://github.com/mkb79/Audible/blob/master/src/audible/login.py
		private static string build_device_serial() => Guid.NewGuid().ToString("N").ToUpper();

		private static string build_client_id(string deviceSerialNumber)
		{
			var client_id_bytes = Encoding.UTF8.GetBytes($"{deviceSerialNumber}#{DEVICE_TYPE}");
			return BitConverter.ToString(client_id_bytes).Replace("-", "");
		}
	}
}
