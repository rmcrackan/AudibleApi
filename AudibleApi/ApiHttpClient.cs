using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi
{
	public class ApiHttpClient : HttpClient, IHttpClient, IHttpClientActions
	{
		public CookieContainer CookieJar { get; } = new CookieContainer();

		private ApiHttpClient(HttpMessageHandler handler) : base(handler) { }

		public static ApiHttpClient Create()
		{
			StackBlocker.ApiTestBlocker();

			var innerHander = new HttpClientHandler
			{
				AllowAutoRedirect = false,
				AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
			};
			return create(innerHander);
		}

		public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			//https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.sendasync?view=net-6.0
			try
			{
				return await base.SendAsync(request, cancellationToken);
			}
			catch (TaskCanceledException ex)
			{
				if (ex.CancellationToken == cancellationToken)
					throw;

				throw new ApiErrorException(request.RequestUri, ex.ToJson("The request failed due to timeout."));
			}
			catch (HttpRequestException ex)
			{
				throw new ApiErrorException(request.RequestUri, ex.ToJson("The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout."));
			}
		}

		public static ApiHttpClient Create(HttpClientHandler innerHandler)
		{
			ArgumentValidator.EnsureNotNull(innerHandler, nameof(innerHandler));

			if (innerHandler.CookieContainer.ReflectOverAllCookies().Count > 0)
				throw new ArgumentException("Cannot use a client which already has cookies");

			return create(innerHandler);
		}

		private static ApiHttpClient create(HttpClientHandler innerHandler)
		{
			var handler = new ApiMessageHandler { InnerHandler = innerHandler };
			var client = new ApiHttpClient(handler);

			// manually handle 3xx redirects
			innerHandler.AllowAutoRedirect = false;
			innerHandler.CookieContainer = client.CookieJar;

			return client;
		}
	}
}
