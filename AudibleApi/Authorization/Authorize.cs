﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

// OAuth is authorization, not authentication
//
// from: https://auth0.com/docs/api-auth/why-use-access-tokens-to-secure-apis
// "OpenID Connect tells you who somebody is. OAuth 2.0 tells you what
// somebody is allowed to do.
//
// OAuth 2.0 is used to grant authorization. It allows you to authorize
// Web App A access to your information from Web App B without requiring
// you to share your credentials. OAuth 2.0 was built with only
// authorization in mind and doesn't include any authentication mechanisms.
// In other words, OAuth 2.0 doesn't give the Authorization Server any way
// of verifying who the user is.
//
// OpenID Connect builds on OAuth 2.0. It enables you, as the user, to
// verify your identity and to give some basic profile information without
// sharing your credentials."
namespace AudibleApi.Authorization
{
	/// <summary>
	/// Directly calls amazon API.
	/// Static, stateless, no replace logic, no storage
	/// </summary>
	public class Authorize : IAuthorize
	{
		private const string iosVersion = "12.3.1";
		private const string appVersion = "3.7";
		private const string appName = "Audible";

		private ISystemDateTime _systemDateTime { get; }
		private Locale _locale { get; }

		private IHttpClientSharer _sharer { get; }
		private IHttpClientActions _client
			=> _sharer.GetSharedHttpClient(_locale.AmazonApiUri());

		public Authorize(Locale locale) : this(locale, new HttpClientSharer(), new SystemDateTime())
			=> StackBlocker.ApiTestBlocker();

		public Authorize(Locale locale, IHttpClientSharer sharer, ISystemDateTime systemDateTime)
		{
			_sharer = sharer ?? throw new ArgumentNullException(nameof(sharer));
			_systemDateTime = systemDateTime ?? throw new ArgumentNullException(nameof(systemDateTime));
			_locale = locale ?? throw new ArgumentNullException(nameof(locale));
		}

		public async Task<JObject> RegisterAsync(AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			if (accessToken is null)
				throw new ArgumentNullException(nameof(accessToken));

			if (cookies is null)
				throw new ArgumentNullException(nameof(cookies));

			if (!cookies.Any())
				throw new ArgumentException("Cookies may not be empty", nameof(cookies));

			try
			{
				var request = buildRegisterRequest(_locale, accessToken, cookies);
				var response = await _client.SendAsync(request);

				response.EnsureSuccessStatusCode();

				var postResponseJObj = await response.Content.ReadAsJObjectAsync();
				return postResponseJObj;
			}
			catch (Exception ex)
			{
				throw new RegistrationException("Could not register", ex);
			}
		}

		private static HttpRequestMessage buildRegisterRequest(Locale locale, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			var jsonBody = buildRegisterBody(locale, accessToken, cookies);
			var cookiesAggregated = cookies
				.Select(kvp => $"{kvp.Key}={kvp.Value}")
				.Aggregate((a, b) => $"{a}; {b}");

			// post directly. no redirects
			// https://stackoverflow.com/a/10679340
			var request = new HttpRequestMessage(HttpMethod.Post, "/auth/register");

			request.AddContent(jsonBody);
			request.Headers.Add("Host", locale.AmazonApiUri().Host);
			// https://stackoverflow.com/a/10679340
			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Add("Accept-Charset", "utf-8");
			request.Headers.Add("x-amzn-identity-auth-domain", locale.AmazonApiUri().Host);
			request.Headers.Add("Accept", "application/json");
			request.Headers.TryAddWithoutValidation("User-Agent", $"AmazonWebView/{appName}/{appVersion}/iOS/{iosVersion}/iPhone");
			request.Headers.Add("Accept-Language", "en_US");
			request.Headers.Add("Cookie", cookiesAggregated);

			return request;
		}

		// do not use Dictionary<string, string> for cookies b/c of duplicates
		private static JObject buildRegisterBody(Locale locale, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			// for dynamic, add nuget ref Microsoft.CSharp
			// https://www.newtonsoft.com/json/help/html/CreateJsonDynamic.htm
			dynamic bodyJson = new JObject();
			bodyJson.requested_token_type = new JArray("bearer", "mac_dms", "website_cookies");
			bodyJson.cookies = new JObject();
			bodyJson.cookies.domain = locale.RegisterDomain();

			var kvpSelect = cookies.Select(kvp =>
			{
				dynamic obj = new JObject();
				obj.Name = kvp.Key;
				obj.Value = kvp.Value;
				return obj;
			});
			bodyJson.cookies.website_cookies = new JArray(kvpSelect);

			var serial = getRandomDeviceSerial();

			bodyJson.registration_data = new JObject();
			bodyJson.registration_data.domain = "Device";
			bodyJson.registration_data.app_version = appVersion;
			bodyJson.registration_data.device_serial = serial;
			bodyJson.registration_data.device_type = "A2CZJZGLK2JJVM";
			bodyJson.registration_data.device_name = "%FIRST_NAME%%FIRST_NAME_POSSESSIVE_STRING%%DUPE_STRATEGY_1ST%Audible for iPhone";
			bodyJson.registration_data.os_version = iosVersion;
			bodyJson.registration_data.device_model = "iPhone";
			bodyJson.registration_data.app_name = appName;
			bodyJson.auth_data = new JObject();
			bodyJson.auth_data.access_token = accessToken.TokenValue;
			bodyJson.requested_extensions = new JArray("device_info", "customer_info");

			return bodyJson;
		}

