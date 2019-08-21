using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BaseLib;

namespace AudibleApi.Authentication
{
    public static class LoginResultRunner
    {
        public static async Task<LoginResult> GetResultsPageAsync(IHttpClient client, ISystemDateTime systemDateTime, Dictionary<string, string> inputs)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));
			if (systemDateTime is null)
				throw new ArgumentNullException(nameof(systemDateTime));
			if (inputs is null)
				throw new ArgumentNullException(nameof(inputs));

			// uses client to make the POST request
			var response = await makeRequestAsync(
				client,
				HttpMethod.Post,
				new Uri(Resources.AmazonLoginUri, "/ap/signin"),
				new FormUrlEncodedContent(inputs));

            // passes client for later use. ie: client is not used in this call
            foreach (var factory in ResultFactory.GetAll())
                if (await factory.IsMatchAsync(response))
                    return await factory.CreateResultAsync(client, systemDateTime, response, inputs);

            throw new LoginFailedException();
        }

        private static async Task<HttpResponseMessage> makeRequestAsync(IHttpClient client, HttpMethod method, Uri uri, HttpContent content = null)
        {
			#region debug
			var preCallCookies = client.CookieJar
				.EnumerateCookies(Resources.AmazonLoginUri)
				?.ToList()
				.Select(c => $"{c.Name}={c.Value}")
				.Aggregate("", (a, b) => $"{a};{b}")
				.Trim(';');
			#endregion

			var request = new HttpRequestMessage()
			{
				Method = method,
				RequestUri = uri,
				Content = content
			};
			var response = await client.SendAsync(request);

			#region debug
			var postCallCookies = client.CookieJar
				.EnumerateCookies(Resources.AmazonLoginUri)
				?.ToList()
				.Select(c => $"{c.Name}={c.Value}")
				.Aggregate("", (a, b) => $"{a};{b}")
				.Trim(';');
			#endregion

			#region debug
			string input = null;
			if (content != null)
				input = await content?.ReadAsStringAsync();
			var requestUri = response.RequestMessage.RequestUri.AbsoluteUri;
			var requestHeaders = response.RequestMessage.Headers;
			var sCode = response.StatusCode;
			var codeInt = (int)sCode;
			var headers = response.Headers.ToString();
			string body = null;
			if (response.Content != null)
				body = await response.Content.ReadAsStringAsync();
			var debugLog
				= $"method: {method}\r\n"
				+ $"request uri: {uri.AbsoluteUri}\r\n"
				+ $"request headers: {requestHeaders}\r\n"
				+ $"cookies before request: {preCallCookies}\r\n"
				+ $"content input: {input}\r\n"
				+ $"RESPONSE\r\n"
				+ $"code: {sCode}={codeInt}\r\n"
				+ $"headers: {headers}\r\n"
				+ $"cookies after request: {postCallCookies}\r\n"
				+ $"body length: {body?.Length}";
			var debug2 = debugLog;
			#endregion

			// We want to handle redirects ourselves so that we can determine the final redirect Location (via header)

			// check for is-complete exit condition before checking status code.
			// is-complete might be a redirect or 404
			if (await ResultFactory.LoginComplete.IsMatchAsync(response))
                return response;

			// if not a redirect, we're done
			var statusCode = (int)response.StatusCode;
            if (statusCode < 300 || statusCode > 399)
            {
                response.EnsureSuccessStatusCode();
                return response;
            }

            var redirectUri = response.Headers.Location;
            if (!redirectUri.IsAbsoluteUri)
                redirectUri = new Uri(uri.GetOrigin() + redirectUri);
            Console.WriteLine($"Redirecting to {redirectUri}");
			// re-directs should always be GET
            return await makeRequestAsync(client, HttpMethod.Get, redirectUri);
        }
    }
}
