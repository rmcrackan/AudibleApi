using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace TestCommon
{
    //
    // from: https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/
    //
    public class ReturnType
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }
    public class MyTestClass
    {
        private HttpClient _client { get; }
        public MyTestClass(HttpClient client) => _client = client;

        public async Task<ReturnType> GetSomethingRemoteAsync(string path)
        {
            var message = await _client.SendAsync(new HttpRequestMessage(HttpMethod.Get, path));
            var str = await message.Content.ReadAsStringAsync();
            var objects = Newtonsoft.Json.JsonConvert.DeserializeObject<ReturnType>(str);
            return objects;
        }
    }
    [TestClass]
    public class HttpMockExample
    {
        [TestMethod]
        public async Task example()
        {
            var handlerMock = HttpMock.CreateMockHttpClientHandler("{'id':1,'value':'1'}");

            // use real http client with mocked handler here
            var httpClient = new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("http://test.com/")
            };
            var subjectUnderTest = new MyTestClass(httpClient);

            // ACT
            var result = await subjectUnderTest.GetSomethingRemoteAsync("api/test/whatever");

            // ASSERT
            result.Should().NotBeNull();
            result.Id.Should().Be(1);

            // also check the 'http' call was like we expected it
            var expectedUri = new Uri("http://test.com/api/test/whatever");

            handlerMock.Protected().Verify(
               "SendAsync",
               Times.Exactly(1), // we expected a single external request
               ItExpr.Is<HttpRequestMessage>(req =>
                  req.Method == HttpMethod.Get  // we expected a GET request
                  && req.RequestUri == expectedUri // to this uri
               ),
               ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
