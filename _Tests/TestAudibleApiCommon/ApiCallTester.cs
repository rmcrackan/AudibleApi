using System;
using System.Net;
using System.Net.Http;
using AudibleApi;
using BaseLib;
using Moq;

namespace TestAudibleApiCommon
{
	public class ApiCallTester
	{
		public Api Api { get; }
		public HttpRequestMessage CapturedRequest { get; private set; }

		void logRequest(HttpRequestMessage r) => CapturedRequest = r;

		public ApiCallTester(string returnString)
		{
			var mockClient = new Mock<ISealedHttpClient>();
			mockClient
				.Setup(c => c.SendAsync(It.IsAny<HttpRequestMessage>()))
				.ReturnsAsync(
				(HttpRequestMessage request) =>
				new HttpResponseMessage
				{
					StatusCode = HttpStatusCode.OK,
					Content = new StringContent(returnString)
				})
				.Callback<HttpRequestMessage>(m => logRequest(m));
			var mockIdMaintainer = new MockIdMaintainer();
			var mockSharer = new Mock<IClientSharer>();
			mockSharer
				.Setup(s => s.GetSharedClient(It.IsAny<Uri>()))
				.Returns(mockClient.Object);
			Api = Api ?? new Api(mockIdMaintainer, mockSharer.Object);
		}
	}
}
