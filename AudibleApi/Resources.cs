using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Dinah.Core;

namespace AudibleApi
{
	public static class Resources
	{
		public const string USER_AGENT = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148";
		public const string DeviceType = "A2CZJZGLK2JJVM";
		public const string IosVersion = "15.0.0";
		public const string SoftwareVersion = "35602678";
		public const string AppVersion = "3.56.2";
		public const string AppName = "Audible";
		public const string DeviceModel = "iPhone";
		public const string DeviceName = "%FIRST_NAME%%FIRST_NAME_POSSESSIVE_STRING%%DUPE_STRATEGY_1ST%Audible for Libation";

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

		public static string OAuthUrl(this Locale locale, string deviceSerial, string codeVerifier) => locale._loginUrl() + "/ap/signin?" + locale.buildOauth(deviceSerial, codeVerifier);
		private static string buildOauth(this Locale locale,  string deviceSerial, string codeVerifier)
		{
			// this helps dramatically with debugging
			var return_to = $"{locale.LoginUri().GetOrigin()}/ap/maplanding";
			var assoc_handle = locale.WithUsername ? $"amzn_audible_ios_lap_{locale.CountryCode}" : $"amzn_audible_ios_{locale.CountryCode}";
			var page_id = locale.WithUsername ? "amzn_audible_ios_privatepool" : "amzn_audible_ios";

			var code_challenge = create_s256_code_challenge(codeVerifier);
			var client_id = build_client_id(deviceSerial);

			var oauth_params = new Dictionary<string, string>
			{
				// these are NOT dependent on locale and do NOT use https
				{"openid.oa2.response_type", "code"},
				{"openid.oa2.code_challenge_method", "S256"},
				{"openid.oa2.code_challenge", code_challenge },
				{"openid.return_to", return_to},
				{ "openid.assoc_handle", assoc_handle},
				{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "pageId", page_id},
				{ "accountStatusPolicy", "P1"},
				{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "openid.mode", "checkid_setup"},
				{ "openid.ns.oa2", "http://www.amazon.com/ap/ext/oauth/2"},
				{ "openid.oa2.client_id", $"device:{client_id}"},
				{ "openid.ns.pape", "http://specs.openid.net/extensions/pape/1.0"},
				{ "marketPlaceId", locale.MarketPlaceId},
				{ "openid.oa2.scope", "device_auth_access"},
				{ "forceMobileLayout", "true"},
				{ "openid.ns", "http://specs.openid.net/auth/2.0"},
				{ "openid.pape.max_auth_age", "0"}
			};

			var encoded = urlencode(oauth_params);
			return encoded;
		}
		static string urlencode(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
			=> nameValuePairs
			.Select(kvp => $"{System.Web.HttpUtility.UrlEncode(kvp.Key)}={System.Web.HttpUtility.UrlEncode(kvp.Value)}")
			.Aggregate((a, b) => $"{a}&{b}");

		public static string build_client_id(string deviceSerialNumber)
		{
			var client_id_bytes = Encoding.UTF8.GetBytes($"{deviceSerialNumber}#{DeviceType}");
			return BitConverter.ToString(client_id_bytes).Replace("-", "").ToLower();
		}

		private static string create_s256_code_challenge(string code_verifier)
		{
			using var sha256 = SHA256.Create();

			sha256.ComputeHash(Encoding.ASCII.GetBytes(code_verifier));
			return Convert.ToBase64String(sha256.Hash).Replace('+', '-').Replace('/', '_').TrimEnd('=');
		}
	}
}
