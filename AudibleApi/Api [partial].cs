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
    public partial class Api
    {
		public IHttpClientSharer Sharer { get; }
        private IIdentityMaintainer _identityMaintainer { get; }

        private ISealedHttpClient _client
            => Sharer.GetSharedHttpClient(_identityMaintainer.Locale.AudibleApiUri());

		public Api(IIdentityMaintainer identityMaintainer)
		{
			StackBlocker.ApiTestBlocker();

			_identityMaintainer = identityMaintainer ?? throw new ArgumentNullException(nameof(identityMaintainer));
			Sharer = new HttpClientSharer();
		}

		public Api(IIdentityMaintainer identityMaintainer, IHttpClientSharer sharer)
		{
			_identityMaintainer = identityMaintainer ?? throw new ArgumentNullException(nameof(identityMaintainer));
			Sharer = sharer ?? throw new ArgumentNullException(nameof(sharer));
		}

		public async Task<JObject> AdHocNonAuthenticatedGetAsync(string requestUri)
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));
            if (string.IsNullOrWhiteSpace(requestUri))
                throw new ArgumentException($"{nameof(requestUri)} may not be blank");

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await _client.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();
            return JObject.Parse(responseString);
        }

		public Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri)
			=> AdHocAuthenticatedGetAsync(requestUri, _client);

		private async Task<HttpResponseMessage> AdHocAuthenticatedGetAsync(string requestUri, ISealedHttpClient client)
        {
            if (requestUri is null)
                throw new ArgumentNullException(nameof(requestUri));
            if (string.IsNullOrWhiteSpace(requestUri))
                throw new ArgumentException($"{nameof(requestUri)} may not be blank");

            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            await signRequestAsync(request);

            var response = await client.SendAsync(request);
            return response;
        }

        private async Task signRequestAsync(HttpRequestMessage request)
			=> request.SignRequest(
				_identityMaintainer.SystemDateTime.UtcNow,
				await _identityMaintainer.GetAdpTokenAsync(),
				await _identityMaintainer.GetPrivateKeyAsync());
	}
}
