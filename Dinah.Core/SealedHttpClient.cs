using System.Net.Http;

namespace Dinah.Core
{
    public class SealedHttpClient : HttpClient, ISealedHttpClient
    {
        public SealedHttpClient(HttpMessageHandler handler) : base(handler) { }
    }
}
