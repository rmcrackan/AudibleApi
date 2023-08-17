using System.Reflection;

namespace TestAudibleApiCommon
{
    //
    // from: https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/
    //
    public static class HttpMock
    {
        public static HttpClientHandler CreateMockHttpClientHandler(string returnContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage
            {
                Content = new StringContent(returnContent),
                StatusCode = statusCode
            };
            return CreateMockHttpClientHandler(response);
        }

        public static HttpClientHandler CreateMockHttpClientHandler(HttpResponseMessage response)
        {
            var handlerMock = Substitute.For<HttpClientHandler>();
            handlerMock
                .GetType()
                .GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(handlerMock, new object[] { Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>() })
                .Returns(x =>
                {
                    HttpRequestMessage request = (HttpRequestMessage)x[0];
                    CancellationToken token = (CancellationToken)x[1];
                    response.RequestMessage = request;
                    return Task.FromResult(response);
                });
            return handlerMock;
        }

        public static HttpClientHandler CreateMockHttpClientHandler(Action action)
        {
            var handlerMock = Substitute.For<HttpClientHandler>();
            handlerMock
                .GetType()
                .GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(handlerMock, new object[] { Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>() })
                .Returns(x =>
                {
                    action();
                    return Task.FromResult(new HttpResponseMessage());
                });
            return handlerMock;
        }

        public static HttpClientHandler GetHandler(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
             => CreateMockHttpClientHandler
                (
                handlerReturnString ?? "foo",
                statusCode
                );

        public static HttpClientHandler GetHandler(HttpResponseMessage response)
            => CreateMockHttpClientHandler(response);
    }
}
