using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authentication
{
    public abstract partial class ResultFactory
    {
        private class LoginCompleteFactory : ResultFactory
        {
			protected override bool AllowBlankBodyIn_IsMatch => true;

			public LoginCompleteFactory() : base(nameof(LoginCompleteFactory)) { }

            protected override bool _isMatchAsync(HttpResponseMessage response, string body) => getAccessToken(response) is not null;

            protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
            {
				var cookies = authenticate.LoginClient.CookieJar
					.EnumerateCookies(authenticate.Locale.AmazonLoginUri())
					?.Select(c => new KeyValuePair<string, string>(c.Name, c.Value))
					.ToList();

				#region debug
				var currCookies = cookies
					?.Select(kvp => $"{kvp.Key}={kvp.Value}")
					.Aggregate("", (a, b) => $"{a};{b}")
					.Trim(';');
				var requestUri = response?.RequestMessage?.RequestUri?.AbsoluteUri;
				var location = response?.Headers?.Location?.ToString();
				var sCode = response.StatusCode;
				var codeInt = (int)sCode;
				var headers = response.Headers.ToString();
				var debugLog
					= $"response request uri: {requestUri}\r\n"
					+ $"cookies: {currCookies}\r\n"
					+ $"RESPONSE\r\n"
					+ $"location: {location}\r\n"
					+ $"code: {sCode}={codeInt}\r\n"
					+ $"headers: {headers}\r\n"
					+ $"body length: {body?.Length}";
				var debug2 = debugLog;
				#endregion

				var accessToken = getAccessToken(response);

                // authentication complete. begin authorization
                var identity = new Identity(authenticate.Locale, accessToken, cookies);

                return new LoginComplete(authenticate, body, identity);
            }

            private static AccessToken getAccessToken(HttpResponseMessage response)
            {
				if (response?.Headers?.Location is null)
                    return null;

                var location = response.Headers.Location;
                var query
                    = location.IsAbsoluteUri ? location.Query
                    : location.OriginalString.Contains('?') ? location.OriginalString.Split('?').Last()
                    : throw new NotAuthenticatedException(
                        response.RequestMessage?.RequestUri,
                        new JObject
                        {
                            { "requestUri", response.RequestMessage?.RequestUri?.OriginalString },
                            { "statusCode", (int)response.StatusCode },
                            { "responseLocation", response.Headers.Location.OriginalString },
                            { "responseHeaders", response.Headers.ToString() },
                        },
                        $"{nameof(LoginCompleteFactory)}.{nameof(getAccessToken)}: error parsing response location query");

                var parameters = System.Web.HttpUtility.ParseQueryString(query);

                var tokenKey = "openid.oa2.access_token";
                if (!parameters.AllKeys.Contains(tokenKey))
                    return null;

                var timeKey = "openid.pape.auth_time";
                if (!parameters.AllKeys.Contains(timeKey))
                    return null;

                var expires = parameters[timeKey];
                var token = new AccessToken(parameters[tokenKey], DateTime.Parse(expires));
                return token;
            }
        }
    }
}
