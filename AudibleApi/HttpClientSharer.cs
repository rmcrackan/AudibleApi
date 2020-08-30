using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Dinah.Core;
using Dinah.Core.Net;
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
			_sharedMessageHandler = sharedMessageHandler ?? throw new ArgumentNullException(nameof(sharedMessageHandler));
		}

		private Dictionary<Uri, ISealedHttpClient> _sharedUrls { get; } = new Dictionary<Uri, ISealedHttpClient>();
		public ISealedHttpClient GetSharedHttpClient(Uri uri)
		{
			if (uri is null)
				throw new ArgumentNullException(nameof(uri));

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
