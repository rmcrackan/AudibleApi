using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public abstract partial class ResultFactory
    {
        private class LoginCompleteFactory : ResultFactory
        {
            public LoginCompleteFactory() : base(nameof(LoginCompleteFactory)) { }

            public override async Task<bool> IsMatchAsync(HttpResponseMessage response)
            {
                // shared validation
                if (!await base.IsMatchAsync(response))
                    return false;

                return getAccessToken(response) != null;
            }

            public override async Task<LoginResult> CreateResultAsync(IHttpClient client, ISystemDateTime systemDateTime, HttpResponseMessage response, Dictionary<string, string> oldInputs)
            {
                // shared validation
                await base.CreateResultAsync(client, systemDateTime, response, oldInputs);

				var body = await response.Content.ReadAsStringAsync();

				var cookies = client.CookieJar
					.EnumerateCookies(Resources.AmazonLoginUri)
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
                var identity = new Identity(Localization.CurrentLocale, accessToken, cookies);

                return new LoginComplete(client, systemDateTime, body, identity);
            }

            private static AccessToken getAccessToken(HttpResponseMessage response)
            {
				if (response?.Headers?.Location is null)
                    return null;

                var parameters = System.Web.HttpUtility.ParseQueryString(response.Headers.Location.Query);

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
