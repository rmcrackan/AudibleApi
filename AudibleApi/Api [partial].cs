using AudibleApi.Authorization;
using AudibleApi.Common;
using AudibleApi.Cryptography;
using Dinah.Core;
using Dinah.Core.Net.Http;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi;

// when possible:
// - return strongly-typed data
// - throw strongly typed exceptions
public partial class Api : ApiUnauthenticated
{
	public override bool IsAuthenticated => true;
	private IIdentityMaintainer _identityMaintainer { get; }

	public Api(IIdentityMaintainer identityMaintainer)
		: this(identityMaintainer, storeLocale: null) { }

	/// <param name="storeLocale">
	/// The marketplace to read. Null means the identity's own marketplace. Pass another locale to read a
	/// library this same login holds under a different storefront; the tokens are unaffected either way.
	/// </param>
	public Api(IIdentityMaintainer identityMaintainer, Locale? storeLocale)
		: base(ArgumentValidator.EnsureNotNull(identityMaintainer?.Locale, nameof(identityMaintainer)), storeLocale)
	{
		_identityMaintainer = identityMaintainer!;
	}

	public Api(IIdentityMaintainer identityMaintainer, IHttpClientSharer sharer)
		: this(identityMaintainer, null, sharer) { }

	/// <param name="storeLocale">
	/// The marketplace to read. Null means the identity's own marketplace. Pass another locale to read a
	/// library this same login holds under a different storefront; the tokens are unaffected either way.
	/// </param>
	public Api(IIdentityMaintainer identityMaintainer, Locale? storeLocale, IHttpClientSharer sharer)
		: base(ArgumentValidator.EnsureNotNull(identityMaintainer?.Locale, nameof(identityMaintainer)), storeLocale, sharer)
	{
		_identityMaintainer = identityMaintainer!;
	}

	public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri)
		=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, Client);

	public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
		=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, client);

	public ApiAuthenticationMethod AuthenticationMethods { get; set; } = ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken;

	public async Task<HttpResponseMessage> AdHocAuthenticatedRequestAsync(string requestUri, HttpMethod method, IHttpClientActions client, HttpBody? requestBody = null)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(requestUri, nameof(requestUri));
		ArgumentValidator.EnsureNotNull(method, nameof(method));
		ArgumentValidator.EnsureNotNull(client, nameof(client));

		using var request = new HttpRequestMessage(method, requestUri);

		if (method.In(HttpMethod.Post, HttpMethod.Put))
		{
			ArgumentValidator.EnsureNotNull(requestBody, nameof(requestBody));
			request.AddContent(requestBody);
		}

		await ApplyAuthenticationAsync(request);

		return await SendClientRequest(client, request);
	}

	public async Task ApplyAuthenticationAsync(HttpRequestMessage request)
	{
		ArgumentValidator.EnsureNotNull(request, nameof(request));

		if (AuthenticationMethods.HasFlag(ApiAuthenticationMethod.AdpSignature))
		{
			var adpToken = await _identityMaintainer.GetAdpTokenAsync() ?? throw new InvalidOperationException("Identity maintainer ADP token is null");
			var privateKey = await _identityMaintainer.GetPrivateKeyAsync() ?? throw new InvalidOperationException("Identity maintainer private key is null");

			request.SignRequest(_identityMaintainer.SystemDateTime.UtcNow, adpToken, privateKey);
		}

		if (AuthenticationMethods.HasFlag(ApiAuthenticationMethod.BearerToken))
		{
			var accessToken = await _identityMaintainer.GetAccessTokenAsync();
			if (accessToken is not null && !accessToken.IsEmpty)
			{
				request.Headers.TryAddWithoutValidation("x-amz-access-token", accessToken.Reveal());
			}
			else if (!AuthenticationMethods.HasFlag(ApiAuthenticationMethod.AdpSignature))
			{
				throw new InvalidOperationException("Identity maintainer access token is null or empty");
			}
		}
	}

	public async Task<HttpResponseMessage> AdHocAuthenticatedGetWithAccessTokenAsync(string requestUri, IHttpClientActions client)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(requestUri, nameof(requestUri));
		ArgumentValidator.EnsureNotNull(client, nameof(client));

		var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		var accessToken = await _identityMaintainer.GetAccessTokenAsync() ?? throw new InvalidOperationException("Identity maintainer access token is null");
		if (accessToken.IsEmpty)
			throw new InvalidOperationException("Identity maintainer access token is empty");

		request.Headers.Add("x-amz-access-token", accessToken.Reveal());

		return await SendClientRequest(client, request);
	}

	public async Task<bool> HasRequiredTokensAsync(ApiAuthenticationMethod methods)
	{
		var defined = ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken;
		if ((methods & ~defined) != 0)
			return false;

		if (methods.HasFlag(ApiAuthenticationMethod.AdpSignature))
		{
			if (await _identityMaintainer.GetAdpTokenAsync() is null || await _identityMaintainer.GetPrivateKeyAsync() is null)
				return false;
		}

		if (methods.HasFlag(ApiAuthenticationMethod.BearerToken))
		{
			var accessToken = await _identityMaintainer.GetAccessTokenAsync();
			if (accessToken is null || accessToken.IsEmpty)
				return false;
		}

		return true;
	}
}
