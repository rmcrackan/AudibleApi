using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Logging;

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

			var response = await makeRequestAsync(authenticate, HttpMethod.Get, new Uri(url));

			return await getResultsPageAsync(authenticate, new Dictionary<string, string>(), response);
		}

		public enum SignInPage { Signin , MFA }
		public static async Task<LoginResult> GetResultsPageAsync(Authenticate authenticate, Dictionary<string, string> inputs, SignInPage signInPage = SignInPage.Signin)
		{
			var url = signInPage switch
			{
				SignInPage.Signin => "/ap/signin",
				SignInPage.MFA => "/ap/mfa",
				_ => throw new ArgumentException(message: "Invalid signInPage value", paramName: nameof(signInPage)),
			};

			return await GetResultsPageAsync(authenticate, inputs, url);
		}
		private static async Task<LoginResult> GetResultsPageAsync(Authenticate authenticate, Dictionary<string, string> inputs, string url)
		{
			if (authenticate is null)
				throw new ArgumentNullException(nameof(authenticate));
			if (inputs is null)
				throw new ArgumentNullException(nameof(inputs));

			// uses client to make the POST request
			var response = await makeRequestAsync(
				authenticate,
				HttpMethod.Post,
				new Uri(authenticate.Locale.AmazonLoginUri(), url),
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
			loginFailedException.SetFile($"{nameof(LoginFailedException)}_ResponseBody_{DateTime.Now.Ticks}.tmp", body);

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
			Serilog.Log.Logger.Information("Send request {@DebugInfo}", new {
				method = method.Method,
				uri,
				absoluteUri
					= !uri.IsAbsoluteUri ? "[relative]"
					: uri.ToString() == uri.AbsoluteUri ? "[== uri]"
					: uri.AbsoluteUri
			});
			ArgumentValidator.EnsureNotNull(uri, nameof(uri));

			#region debug: enumerate pre-call cookies
			if (Serilog.Log.Logger.IsDebugEnabled())
			{
				try
				{
					Serilog.Log.Logger.Debug("Cookies before request. {@DebugInfo}", authenticate.LoginClient.CookieJar.Debug_GetCookies(authenticate.Locale.AmazonLoginUri()));
				}
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "pre-call cookies debug failure");
				}
			}
			#endregion

			HttpRequestMessage request;
			try
			{
				request = new HttpRequestMessage()
				{
					Method = method,
					RequestUri = uri,
					Content = content
				};
			}
			catch (Exception ex)
			{
				var contentLength = "null";
				if (content is not null)
					contentLength = (await content.ReadAsStringAsync()).Length.ToString();

				Serilog.Log.Logger.Error(ex, "Error constructing request message. {@DebugInfo}", contentLength);
				throw;
			}

			var response = await authenticate.LoginClient.SendAsync(request);

			#region debug: enumerate post-call cookies
			if (Serilog.Log.Logger.IsDebugEnabled())
			{
				try
				{
					Serilog.Log.Logger.Debug("Cookies after request. {@DebugInfo}", authenticate.LoginClient.CookieJar.Debug_GetCookies(authenticate.Locale.AmazonLoginUri()));
				}
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "post-call cookies debug failure");
				}
			}
			#endregion

			#region debug: request/response details
			if (Serilog.Log.Logger.IsDebugEnabled())
			{
				try
				{
					var requestContentLength = "null";
					if (content is not null)
						requestContentLength = (await content.ReadAsStringAsync()).Length.ToString();

					var responseContentLength = "null";
					if (response?.Content is not null)
						responseContentLength = (await response.Content.ReadAsStringAsync()).Length.ToString();

					Serilog.Log.Logger.Debug("Request/Response details. {@DebugInfo}", new {
						requestUri_fromResponse = response.RequestMessage.RequestUri.AbsoluteUri,
						requestHeaders = response.RequestMessage.Headers,
						requestContentLength,
						responseCode = $"{response.StatusCode}={(int)response.StatusCode}",
						responseHeaders = response.Headers,
						responseContentLength
					});
				}
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "request/response details debug failure");
				}
			}
			#endregion

			#region VERBOSE debugging only: save page content
			if (Serilog.Log.Logger.IsVerboseEnabled())
			{
				try
				{
					var body = await response?.Content?.ReadAsStringAsync();
					if (body is null)
						Serilog.Log.Logger.Verbose("Response body is null");
					else
					{
						// save inputs
						var newInputs = HtmlHelper.GetInputs(body);
						var responseInputs = getSanitizedInputs(newInputs);
						Serilog.Log.Logger.Verbose("ResponseInputFields: {@DebugInfo}", responseInputs);

						// save page content
						var filename = $"Verbose_ResponseBody_{DateTime.Now.Ticks}.tmp";
						var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Libation", "Verbose", filename);
						System.IO.File.WriteAllText(path, body);
					}
				}
				catch (Exception ex)
				{
					Serilog.Log.Error(ex, "save page content verbose-debug failure");
				}
			}
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
