using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authorization;
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
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authoriz.IdentityTests
{
	[TestClass]
	public class KVP_class
	{
		[TestMethod]
		public new void ToString()
			=> new KVP<string, string> { Key = "k", Value = "val" }
			.ToString()
			.Should().Be("[k=val]");
	}

	[TestClass]
	public class ctor
	{
		[TestMethod]
		public void null_params_throw()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				null,
				new List<KeyValuePair<string, string>>()));
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				new AccessToken("Atna|", DateTime.MaxValue),
				null));
		}

		[TestMethod]
		public void loads_cookies()
		{
			var idMgr = new Identity(new AccessToken("Atna|", DateTime.MaxValue), new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("name1", "value1") });

			idMgr.Cookies.Count().Should().Be(1);
			idMgr.Cookies.Single().Key.Should().Be("name1");
			idMgr.Cookies.Single().Value.Should().Be("value1");
		}
	}

	[TestClass]
	public class IsValid
	{
		string pk { get; } = PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING;
		string adp { get; } = AdpTokenValue;
		string at { get; } = AccessToken.REQUIRED_BEGINNING;
		string rt { get; } = RefreshToken.REQUIRED_BEGINNING;

		[TestMethod]
		public void ctor_state()
		{
			var id = new Identity(new AccessToken(at, DateTime.MaxValue), new List<KeyValuePair<string, string>>());
			id.IsValid.Should().BeFalse();
		}

		[TestMethod]
		public void update_state()
		{
			var id = new Identity(new AccessToken(at, DateTime.MaxValue), new List<KeyValuePair<string, string>>());
			id.IsValid.Should().BeFalse();

			id.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				new AccessToken(at, DateTime.MaxValue),
				new RefreshToken(rt)
				);
			id.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void invalidate_state()
		{
			var id = new Identity(new AccessToken(at, DateTime.MaxValue), new List<KeyValuePair<string, string>>());
			id.IsValid.Should().BeFalse();

			id.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				new AccessToken(at, DateTime.MaxValue),
				new RefreshToken(rt)
				);
			id.IsValid.Should().BeTrue();

			id.Invalidate();
			id.IsValid.Should().BeFalse();
		}

		[TestMethod]
		public void from_json()
		{
			var id = Identity.FromJson(IdentityJson_Future);
			id.IsValid.Should().BeTrue();
		}
	}

	[TestClass]
	public class FromJson
	{
		[TestMethod]
		public void loads_all_values()
		{
			var idMgr = GetIdentity_Future();
			idMgr.AdpToken.Should().Be(AdpTokenValue);
			idMgr.Cookies.Count().Should().Be(2);
			var cookies = idMgr.Cookies.ToKeyValuePair();
			cookies[0].Key.Should().Be("key1");
			cookies[1].Key.Should().Be("key1");
			cookies[0].Value.Should().Be("value 1");
			cookies[1].Value.Should().Be("value 2");
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(AccessTokenExpires_Future_Parsed);
			idMgr.PrivateKey.Should().Be(PrivateKeyValueNewLines);
			idMgr.RefreshToken.Should().Be(RefreshTokenValue);
		}
	}

	[TestClass]
	public class Updated
	{
		[TestMethod]
		public void no_subscribers_does_not_throw()
		{
			var idMgr = Identity.FromJson(IdentityJson_Future);
			idMgr.Update(new AccessToken("Atna|", DateTime.MaxValue));
		}

		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = Identity.FromJson(IdentityJson_Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.Update(new AccessToken("Atna|", DateTime.MaxValue));

			log.Count.Should().Be(1);
		}
	}

	[TestClass]
	public class Update_accessToken
	{
		[TestMethod]
		public void null_param_throws()
		{
			var idMgr = Identity.FromJson(IdentityJson_Future);
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(null));
		}

		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = Identity.FromJson(IdentityJson_Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(AccessTokenExpires_Future_Parsed);

			idMgr.Update(new AccessToken(AccessToken.REQUIRED_BEGINNING, DateTime.MaxValue));

			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessToken.REQUIRED_BEGINNING);
			idMgr.ExistingAccessToken.Expires.Should().Be(DateTime.MaxValue);

			log.Count.Should().Be(1);
		}
	}

	[TestClass]
	public class Update_all
	{
		string pk { get; } = PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING;
		string adp { get; } = AdpTokenValue;
		string at { get; } = AccessToken.REQUIRED_BEGINNING;
		string rt { get; } = RefreshToken.REQUIRED_BEGINNING;

		[TestMethod]
		public void null_param_throws()
		{
			var idMgr = Identity.FromJson(IdentityJson_Future);
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(
				null,
				new AdpToken(adp),
				new AccessToken(at, DateTime.MaxValue),
				new RefreshToken(rt)
				));
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(
				new PrivateKey(pk),
				null,
				new AccessToken(at, DateTime.MaxValue),
				new RefreshToken(rt)
				));
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				null,
				new RefreshToken(rt)
				));
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				new AccessToken(at, DateTime.MaxValue),
				null
				));
		}

		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = Identity.FromJson(IdentityJson_Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.AdpToken.Value.Should().Be(AdpTokenValue);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(AccessTokenExpires_Future_Parsed);
			idMgr.RefreshToken.Value.Should().Be(RefreshTokenValue);

			idMgr.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				new AccessToken(at, DateTime.MaxValue),
				new RefreshToken(rt)
				);

			idMgr.PrivateKey.Value.Should().Be(pk);
			idMgr.AdpToken.Value.Should().Be(adp);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(at);
			idMgr.ExistingAccessToken.Expires.Should().Be(DateTime.MaxValue);
			idMgr.RefreshToken.Value.Should().Be(rt);

			log.Count.Should().Be(1);
		}
	}

	[TestClass]
	public class Invalidate
	{
		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = Identity.FromJson(IdentityJson_Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.AdpToken.Value.Should().Be(AdpTokenValue);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(AccessTokenExpires_Future_Parsed);
			idMgr.RefreshToken.Value.Should().Be(RefreshTokenValue);

			idMgr.Invalidate();

			idMgr.IsValid.Should().BeFalse();
			idMgr.AdpToken.Should().BeNull();
			idMgr.RefreshToken.Should().BeNull();
			idMgr.ExistingAccessToken.Expires.Should().Be(DateTime.MinValue);

			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);

			log.Count.Should().Be(1);
		}
	}
}
