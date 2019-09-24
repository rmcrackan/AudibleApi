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
using static TestAudibleApiCommon.ComputedTestValues;

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0
{
	[TestClass]
	public class UserProfileAsync
	{
		public Api api { get; set; }

		[TestMethod]
		public async Task get_profile()
		{
			var json = await GetResponseAsync();
			var jObj = JObject.Parse(json);

			jObj.ContainsKey("user_id").Should().BeTrue();
			jObj.ContainsKey("name").Should().BeTrue();
			jObj.ContainsKey("shipping_address").Should().BeTrue();
			jObj.ContainsKey("email").Should().BeTrue();
		}

		// this must remain as a separate public step so ComputedTestValues can rebuild the string
		public async Task<string> GetResponseAsync()
		{
			api ??= await ApiClientMock.GetApiAsync(UserProfileValue);

			var jObj = await api.UserProfileAsync();
			var json = jObj.ToString(Formatting.Indented);
			return json;
		}
	}
}

// ApiTests_L0 should be inherited by L1. ApiTests_L0.Sealed should not be inherited by L1
namespace ApiTests_L0.Sealed
{
}
