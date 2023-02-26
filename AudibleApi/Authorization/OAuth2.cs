using System;
using System.Linq;

namespace AudibleApi.Authorization
{
	public record OAuth2
	{
		public static OAuth2 Empty => new(string.Empty);
		public string Code { get; }
		public RegistrationOptions RegistrationOptions { get; set; }

		private OAuth2(string authCode) => Code = authCode;

		public static OAuth2 Parse(Uri uri)
			=> uri.IsAbsoluteUri
			? ParseQuery(uri?.Query)
			: Parse(uri?.OriginalString);

		public static OAuth2 Parse(string url) => ParseQuery(url?.Split('?').Last());

		public static OAuth2 ParseQuery(string urlQueryPortion)
		{
			if (string.IsNullOrWhiteSpace(urlQueryPortion))
				return null;

			// keys and values are already url-decoded
			var parameters = System.Web.HttpUtility.ParseQueryString(urlQueryPortion);

			const string tokenKey = "openid.oa2.authorization_code";

			if (!parameters.AllKeys.Contains(tokenKey))
				return null;

			return string.IsNullOrWhiteSpace(parameters[tokenKey]) ? null : new OAuth2(parameters[tokenKey]);
		}
	}
}
