using Dinah.Core.Net.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi
{
	public partial class ApiUnauthenticated
	{
		public virtual bool IsAuthenticated => false;
		public IHttpClientSharer Sharer { get; }
		protected Locale _locale { get; }
		protected IHttpClientActions _client
			=> Sharer.GetSharedHttpClient(_locale.AudibleApiUri());
		public ApiUnauthenticated(Locale locale)
		{
			StackBlocker.ApiTestBlocker();
			_locale = locale ?? throw new ArgumentNullException(nameof(locale));
			Sharer = new HttpClientSharer();
		}

		public ApiUnauthenticated(Locale locale, IHttpClientSharer sharer)
		{
			_locale = locale ?? throw new ArgumentNullException(nameof(locale));
			Sharer = sharer ?? throw new ArgumentNullException(nameof(sharer));
		}

		public Task<HttpResponseMessage> AdHocNonAuthenticatedGetAsync(string requestUri)
			=> AdHocNonAuthenticatedGetAsync(requestUri, _client);

		public async Task<HttpResponseMessage> AdHocNonAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
		{
			if (requestUri is null)
				throw new ArgumentNullException(nameof(requestUri));
			if (string.IsNullOrWhiteSpace(requestUri))
				throw new ArgumentException($"{nameof(requestUri)} may not be blank");

			var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

			var response = await client.SendAsync(request);
			return response;
		}
	}
}
