using AudibleApi.Authorization;
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
		: base(ArgumentValidator.EnsureNotNull(identityMaintainer?.Locale, nameof(identityMaintainer)))
	{
		_identityMaintainer = identityMaintainer;
	}

	public Api(IIdentityMaintainer identityMaintainer, IHttpClientSharer sharer)
		: base(ArgumentValidator.EnsureNotNull(identityMaintainer?.Locale, nameof(identityMaintainer)), sharer)
	{
		_identityMaintainer = identityMaintainer;
	}

	public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri)
		=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, Client);

	public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
		=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, client);

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
		var adpToken = await _identityMaintainer.GetAdpTokenAsync() ?? throw new InvalidOperationException("Identity maintainer ADP token is null");
		var privateKey = await _identityMaintainer.GetPrivateKeyAsync() ?? throw new InvalidOperationException("Identity maintainer private key is null");

		request.SignRequest(_identityMaintainer.SystemDateTime.UtcNow, adpToken, privateKey);

		return await SendClientRequest(client, request);
	}

	public async Task<HttpResponseMessage> AdHocAuthenticatedGetWithAccessTokenAsync(string requestUri, IHttpClientActions client)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(requestUri, nameof(requestUri));
		ArgumentValidator.EnsureNotNull(client, nameof(client));

		var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		var accessToken = await _identityMaintainer.GetAccessTokenAsync() ?? throw new InvalidOperationException("Identity maintainer access token is null");

		request.Headers.Add("x-amz-access-token", accessToken.TokenValue);

		return await SendClientRequest(client, request);
	}
}
