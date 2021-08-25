using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TestAudibleApiCommon;

namespace LoginTests_L0
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.ThrowsException<MethodAccessException>(() => new Authenticate(Locale.Empty));
	}

    [TestClass]
    public class ctor_client_systemDateTime
    {
        [TestMethod]
        public void null_locale_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new Authenticate(null, ApiHttpClient.Create(HttpMock.GetHandler()), new Mock<ISystemDateTime>().Object));

        [TestMethod]
        public void null_httpClientHandler_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new Authenticate(Locale.Empty, null, new Mock<ISystemDateTime>().Object));

		[TestMethod]
		public void has_cookies_throws()
		{
			var client = ApiHttpClient.Create(HttpMock.GetHandler());
			client.CookieJar.Add(new Cookie("foo", "bar", "/", "a.com"));
			Assert.ThrowsException<ArgumentException>(() => new Authenticate(Locale.Empty, client, new Mock<ISystemDateTime>().Object));
		}

		[TestMethod]
		public void null_systemDateTime_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new Authenticate(Locale.Empty, ApiHttpClient.Create(HttpMock.GetHandler()), null));
    }

    [TestClass]
    public class LoadSessionCookiesAsync
    {
        private Locale locale => Localization.Get("us");

        private Authenticate GetLogin(HttpClientHandler handler)
        {
			var client = ApiHttpClient.Create(handler);
			var login = new Authenticate(locale, client, new Mock<ISystemDateTime>().Object)
			{
				MaxLoadSessionCookiesTrips = 3
			};
			return login;
        }

        [TestMethod]
        public async Task trip_2_passes()
        {
            var msg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // 1st run: do nothing
                .ReturnsAsync(msg)
                // 2nd run: set cookie
                .Returns(() =>
                {
                    handlerMock.Object.CookieContainer.Add(
                        new Cookie("session-token", "my-value", "/", "amazon.com")
                        );
                    return Task.FromResult(msg);
                });

            var login = GetLogin(handlerMock.Object);
            await has_session_token(login, locale);
        }

        [TestMethod]
        public async Task trip_3_passes()
        {
            var msg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // 1st run: do nothing
                .ReturnsAsync(msg)
                // 2nd run: do nothing
                .ReturnsAsync(msg)
                // 3rd run: set cookie
                .Returns(() =>
                {
                    handlerMock.Object.CookieContainer.Add(
                        new Cookie("session-token", "my-value", "/", "amazon.com")
                        );
                    return Task.FromResult(msg);
                });

            var login = GetLogin(handlerMock.Object);
            await has_session_token(login, locale);
        }

        [TestMethod]
        public async Task trip_4_fails()
        {
            var msg = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            var handlerMock = new Mock<HttpClientHandler>(MockBehavior.Strict);
            handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // 1st run: do nothing
                .ReturnsAsync(msg)
                // 2nd run: do nothing
                .ReturnsAsync(msg)
                // 3rd run: do nothing
                .ReturnsAsync(msg)
                // 4th run: would set cookie if we got this far
                .Returns(() =>
                {
                    handlerMock.Object.CookieContainer.Add(
                        new Cookie("session-token", "my-value", "/", "amazon.com")
                        );
                    return Task.FromResult(msg);
                });

            var login = GetLogin(handlerMock.Object);
            await Assert.ThrowsExceptionAsync<TimeoutException>(() => has_session_token(login, locale));
        }

        public static async Task has_session_token(Authenticate login, Locale locale)
        {
            await login.LoadSessionCookiesAsync();

            var amazonCookies = login.GetCookies(locale.AmazonLoginUri()).Cast<Cookie>().ToList();
            var cookieNames = amazonCookies.Select(c => c.Name);
            cookieNames.Should().Contain("session-token");
        }
    }
}
