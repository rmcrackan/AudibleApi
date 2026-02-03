using AudibleApi.Authorization;
using Dinah.Core.Net;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace AudibleApi.Authentication;

internal abstract partial class ResultFactory
{
	private class LoginCompleteFactory : ResultFactory
	{
		protected override bool AllowBlankBodyIn_IsMatch => true;

		public LoginCompleteFactory() : base(nameof(LoginCompleteFactory)) { }

		protected override bool _isMatchAsync(HttpResponseMessage response, string body) => getAuthorizationCode(response) is not null;

		protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string?> oldInputs)
		{
			var cookies = authenticate.LoginClient.CookieJar
				.EnumerateCookies(authenticate.Locale.LoginUri())
				?.Select(c => new KeyValuePair<string, string?>(c.Name, c.Value))
				.ToList();

			#region debug
			var currCookies = cookies
				?.Select(kvp => $"{kvp.Key}={kvp.Value}")
				.Aggregate("", (a, b) => $"{a};{b}")
				.Trim(';');
			var requestUri = response.RequestMessage?.RequestUri?.AbsoluteUri;
			var location = response.Headers?.Location?.ToString();
			var sCode = response.StatusCode;
			var codeInt = (int)sCode;
			var headers = response.Headers?.ToString();
			var debugLog
				= $"response request uri: {requestUri}\r\n"
				+ $"cookies: {currCookies}\r\n"
				+ $"RESPONSE\r\n"
				+ $"location: {location}\r\n"
				+ $"code: {sCode}={codeInt}\r\n"
				+ $"headers: {headers}\r\n"
				+ $"body length: {body.Length}";
			var debug2 = debugLog;
			#endregion

			var authCode = getAuthorizationCode(response)
				?? throw new ApiErrorException(response.RequestMessage?.RequestUri, null, "Failed to get authorization code from response body");

			// authentication complete. begin authorization
			var identity = new Identity(authenticate.Locale, authCode with { RegistrationOptions = authenticate.RegistrationOptions }, cookies);

			return new LoginComplete(authenticate, body, identity);
		}

		private static OAuth2? getAuthorizationCode(HttpResponseMessage response)
		{
			if (response?.Headers?.Location is null)
				return null;

			var location = response.Headers.Location;
			if (!location.IsAbsoluteUri && !location.OriginalString.Contains('?'))
				throw new NotAuthenticatedException(
					response.RequestMessage?.RequestUri,
					new JObject
					{
						{ "requestUri", response.RequestMessage?.RequestUri?.OriginalString },
						{ "statusCode", (int)response.StatusCode },
						{ "responseLocation", response.Headers.Location.OriginalString },
						{ "responseHeaders", response.Headers.ToString() },
					},
					$"{nameof(LoginCompleteFactory)}.{nameof(getAuthorizationCode)}: error parsing response location query");

			return OAuth2.Parse(location);
		}
	}
}
