﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using TestAudibleApiCommon;
using TestCommon;
using static AuthorizationShared.Shared;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authoriz.AuthorizeTests
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.ThrowsException<MethodAccessException>(() => new Authorize(Locale.Empty));
	}

	[TestClass]
	public class ctor_sharer_systemDateTime
    {
        [TestMethod]
        public void null_locale_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new Authorize(null, new Mock<IHttpClientSharer>().Object, StaticSystemDateTime.Past));

        [TestMethod]
        public void null_sharer_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new Authorize(Locale.Empty, null, StaticSystemDateTime.Past));

        [TestMethod]
		public void null_systemDateTime_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new Authorize(Locale.Empty, new Mock<IHttpClientSharer>().Object, null));
	}

    [TestClass]
    public class ExtractAccessTokenAsync
    {
        [TestMethod]
        public async Task null_param_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new Authorize(Locale.Empty, new Mock<IHttpClientSharer>().Object, new Mock<ISystemDateTime>().Object).ExtractAccessTokenAsync(null));

        [TestMethod]
        public async Task parse_response()
        {
			var handler = HttpMock.GetHandler();

            var response = new HttpResponseMessage { Content = new StringContent(RefreshTokenResponse) };

            var accessToken = await new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past).ExtractAccessTokenAsync(response);

            accessToken.TokenValue.Should().Be(AccessTokenValue);

			var now = StaticSystemDateTime.Past.UtcNow;
            accessToken.Expires.Should().Be(now.AddSeconds(3600));
        }
    }

    [TestClass]
    public class RegisterAsync
    {
        [TestMethod]
        public async Task null_token_throws()
		{
			var handler = HttpMock.GetHandler();
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past);

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => auth.RegisterAsync(null, new List<KeyValuePair<string, string>>()));
        }

        [TestMethod]
        public async Task null_cookies_throws()
		{
			var handler = HttpMock.GetHandler();
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past);

			var accessToken = new AccessToken("Atna|", DateTime.MinValue);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => auth.RegisterAsync(accessToken, null));
        }

        [TestMethod]
        public async Task empty_cookies_throws()
		{
			var handler = HttpMock.GetHandler();
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past);

			var accessToken = new AccessToken("Atna|", DateTime.MinValue);
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => auth.RegisterAsync(accessToken, new Dictionary<string,string>()));
        }

        [TestMethod]
        public async Task error_throws_RegistrationException()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup("Dispose", ItExpr.IsAny<bool>())
                .Verifiable();
            handlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new RegistrationException());
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handlerMock.Object), StaticSystemDateTime.Past);

			var accessToken = new AccessToken("Atna|", DateTime.MinValue);
            await Assert.ThrowsExceptionAsync<RegistrationException>(() => auth.RegisterAsync(accessToken, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>() }));
        }
    }

    [TestClass]
    public class DeregisterAsync
    {
        [TestMethod]
        public async Task null_token_throws()
		{
			var handler = HttpMock.GetHandler();
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past);

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => auth.DeregisterAsync(null, new List<KeyValuePair<string, string>>()));
        }

        [TestMethod]
        public async Task null_cookies_throws()
		{
			var handler = HttpMock.GetHandler();
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past);

			var accessToken = new AccessToken("Atna|", DateTime.MinValue);
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => auth.DeregisterAsync(accessToken, null));
        }

        [TestMethod]
        public async Task error_throws_RegistrationException()
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup("Dispose", ItExpr.IsAny<bool>())
                .Verifiable();
            handlerMock
                .Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new RegistrationException());
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handlerMock.Object), StaticSystemDateTime.Past);

			var accessToken = new AccessToken("Atna|", DateTime.MinValue);
            await Assert.ThrowsExceptionAsync<RegistrationException>(() => auth.DeregisterAsync(accessToken, new List<KeyValuePair<string, string>>()));
        }
    }

    [TestClass]
    public class RefreshAccessTokenAsync
    {
        [TestMethod]
        public async Task null_param_throws()
		{
			var handler = HttpMock.GetHandler();
			var auth = new Authorize(Locale.Empty, new HttpClientSharer(handler), StaticSystemDateTime.Past);

			await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => auth.RefreshAccessTokenAsync(null));
        }
    }
}
