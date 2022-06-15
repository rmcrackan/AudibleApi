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
		protected Locale Locale { get; }
		protected IHttpClientActions Client
			=> Sharer.GetSharedHttpClient(Locale.AudibleApiUri());

		public ApiUnauthenticated(Locale locale)
		{
			StackBlocker.ApiTestBlocker();
			Locale = locale ?? throw new ArgumentNullException(nameof(locale));
			Sharer = new HttpClientSharer();
		}

		public ApiUnauthenticated(Locale locale, IHttpClientSharer sharer)
		{
			Locale = locale ?? throw new ArgumentNullException(nameof(locale));
			Sharer = sharer ?? throw new ArgumentNullException(nameof(sharer));
		}

		public Task<HttpResponseMessage> AdHocNonAuthenticatedGetAsync(string requestUri)
			=> AdHocNonAuthenticatedGetAsync(requestUri, Client);

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
