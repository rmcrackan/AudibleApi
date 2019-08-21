using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BaseLib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;

namespace TestCommon
{
    //
    // from: https://gingter.org/2018/07/26/how-to-mock-httpclient-in-your-net-c-unit-tests/
    //
    public static class HttpMock
    {
        // see:
        //   MoqExamples.complex_sequence
        // for
        //   Protected SetupSequence 

        public static Mock<HttpClientHandler> CreateMockHttpClientHandler(string returnContent, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage
            {
                Content = new StringContent(returnContent),
                StatusCode = statusCode
            };
            return CreateMockHttpClientHandler(response);
        }

        public static Mock<HttpClientHandler> CreateMockHttpClientHandler(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
               {
                   response.RequestMessage = request;
                   return response;
               })
               .Verifiable();
            handlerMock
               .Protected()
               .Setup(
                  "Dispose",
                  ItExpr.IsAny<bool>()
               )
               .Verifiable();

            return handlerMock;
        }

        public static Mock<HttpClientHandler> CreateMockHttpClientHandler(Action action)
        {
            var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
            handlerMock
               .Protected()
               .Setup<Task<HttpResponseMessage>>(
                  "SendAsync",
                  ItExpr.IsAny<HttpRequestMessage>(),
                  ItExpr.IsAny<CancellationToken>()
               )
               .ReturnsAsync((HttpRequestMessage request, CancellationToken token) =>
               {
                   action();
                   return new HttpResponseMessage();
               })
               .Verifiable();
            handlerMock
               .Protected()
               .Setup(
                  "Dispose",
                  ItExpr.IsAny<bool>()
               )
               .Verifiable();

            return handlerMock;
        }
    }
}
