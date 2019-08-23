using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using Dinah.Core;

namespace AudibleApi
{
	public class ClientSharer : IClientSharer
	{
		private HttpMessageHandler _sharedMessageHandler { get; }

		public ClientSharer()
		{
			StackBlocker.ApiTestBlocker();

			var handler = new HttpClientHandler
			{
				// AllowAutoRedirect = false needed for DownloadAaxWorkaroundAsync and seems to do no hard anywhere else
				AllowAutoRedirect = false,
				AutomaticDecompression =
					DecompressionMethods.GZip |
					DecompressionMethods.Deflate
			};
			_sharedMessageHandler = handler;
		}
		public ClientSharer(HttpMessageHandler sharedMessageHandler)
		{
			_sharedMessageHandler = sharedMessageHandler ?? throw new ArgumentNullException(nameof(sharedMessageHandler));
		}

		private Dictionary<Uri, ISealedHttpClient> _sharedUrls { get; } = new Dictionary<Uri, ISealedHttpClient>();
		public ISealedHttpClient GetSharedClient(Uri uri)
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
