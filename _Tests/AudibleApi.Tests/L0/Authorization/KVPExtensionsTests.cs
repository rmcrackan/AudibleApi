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
using static AuthorizationShared.Shared;

namespace Authoriz.KVPExtensionsTests
{
	[TestClass]
	public class ToKeyValuePair
	{
		[TestMethod]
		public void null_param_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() => KVPExtensions.ToKeyValuePair(null));

		[TestMethod]
		public void convert_0()
		{
			var pairs = new List<KVP<string, string>>();

			var result = pairs.ToKeyValuePair();

			result.Should().NotBeNull();
			result.Count.Should().Be(0);
		}

		[TestMethod]
		public void convert_1()
		{
			var pairs = new List<KVP<string, string>>
			{
				new KVP<string, string> { Key="k", Value="val" }
			};

			var result = pairs.ToKeyValuePair();

			result.Count.Should().Be(1);
			var kvp = result[0];
			kvp.Key.Should().Be("k");
			kvp.Value.Should().Be("val");
		}

		[TestMethod]
		public void convert_3()
		{
			// duplicate keys are allowed
			var pairs = new List<KVP<string, string>>
			{
				new KVP<string, string> { Key="k", Value="val" },
				new KVP<string, string> { Key="k", Value="val2" },
				new KVP<string, string> { Key="k3", Value="val" },
			};

			var result = pairs.ToKeyValuePair();

			result.Count.Should().Be(3);

			var kvp1 = result[0];
			kvp1.Key.Should().Be("k");
			kvp1.Value.Should().Be("val");

			var kvp2 = result[1];
			kvp2.Key.Should().Be("k");
			kvp2.Value.Should().Be("val2");

			var kvp3 = result[2];
			kvp3.Key.Should().Be("k3");
			kvp3.Value.Should().Be("val");
		}
	}
}
