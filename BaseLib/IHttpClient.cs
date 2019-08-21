using System;
using System.Net;
using System.Net.Http.Headers;

namespace BaseLib
{
	public interface IHttpClient : ISealedHttpClient
	{
		CookieContainer CookieJar { get; }
		HttpRequestHeaders DefaultRequestHeaders { get; }
		Uri BaseAddress { get; set; }
		long MaxResponseContentBufferSize { get; set; }
		TimeSpan Timeout { get; set; }
	}
}
