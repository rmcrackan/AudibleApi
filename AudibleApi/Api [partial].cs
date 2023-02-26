using System;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using Dinah.Core;
using Dinah.Core.Net.Http;

namespace AudibleApi
{
    // when possible:
    // - return strongly-typed data
    // - throw strongly typed exceptions
    public partial class Api  : ApiUnauthenticated
	{
		public override bool IsAuthenticated => true;
		private IIdentityMaintainer _identityMaintainer { get; }

		public Api(IIdentityMaintainer identityMaintainer) 
			: base(identityMaintainer?.Locale)
		{
			_identityMaintainer = ArgumentValidator.EnsureNotNull(identityMaintainer, nameof(identityMaintainer));
		}

		public Api(IIdentityMaintainer identityMaintainer, IHttpClientSharer sharer) 
			: base(identityMaintainer?.Locale, sharer)
		{
			_identityMaintainer = ArgumentValidator.EnsureNotNull(identityMaintainer, nameof(identityMaintainer));
		}

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri)
			=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, Client);

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
			=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, client);

		public async Task<HttpResponseMessage> AdHocAuthenticatedRequestAsync(string requestUri, HttpMethod method, IHttpClientActions client, HttpBody requestBody = null)
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

			request.SignRequest(
					_identityMaintainer.SystemDateTime.UtcNow,
					await _identityMaintainer.GetAdpTokenAsync(),
					await _identityMaintainer.GetPrivateKeyAsync());

			return await SendClientRequest(client, request);
		}

		public async Task<HttpResponseMessage> AdHocAuthenticatedGetWithAccessTokenAsync(string requestUri, IHttpClientActions client)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(requestUri, nameof(requestUri));
			ArgumentValidator.EnsureNotNull(client, nameof(client));

			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

			var accessToken = await _identityMaintainer.GetAccessTokenAsync();

			request.Headers.Add("x-amz-access-token", accessToken.TokenValue);

			return await SendClientRequest(client, request);
		}
	}
}
