using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
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
		private ISystemDateTime _systemDateTime { get; }
		private Locale _locale { get; }

		private IHttpClientSharer _sharer { get; }
		private IHttpClientActions _client
			=> _sharer.GetSharedHttpClient(_locale.RegistrationUri());

		public Authorize(Locale locale) : this(locale, new HttpClientSharer(), new SystemDateTime())
			=> StackBlocker.ApiTestBlocker();

		public Authorize(Locale locale, IHttpClientSharer sharer, ISystemDateTime systemDateTime)
		{
			_sharer = sharer ?? throw new ArgumentNullException(nameof(sharer));
			_systemDateTime = systemDateTime ?? throw new ArgumentNullException(nameof(systemDateTime));
			_locale = locale ?? throw new ArgumentNullException(nameof(locale));
		}

		public async Task<JObject> RegisterAsync(OAuth2 authorization)
		{
			ArgumentValidator.EnsureNotNull(authorization, nameof(authorization));

			try
			{
				var regUri = new Uri(_locale.RegistrationUri(), "/auth/register");
				var content = buildRegisterBody(_locale, authorization);
				var response = await new HttpClient().PostAsync(regUri, content.Content);

				response.EnsureSuccessStatusCode();

				var postResponseJObj = await response.Content.ReadAsJObjectAsync();
				return postResponseJObj;
			}
			catch (Exception ex)
			{
				throw new RegistrationException("Could not register", ex);
			}
		}

		private static HttpBody buildRegisterBody(Locale locale, OAuth2 authorization)
		{
			// for dynamic, add nuget ref Microsoft.CSharp
			// https://www.newtonsoft.com/json/help/html/CreateJsonDynamic.htm
			dynamic bodyJson = new JObject();
			bodyJson.requested_token_type = new JArray("bearer", "mac_dms", "website_cookies", "store_authentication_cookie");

			bodyJson.cookies = new JObject();
			bodyJson.cookies.website_cookies = new JArray();
			bodyJson.cookies.domain = locale.RegisterDomain();

			bodyJson.registration_data = new JObject();
			bodyJson.registration_data.domain = "Device";
			bodyJson.registration_data.app_version = Resources.AppVersion;
			bodyJson.registration_data.device_serial = authorization.DeviceSerialNumber;
			bodyJson.registration_data.device_type = Resources.DeviceType;
			bodyJson.registration_data.device_name = Resources.DeviceName;
			bodyJson.registration_data.os_version = Resources.IosVersion;
			bodyJson.registration_data.software_version = Resources.SoftwareVersion;
			bodyJson.registration_data.device_model = Resources.DeviceModel;
			bodyJson.registration_data.app_name = Resources.AppName;

			bodyJson.auth_data = new JObject();
			bodyJson.auth_data.client_id = Resources.build_client_id(authorization.DeviceSerialNumber);
			bodyJson.auth_data.authorization_code = authorization.Code;
			bodyJson.auth_data.code_verifier = authorization.CodeVerifier;
			bodyJson.auth_data.code_algorithm = "SHA-256";
			bodyJson.auth_data.client_domain = "DeviceLegacy";

			bodyJson.requested_extensions = new JArray("device_info", "customer_info");

			return bodyJson;
		}

		public async Task<bool> DeregisterAsync(AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			ArgumentValidator.EnsureNotNull(accessToken, nameof(accessToken));

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
			var request = new HttpRequestMessage(HttpMethod.Post, "/auth/deregister");

			request.AddContent(JObject.Parse("{ 'deregister_all_existing_accounts' : true }"));
			request.Headers.Add("Host", locale.RegistrationUri().Host);
			request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			request.Headers.Add("Accept-Charset", "utf-8");
			request.Headers.Add("x-amzn-identity-auth-domain", locale.RegistrationUri().Host);
			request.Headers.Add("Accept", "application/json");
			request.Headers.TryAddWithoutValidation("User-Agent", Resources.User_Agent);
			request.Headers.Add("Accept-Language", "en_US");
			request.Headers.Add("Authorization", $"Bearer {accessToken.TokenValue}");

			if (cookies is not null && cookies.Any())
			{
				var cookiesAggregated = cookies
					.Select(kvp => $"{kvp.Key}={kvp.Value}")
					.Aggregate((a, b) => $"{a}; {b}");
				request.Headers.Add("Cookie", cookiesAggregated);
			}

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
				["app_name"] = Resources.AppName,
				["app_version"] = Resources.AppVersion,
				["source_token"] = refresh_token.Value,
				["requested_token_type"] = "access_token",
				["source_token_type"] = "refresh_token"
			};
			var request = new HttpRequestMessage(HttpMethod.Post, "/auth/token");
			request.Headers.Add("x-amzn-identity-auth-domain", _locale.RegistrationUri().Host);
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
