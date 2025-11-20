using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
using AudibleApi.Cryptography;
using Dinah.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shouldly;
using TestAudibleApiCommon;
using static AuthorizationShared.Shared;

namespace StackBlockerTests
{
    [TestClass]
	public class L1Regex
	{
		[TestMethod]

		[DataRow("L1")]
		[DataRow("l1")]

		[DataRow("L1_")]
		[DataRow("l1_")]
		[DataRow("L1.")]
		[DataRow("l1.")]
		[DataRow("L1_foo")]
		[DataRow("l1_foo")]
		[DataRow("L1.foo")]
		[DataRow("l1.foo")]

		[DataRow("_L1_")]
		[DataRow("_l1_")]
		[DataRow(".L1_")]
		[DataRow(".l1_")]
		[DataRow("_L1.")]
		[DataRow("_l1.")]
		[DataRow(".L1.")]
		[DataRow(".l1.")]

		[DataRow("foo_L1_")]
		[DataRow("foo_l1_")]
		[DataRow("foo.L1_")]
		[DataRow("foo.l1_")]
		[DataRow("foo_L1.")]
		[DataRow("foo_l1.")]
		[DataRow("foo.L1.")]
		[DataRow("foo.l1.")]

		[DataRow("_L1_foo")]
		[DataRow("_l1_foo")]
		[DataRow(".L1_foo")]
		[DataRow(".l1_foo")]
		[DataRow("_L1.foo")]
		[DataRow("_l1.foo")]
		[DataRow(".L1.foo")]
		[DataRow(".l1.foo")]

		[DataRow("_L1")]
		[DataRow("_l1")]
		[DataRow(".L1")]
		[DataRow(".l1")]
		[DataRow("foo_L1")]
		[DataRow("foo_l1")]
		[DataRow("foo.L1")]
		[DataRow("foo.l1")]
		public void matches(string str)
			=> Assert.IsTrue(StackBlocker.L1Regex.IsMatch(str));

		[TestMethod]

		[DataRow("")]
		[DataRow("L")]
		[DataRow("_L")]
		[DataRow("L_")]
		[DataRow("L_1")]
		public void non_matches(string str)
			=> Assert.IsFalse(StackBlocker.L1Regex.IsMatch(str));
	}
}

namespace StackBlockerTests_L0_Fail
{
    [TestClass]
	public class ApiTestBlocker
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.Throws<MethodAccessException>(() => StackBlocker.ApiTestBlocker());
	}
}

namespace StackBlockerTests_L1_Pass
{
    [TestClass]
	public class ApiTestBlocker
	{
		[TestMethod]
		public void access_from_L1_passes()
			=> StackBlocker.ApiTestBlocker();
	}

	[TestClass]
	public class IdentityMaintainer_CreateAsync_identity
	{
		[TestMethod]
		public async Task access_from_L1_passes()
		{
			var identity = Substitute.For<IIdentity>();
			identity.ExistingAccessToken.Returns(AccessToken.EmptyFuture);
			identity.IsValid.Returns(true);
			identity.Locale.Returns(Locale.Empty);

			await IdentityMaintainer.CreateAsync(identity);
		}

		[TestMethod]
		public async Task null_param_throws()
			=> await Assert.ThrowsAsync<ArgumentNullException>(() => IdentityMaintainer.CreateAsync(null));
	}

	[TestClass]
	public class HttpClientSharer_ctor
	{
		[TestMethod]
		public void access_from_L1_passes()
			=> new HttpClientSharer();
	}

	[TestClass]
	public class Authorize_ctor
	{
		[TestMethod]
		public void access_from_L1_passes()
			=> new Authorize(Locale.Empty);
	}

	[TestClass]
	public class Authenticate_ctor
	{
		[TestMethod]
		public void access_from_L1_passes()
			=> new AudibleApi.Authentication.Authenticate(Localization.Get("us"), "");
	}

	[TestClass]
	public class Api_ctor_identityMaintainer
	{
		class MockIdentityMaintainer : IIdentityMaintainer
		{
			public ISystemDateTime SystemDateTime => throw new NotImplementedException();
			public Locale Locale => Localization.Get("us");
			public string DeviceSerialNumber => throw new NotImplementedException();
			public string DeviceType => throw new NotImplementedException();
			public string AmazonAccountId => throw new NotImplementedException();
			public Task<AccessToken> GetAccessTokenAsync() => throw new NotImplementedException();
			public Task<AdpToken> GetAdpTokenAsync() => throw new NotImplementedException();
			public Task<PrivateKey> GetPrivateKeyAsync() => throw new NotImplementedException();
		}

		[TestMethod]
		public void access_from_L1_passes()
			=> new Api(new MockIdentityMaintainer());

		[TestMethod]
		public void null_param_throws()
			=> Assert.Throws<ArgumentNullException>(() => new Api(null));
	}

	[TestClass]
	public class ApiHttpClient_ctor
	{
		[TestMethod]
		public void access_from_L1_passes()
			=> ApiHttpClient.Create();
	}
}
