using System.Net.Http;

namespace Dinah.Core.Net.Http
{
	public class SealedHttpClient : HttpClient, ISealedHttpClient
	{
		public SealedHttpClient(HttpMessageHandler handler) : base(handler) { }
	}
}
