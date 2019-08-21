using System.Net.Http;

namespace BaseLib
{
    public class SealedHttpClient : HttpClient, ISealedHttpClient
    {
        public SealedHttpClient(HttpMessageHandler handler) : base(handler) { }
    }
}
