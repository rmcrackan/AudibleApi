using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using BaseLib;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;
using static AuthorizationShared.Shared;

namespace ApiClientTests
{
	[TestClass]
	public class Create
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.ThrowsException<MethodAccessException>(() => ApiClient.Create());
	}

	[TestClass]
	public class Create_innerHandler
	{
		[TestMethod]
		public void null_param_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => ApiClient.Create(null));

		[TestMethod]
		public void has_cookies_throws()
		{
			var httpClientHandler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer()
			};
			httpClientHandler.CookieContainer.Add(new Cookie("foo", "bar", "/", "a.com"));
			Assert.ThrowsException<ArgumentException>(() => ApiClient.Create(httpClientHandler));
		}

		[TestMethod]
		public void sets_cookie_jar()
			=> ApiClient
				.Create(ApiClientMock.GetHandler())
				.CookieJar
				.Should().NotBeNull();

		[TestMethod]
		public async Task throw_api_error()
		{
			// ensure handler incl ApiMessageHandler

			var handler = ApiClientMock.GetHandler(new JObject
			{
				{ "message", "Invalid response group" }
			}.ToString());
			var client = ApiClient.Create(handler);
			await Assert.ThrowsExceptionAsync<InvalidResponseException>(() => client.GetAsync(new Uri("http://a.com")));
		}
	}
}
