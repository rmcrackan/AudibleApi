using System;
using BaseLib;

namespace AudibleApi
{
	public static class Resources
	{
		public const string UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 12_3_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148";

		private static string _audibleApiUrl
			=> $"https://api.audible.{Localization.CurrentLocale.Domain}";
		public static Uri AudibleApiUri => new Uri(_audibleApiUrl);

		private static string _amazonApiUrl
			=> $"https://api.amazon.{Localization.CurrentLocale.Domain}";
		public static Uri AmazonApiUri => new Uri(_amazonApiUrl);

		private static string _amazonLoginUrl
			=> $"https://www.amazon.{Localization.CurrentLocale.Domain}";
		public static Uri AmazonLoginUri => new Uri(_amazonLoginUrl);

		private static string buildOauth()
		{
			// this helps dramatically with debugging
			var return_to = $"{AmazonLoginUri.GetOrigin()}/ap/maplanding";
			var assoc_handle = $"amzn_audible_ios_{Localization.CurrentLocale.CountryCode}";
			var marketPlaceId = Localization.CurrentLocale.MarketPlaceId;

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
			q["pageId"] = "amzn_audible_ios";
			q["accountStatusPolicy"] = "P1";
			q["openid.mode"] = "checkid_setup";
			q["openid.oa2.client_id"] = "device:6a52316c62706d53427a5735505a76477a45375959566674327959465a6374424a53497069546d45234132435a4a5a474c4b324a4a564d";
			q["language"] = LanguageTag;
			q["marketPlaceId"] = marketPlaceId;
			q["openid.oa2.scope"] = "device_auth_access";
			q["forceMobileLayout"] = "true";
			q["openid.pape.max_auth_age"] = "0";

			#region debug
			var str = q.ToString();
			#endregion

			return str;
		}
		public static string OAuthUrl
			=> _amazonLoginUrl + "/ap/signin?" + buildOauth();

		public static string RegisterDomain
			=> $".amazon.{Localization.CurrentLocale.Domain}";

		public static string LanguageTag
			=> Localization.CurrentLocale.Language;
	}
}
