using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using static AuthorizationShared.Shared;

namespace ApiHttpClientTests
{
	[TestClass]
	public class Create
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.ThrowsException<MethodAccessException>(() => ApiHttpClient.Create());
	}

	[TestClass]
	public class Create_innerHandler
	{
		[TestMethod]
		public void null_param_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => ApiHttpClient.Create(null));

		[TestMethod]
		public void has_cookies_throws()
		{
			var httpClientHandler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer()
			};
			httpClientHandler.CookieContainer.Add(new Cookie("foo", "bar", "/", "a.com"));
			Assert.ThrowsException<ArgumentException>(() => ApiHttpClient.Create(httpClientHandler));
		}

		[TestMethod]
		public void sets_cookie_jar()
			=> ApiHttpClient
				.Create(HttpMock.GetHandler())
				.CookieJar
				.Should().NotBeNull();

		[TestMethod]
		public async Task throw_api_error()
		{
			// ensure handler incl ApiMessageHandler

			var handler = HttpMock.GetHandler(new JObject
			{
				{ "message", "Invalid response group" }
			}.ToString());
			var client = ApiHttpClient.Create(handler);
			await Assert.ThrowsExceptionAsync<InvalidResponseException>(() => client.GetAsync(new Uri("http://a.com")));
		}
	}
}
