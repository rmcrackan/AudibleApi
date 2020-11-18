using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public static class LoginResultRunner
	{
		public static async Task<LoginResult> GetResultsPageAsync(Authenticate authenticate, string url)
		{
			if (authenticate is null)
				throw new ArgumentNullException(nameof(authenticate));
			if (url is null)
				throw new ArgumentNullException(nameof(url));

			// uses client to make the POST request
			var response = await makeRequestAsync(authenticate, HttpMethod.Get, new Uri(url));

			return await getResultsPageAsync(authenticate, new Dictionary<string, string>(), response);
		}

		public static async Task<LoginResult> GetResultsPageAsync(Authenticate authenticate, Dictionary<string, string> inputs)
		{
			if (authenticate is null)
				throw new ArgumentNullException(nameof(authenticate));
			if (inputs is null)
				throw new ArgumentNullException(nameof(inputs));

			// uses client to make the POST request
			var response = await makeRequestAsync(
				authenticate,
				HttpMethod.Post,
				new Uri(authenticate.Locale.AmazonLoginUri(), "/ap/signin"),
				new FormUrlEncodedContent(inputs));

			return await getResultsPageAsync(authenticate, inputs, response);
		}

		private static async Task<LoginResult> getResultsPageAsync(Authenticate authenticate, Dictionary<string, string> inputs, HttpResponseMessage response)
		{
			foreach (var factory in ResultFactory.GetAll())
				if (await factory.IsMatchAsync(response))
					return await factory.CreateResultAsync(authenticate, response, inputs);

			// no match. throw exception
			var body = await response.Content.ReadAsStringAsync();
			var newInputs = HtmlHelper.GetInputs(body);
			var responseInputs = getSanitizedInputs(newInputs);

			var loginFailedException = new LoginFailedException("No matching result page type")
			{
				RequestUrl = response.RequestMessage?.RequestUri?.AbsoluteUri,
				ResponseStatusCode = response.StatusCode,
				ResponseInputFields = responseInputs,
			};
			loginFailedException.SaveResponseBodyFile(body, $"{nameof(LoginFailedException)}_ResponseBody_{DateTime.Now.Ticks}.tmp");

			throw loginFailedException;
		}

		private static Dictionary<string, string> getSanitizedInputs(Dictionary<string, string> inputs)
		{
			if (inputs == null || !inputs.Any())
				return inputs;

			static string mask(KeyValuePair<string, string> kvp)
				=> kvp.Key.EqualsInsensitive("showPasswordChecked") || kvp.Key.EqualsInsensitive("encryptedPasswordExpected") ? kvp.Value
				: kvp.Key.ContainsInsensitive("password") ? (
					kvp.Value is null ? "[null]"
					: kvp.Value == "" ? "[empty]"
					: string.IsNullOrWhiteSpace(kvp.Value) ? "[blank]"
					: "[password hidden]")
				: kvp.Key.EqualsInsensitive("metadata1") || kvp.Key.EqualsInsensitive("workflowState") || kvp.Key.EqualsInsensitive("ces") ? kvp.Value.Truncate(20) + "..."
				: kvp.Key.EqualsInsensitive("email") ? kvp.Value.ToMask()
				: kvp.Value;

			return inputs
				.Select(kvp => new { kvp.Key, Value = mask(kvp) })
				.ToDictionary(x => x.Key, x => x.Value);
		}

		private static async Task<HttpResponseMessage> makeRequestAsync(Authenticate authenticate, HttpMethod method, Uri uri, HttpContent content = null)
        {
			#region debug
			var preCallCookies = authenticate.LoginClient.CookieJar
				.EnumerateCookies(authenticate.Locale.AmazonLoginUri())
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
			var response = await authenticate.LoginClient.SendAsync(request);

			#region debug
			var postCallCookies = authenticate.LoginClient.CookieJar
				.EnumerateCookies(authenticate.Locale.AmazonLoginUri())
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
			if (await ResultFactory.IsCompleteAsync(response))
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

			Serilog.Log.Logger.Information($"Redirecting to {redirectUri}");

			// re-directs should always be GET
            return await makeRequestAsync(authenticate, HttpMethod.Get, redirectUri);
        }
    }
}
