using Dinah.Core.Net.Http;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
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

			return await SendClientRequest(client, request);
		}

		protected async Task<HttpResponseMessage> SendClientRequest(IHttpClientActions client, HttpRequestMessage request)
		{
			//https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.sendasync?view=net-6.0
			try
			{
				return await client.SendAsync(request);
			}
			catch (TaskCanceledException)
			{
				throw new ApiErrorException(request.RequestUri, new Newtonsoft.Json.Linq.JObject { { "http_error", "The request failed due to timeout." } });
			}
			catch (HttpRequestException)
			{
				throw new ApiErrorException(request.RequestUri, new Newtonsoft.Json.Linq.JObject { { "http_error", "The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout." } });
			}
		}
	}
}