		static char[] chars { get; } = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
		static Random random { get; } = new Random();
		/// <summary>
		/// Generates a random text (str + int) with a length of 40 chars.
		/// Use of random serial prevents unregister device by other users with same 'device_serial'
		/// </summary>
		/// <returns></returns>
		private static string getRandomDeviceSerial()
		{
			var serialBuilder = new System.Text.StringBuilder();
			for (var i = 0; i < 40; i++)
			{
				var r = random.Next(0, chars.Length);
				var c = chars[r];
				serialBuilder.Append(c);
			}
			var serial = serialBuilder.ToString();
			return serial;
		}

		public async Task<bool> DeregisterAsync(AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			if (accessToken is null)
				throw new ArgumentNullException(nameof(accessToken));

			if (cookies is null)
				throw new ArgumentNullException(nameof(cookies));

			try
			{
				var request = buildDeregisterRequest(_locale, accessToken, cookies);
				var response = await _client.SendAsync(request);

				return response.StatusCode == HttpStatusCode.OK;
			}
			catch (Exception ex)
			{
				throw new RegistrationException("Could not deregister", ex);
			}
		}

		private static HttpRequestMessage buildDeregisterRequest(Locale locale, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			var cookiesAggregated = cookies
				.Select(kvp => $"{kvp.Key}={kvp.Value}")
				.Aggregate((a, b) => $"{a}; {b}");

			var request = new HttpRequestMessage(HttpMethod.Post, "/auth/deregister");

			request.AddContent(JObject.Parse("{ 'deregister_all_existing_accounts' : true }"));
			request.Headers.Add("Host", locale.AmazonApiUri().Host);
			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Add("Accept-Charset", "utf-8");
			request.Headers.Add("x-amzn-identity-auth-domain", locale.AmazonApiUri().Host);
			request.Headers.Add("Accept", "application/json");
			request.Headers.TryAddWithoutValidation("User-Agent", $"AmazonWebView/{appName}/{appVersion}/iOS/{iosVersion}/iPhone");
			request.Headers.Add("Accept-Language", "en_US");
			request.Headers.Add("Cookie", cookiesAggregated);
			request.Headers.Add("Authorization", $"Bearer {accessToken.TokenValue}");

			return request;
		}

		/// <summary>Refresh access token. Access tokens are valid for 60 min</summary>
		public async Task<AccessToken> RefreshAccessTokenAsync(RefreshToken refresh_token)
		{
			if (refresh_token is null)
				throw new ArgumentNullException(nameof(refresh_token));

			var response = await requestTokenRefreshAsync(refresh_token);
			var accessToken = await ExtractAccessTokenAsync(response);
			return accessToken;
		}

		private async Task<HttpResponseMessage> requestTokenRefreshAsync(RefreshToken refresh_token)
		{
			var body = new Dictionary<string, string>
			{
				["app_name"] = appName,
				["app_version"] = appVersion,
				["source_token"] = refresh_token.Value,
				["requested_token_type"] = "access_token",
				["source_token_type"] = "refresh_token"
			};
			var request = new HttpRequestMessage(HttpMethod.Post, "/auth/token");
			request.Headers.Add("x-amzn-identity-auth-domain", _locale.AmazonApiUri().Host);
			request.AddContent(body);

			var response = await _client.SendAsync(request);
			return response;
		}

		public async Task<AccessToken> ExtractAccessTokenAsync(HttpResponseMessage response)
		{
			if (response is null)
				throw new ArgumentNullException(nameof(response));

			var responseBody = await response.Content.ReadAsStringAsync();

			var authRegisterJson = JObject.Parse(responseBody);
			var accessTokenStr = authRegisterJson["access_token"].ToString();
			var expires = int.Parse(authRegisterJson["expires_in"].ToString());

			var expiresDateTime = _systemDateTime.UtcNow.AddSeconds(expires);

			var accessToken = new AccessToken(accessTokenStr, expiresDateTime);
			return accessToken;
		}
	}
}
