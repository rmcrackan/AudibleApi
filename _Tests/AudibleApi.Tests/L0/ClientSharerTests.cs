using System;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;
using static AuthorizationShared.Shared;

namespace ClientSharerTests
{
	[TestClass]
	public class ctor
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.ThrowsException<MethodAccessException>(() => new ClientSharer());
	}

	[TestClass]
	public class ctor_sharedMessageHandler
	{
		[TestMethod]
		public void null_param_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new ClientSharer(null));
	}

	[TestClass]
	public class GetSharedClient
	{
		public static HttpMessageHandler newHandler(Action action)
			=> HttpMock.CreateMockHttpClientHandler(action).Object;

		[TestMethod]
		public void null_param_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => new ClientSharer(newHandler(() => { })).GetSharedClient(null));

		[TestMethod]
		public async Task uses_SharedMessageHandler()
		{
			var log = new List<string>();
			var settings = new ClientSharer(newHandler(() => log.Add("send")));
			var client = settings.GetSharedClient(Resources.AmazonApiUri);

			var httpClient = client as HttpClient;
			httpClient.BaseAddress.AbsoluteUri.Should().Be("https://api.amazon.com/");

			await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://test.com")));

			log.Should().Equal("send");
		}

		[TestMethod]
		public void returned_instances_are_equal()
		{
			var settings = new ClientSharer(newHandler(() => { }));

			var client1 = settings.GetSharedClient(Resources.AmazonApiUri);
			var client2 = settings.GetSharedClient(Resources.AmazonApiUri);

			client1.Should().Be(client2);
		}
	}
}
