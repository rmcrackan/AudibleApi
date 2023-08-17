using System.Reflection;

namespace TestAudibleApiCommon
{
    public static class NSubstituteExtensions
    {
        public static object ProtectedMethod<T>(this T value, string methodName, params object[] parameters)
            => value
            .GetType()
            .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(value, parameters);

        /*
        protected property:

        var mock = Substitute.For<MyExampleClass>();
        mock
            .GetType()
            .GetProperty("Property1", BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(mock, null)
            .Returns("Hello World");
         */
    }

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
                .ProtectedMethod("SendAsync", Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    response.RequestMessage = (HttpRequestMessage)x[0];
                    return Task.FromResult(response);
                });
            return handlerMock;
        }
        
        public static HttpClientHandler CreateMockHttpClientHandler(Action action)
        {
            var handlerMock = Substitute.For<HttpClientHandler>();
            handlerMock
                .ProtectedMethod("SendAsync", Arg.Any<HttpRequestMessage>(), Arg.Any<CancellationToken>())
                .Returns(x =>
                {
                    action();
                    return Task.FromResult(new HttpResponseMessage());
                });
            return handlerMock;
        }

        public static HttpClientHandler GetHandler(string handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
            => CreateMockHttpClientHandler(handlerReturnString ?? "foo", statusCode);

        public static HttpClientHandler GetHandler(HttpResponseMessage response)
            => CreateMockHttpClientHandler(response);
    }
}
