using System;
using System.Net;
using System.Net.Http;
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
		public static ApiHttpClient Create(HttpClientHandler innerHandler)
		{
			if (innerHandler is null)
				throw new ArgumentNullException(nameof(innerHandler));

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
