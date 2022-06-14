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
			: base(identityMaintainer?.Locale ?? throw new ArgumentNullException(nameof(identityMaintainer)))
		{
			_identityMaintainer = identityMaintainer;
		}

		public Api(IIdentityMaintainer identityMaintainer, IHttpClientSharer sharer) 
			: base(identityMaintainer?.Locale ?? throw new ArgumentNullException(nameof(identityMaintainer)), sharer)
		{
			_identityMaintainer = identityMaintainer;
		}

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri)
			=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, _client);

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
			=> AdHocAuthenticatedRequestAsync(requestUri, HttpMethod.Get, client);

		public async Task<HttpResponseMessage> AdHocAuthenticatedRequestAsync(string requestUri, HttpMethod method, IHttpClientActions client, JObject postData = null)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (string.IsNullOrWhiteSpace(requestUri))
				throw new ArgumentException($"{nameof(requestUri)} may not be blank");
			if (method is null)
				throw new ArgumentNullException(nameof(method));
			if (method.Method == HttpMethod.Post.Method && postData is null)
				throw new ArgumentNullException(nameof(postData), $"Must provide post data when using {nameof(HttpMethod)}.{nameof(HttpMethod.Post)}");

			var request = new HttpRequestMessage(method, requestUri);

			if (method.Method == HttpMethod.Post.Method)
				request.AddContent(postData);

			request.SignRequest(
					_identityMaintainer.SystemDateTime.UtcNow,
					await _identityMaintainer.GetAdpTokenAsync(),
					await _identityMaintainer.GetPrivateKeyAsync());

			var response = await client.SendAsync(request);
			return response;
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

			var response = await client.SendAsync(request);
			return response;
		}
	}
}
