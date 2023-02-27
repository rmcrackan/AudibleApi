using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Dinah.Core;
using Dinah.Core.Net.Http;

namespace AudibleApi
{
	public class HttpClientSharer : IHttpClientSharer
	{
		private HttpMessageHandler _sharedMessageHandler { get; }

		public HttpClientSharer()
		{
			StackBlocker.ApiTestBlocker();

			var handler = new HttpClientHandler
			{
				// AllowAutoRedirect = false needed for DownloadAaxWorkaroundAsync and seems to do no harm anywhere else
				AllowAutoRedirect = false,
				AutomaticDecompression =
					DecompressionMethods.GZip |
					DecompressionMethods.Deflate
			};
			_sharedMessageHandler = handler;
		}
		public HttpClientSharer(HttpMessageHandler sharedMessageHandler)
		{
			_sharedMessageHandler = ArgumentValidator.EnsureNotNull(sharedMessageHandler, nameof(sharedMessageHandler));
		}

		private Dictionary<Uri, IHttpClientActions> _sharedUrls { get; } = new Dictionary<Uri, IHttpClientActions>();
		public IHttpClientActions GetSharedHttpClient(string uri) => GetSharedHttpClient(new Uri(uri));
		public IHttpClientActions GetSharedHttpClient(Uri uri)
		{
			ArgumentValidator.EnsureNotNull(uri, nameof(uri));

			if (!_sharedUrls.ContainsKey(uri))
			{
				var wrappedHandler = new ApiMessageHandler { InnerHandler = _sharedMessageHandler };
				var client = new SealedHttpClient(wrappedHandler)
				{
					BaseAddress = uri,
					Timeout = new TimeSpan(0, 0, 30)
				};
				_sharedUrls[uri] = client;
			}

			return _sharedUrls[uri];
		}
	}
}
