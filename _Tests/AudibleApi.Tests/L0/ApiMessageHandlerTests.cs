using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using TestAudibleApiCommon;
using TestCommon;

namespace ApiMessageHandlerTests
{
    [TestClass]
    public class ProcessResponse
    {
        // only a few tests are needed for basic return-string checking. the RestMessageValidatorTests do the heavy checking
        [TestMethod]
        public async Task should_be_processed_by_inner_handler()
        {
            // all of the important stuff goes into the InnerHandler
            var _200 = HttpStatusCode.OK;
            var message = "{'foo':'bar'}";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            var innerHandler = HttpMock.CreateMockHttpClientHandler(message, _200).Object;

            var apiMessageHandler = new ApiMessageHandler(innerHandler);
            var client = new HttpClient(apiMessageHandler);

            var response = await client.SendAsync(request);
            response.StatusCode.Should().Be(_200);
            (await response.Content.ReadAsStringAsync()).Should().Be(message);
        }

        [TestMethod]
        public async Task throws_NotAuthenticatedException()
        {
            // all of the important stuff goes into the InnerHandler
            var _401 = HttpStatusCode.Unauthorized;
            var message = "{'message':'Message could not be authenticated'}";
            var request = new HttpRequestMessage(HttpMethod.Get, "http://test.com");
            var innerHandler = HttpMock.CreateMockHttpClientHandler(message, _401).Object;

            var apiMessageHandler = new ApiMessageHandler(innerHandler);
            var client = new HttpClient(apiMessageHandler);

            await Assert.ThrowsExceptionAsync<NotAuthenticatedException>(async () => await client.SendAsync(request));
        }

		// - custom length
		// - records whether stream/string was requested
		class testContent : HttpContent
		{
			public bool serializedWasCalled = false;
			long _length { get; }
			public testContent() { }
			public testContent(long length) => _length = length;
			protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
			{
				var byteArray = System.Text.Encoding.ASCII.GetBytes("test");
				new MemoryStream(byteArray).CopyTo(stream);

				serializedWasCalled = true;
				var tcs = new TaskCompletionSource<object>();
				tcs.SetResult(null);
				return tcs.Task;
			}
			protected override bool TryComputeLength(out long length)
			{
				length = _length;
				return length >= 0;
			}
		}
		class exposeApiMsgHandler : ApiMessageHandler
		{
			public new HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
			=> base.ProcessResponse(response, cancellationToken);
		}

		// blacklist
		[TestMethod]
		public void blank_headers()
		{
			var handler = new exposeApiMsgHandler();
			var content = new testContent();
			var response = new HttpResponseMessage { Content = content };
			var _ = CancellationToken.None;

			handler.ProcessResponse(response, _);

			content.serializedWasCalled.Should().BeFalse();
		}

		// blacklist
		[TestMethod]
		public void length_null()
		{
			var handler = new exposeApiMsgHandler();
			var content = new testContent(-1);
			content.Headers.ContentType = new MediaTypeHeaderValue("a/b");
			var response = new HttpResponseMessage { Content = content };
			var _ = CancellationToken.None;

			handler.ProcessResponse(response, _);

			content.serializedWasCalled.Should().BeFalse();
		}

		// blacklist
		[TestMethod]
		public void length_0()
		{
			var handler = new exposeApiMsgHandler();
			var content = new testContent(0);
			content.Headers.ContentType = new MediaTypeHeaderValue("a/b");
			var response = new HttpResponseMessage { Content = content };
			var _ = CancellationToken.None;

			handler.ProcessResponse(response, _);

			content.serializedWasCalled.Should().BeFalse();
		}

		// blacklist
		[TestMethod]
		public void length_huge()
		{
			var handler = new exposeApiMsgHandler();
			var content = new testContent(1_000_001);
			content.Headers.ContentType = new MediaTypeHeaderValue("a/b");
			var response = new HttpResponseMessage { Content = content };
			var _ = CancellationToken.None;

			handler.ProcessResponse(response, _);

			content.serializedWasCalled.Should().BeFalse();
		}

		// whitelist
		[TestMethod]
		[DataRow("application/json", true)]
		[DataRow("application/xml", true)]
		[DataRow("application/x-www-form-urlencoded", true)]
		[DataRow("multipart/form-data", true)]
		[DataRow("text/foo", true)]
		[DataRow("text/plain", true)]
		[DataRow("text/html", true)]
		//invalid: text/html; charset=utf-8"
		[DataRow("audio/vnd.audible.aax", false)]
		public void content_types(string contentType, bool expected)
		{
			var handler = new exposeApiMsgHandler();
			var content = new testContent(5);
			content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			var response = new HttpResponseMessage
			{
				Content = content,
				RequestMessage = new HttpRequestMessage
				{
					RequestUri = new Uri("http://a.b")
				}
			};
			var _ = CancellationToken.None;

			handler.ProcessResponse(response, _);

			Assert.AreEqual(expected, content.serializedWasCalled);
		}
	}
}
