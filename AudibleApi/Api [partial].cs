using System;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authorization;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;

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
			_identityMaintainer = identityMaintainer ?? throw new ArgumentNullException(nameof(identityMaintainer));
		}

		public Api(IIdentityMaintainer identityMaintainer, IHttpClientSharer sharer) 
			: base(identityMaintainer?.Locale, sharer)
		{
			_identityMaintainer = identityMaintainer ?? throw new ArgumentNullException(nameof(identityMaintainer));
		}

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri)
			=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, Client);

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
			=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, client);

		public async Task<HttpResponseMessage> AdHocAuthenticatedRequestAsync(string requestUri, HttpMethod method, IHttpClientActions client, JObject requestBody = null)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (string.IsNullOrWhiteSpace(requestUri))
				throw new ArgumentException($"{nameof(requestUri)} may not be blank");
			if (method is null)
				throw new ArgumentNullException(nameof(method));
			if (requestBody is null && (method.Method == HttpMethod.Post.Method || method.Method == HttpMethod.Put.Method))
				throw new ArgumentNullException(nameof(requestBody), $"Must provide request body content when using the {method.Method} {nameof(HttpMethod)}");

			var request = new HttpRequestMessage(method, requestUri);

			if (method.Method == HttpMethod.Post.Method || method.Method == HttpMethod.Put.Method)
				request.AddContent(requestBody);

			request.SignRequest(
					_identityMaintainer.SystemDateTime.UtcNow,
					await _identityMaintainer.GetAdpTokenAsync(),
					await _identityMaintainer.GetPrivateKeyAsync());

			return await SendClientRequest(client, request);
		}

		public async Task<HttpResponseMessage> AdHocAuthenticatedGetWithAccessTokenAsync(string requestUri, IHttpClientActions client)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (string.IsNullOrWhiteSpace(requestUri))
				throw new ArgumentException($"{nameof(requestUri)} may not be blank");

			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

			var accessToken = await _identityMaintainer.GetAccessTokenAsync();

			request.Headers.Add("x-amz-access-token", accessToken.TokenValue);

			return await SendClientRequest(client, request);
		}
	}
}
