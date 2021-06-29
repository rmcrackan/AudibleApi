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
using static AuthorizationShared.Shared.AccessTokenTemporality;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authoriz.IdentityTests
{
	[TestClass]
	public class KVP_class
	{
		[TestMethod]
		public void KvpToString()
			=> new KVP<string, string> { Key = "k", Value = "val" }
			.ToString()
			.Should().Be("[k=val]");
	}

	[TestClass]
	public class ctor_locale
	{
		[TestMethod]
		public void null_params_throw()
			=> Assert.ThrowsException<ArgumentNullException>(() => new Identity(null));

		[TestMethod]
		public void invalid()
		{
			var us = new Identity(Localization.Get("us"));
			us.IsValid.Should().BeFalse();
			us.Locale.Name.Should().Be("us");
		}
	}

	[TestClass]
	public class ctor_locale_accessToken_cookies
	{
		[TestMethod]
		public void null_params_throw()
		{
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				null,
				AccessToken.Empty,
				new List<KeyValuePair<string, string>>()));
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				Locale.Empty,
				null,
				new List<KeyValuePair<string, string>>()));
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				Locale.Empty,
				AccessToken.Empty,
				null));
		}

		[TestMethod]
		public void loads_cookies()
		{
			var idMgr = new Identity(Locale.Empty, AccessToken.Empty, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("name1", "value1") });

			idMgr.Cookies.Count().Should().Be(1);
			idMgr.Cookies.Single().Key.Should().Be("name1");
			idMgr.Cookies.Single().Value.Should().Be("value1");
		}
	}

	[TestClass]
	public class Empty
	{
		[TestMethod]
		public void ensure_valid()
		{
			// most important part is to get past this line w/o exceptions
			var i = Identity.Empty;
			i.IsValid.Should().BeFalse();
		}
	}

	[TestClass]
	public class IsValid
	{
		string pk { get; } = PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING;
		string adp { get; } = AdpTokenValue;
		string rt { get; } = RefreshToken.REQUIRED_BEGINNING;

		[TestMethod]
		public void ctor_state()
		{
			var id = Identity.Empty;
			id.IsValid.Should().BeFalse();
		}

		[TestMethod]
		public void update_state()
		{
			var id = Identity.Empty;
			id.IsValid.Should().BeFalse();

			id.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				AccessToken.Empty,
				new RefreshToken(rt)
				);
			id.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void invalidate_state()
		{
			var id = Identity.Empty;
			id.IsValid.Should().BeFalse();

			id.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				AccessToken.Empty,
				new RefreshToken(rt)
				);
			id.IsValid.Should().BeTrue();

			id.Invalidate();
			id.IsValid.Should().BeFalse();
		}

		[TestMethod]
		public void from_json()
		{
			var id = GetIdentity(Future);
			id.IsValid.Should().BeTrue();
		}

		[TestMethod]
		public void from_json_no_locale_not_valid()
		{
			var origJson = GetIdentityJson(Future);

			var jObj = JObject.Parse(origJson);
			jObj.Remove("LocaleName");
			var json = jObj.ToString(Formatting.Indented);

			var id = Identity.FromJson(json);
			id.IsValid.Should().BeFalse();
		}
	}

	[TestClass]
	public class FromJson
	{
		[TestMethod]
		public void loads_all_values()
		{
			var idMgr = GetIdentity(Future);
			idMgr.AdpToken.Should().Be(AdpTokenValue);
			idMgr.Cookies.Count().Should().Be(2);
			var cookies = idMgr.Cookies.ToKeyValuePair();
			cookies[0].Key.Should().Be("key1");
			cookies[1].Key.Should().Be("key1");
			cookies[0].Value.Should().Be("value 1");
			cookies[1].Value.Should().Be("value 2");
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));
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
			var idMgr = GetIdentity(Future);
			idMgr.Update(new AccessToken("Atna|", DateTime.MaxValue));
		}

		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
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
			var idMgr = GetIdentity(Future);
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(null));
		}

		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));

			idMgr.Update(AccessToken.Empty);

			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessToken.Empty.TokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(AccessToken.Empty.Expires);

			log.Count.Should().Be(1);
		}
	}

	[TestClass]
	public class Update_all
	{
		string pk { get; } = PrivateKey.REQUIRED_BEGINNING + PrivateKey.REQUIRED_ENDING;
		string adp { get; } = AdpTokenValue;
		string rt { get; } = RefreshToken.REQUIRED_BEGINNING;

		[TestMethod]
		public void null_param_throws()
		{
			var idMgr = GetIdentity(Future);
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(
				null,
				new AdpToken(adp),
				AccessToken.Empty,
				new RefreshToken(rt)
				));
			Assert.ThrowsException<ArgumentNullException>(() => idMgr.Update(
				new PrivateKey(pk),
				null,
				AccessToken.Empty,
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
				AccessToken.Empty,
				null
				));
		}

		[TestMethod]
		public void subscribers_are_updated()
		{
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.AdpToken.Value.Should().Be(AdpTokenValue);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));
			idMgr.RefreshToken.Value.Should().Be(RefreshTokenValue);

			idMgr.Update(
				new PrivateKey(pk),
				new AdpToken(adp),
				AccessToken.Empty,
				new RefreshToken(rt)
				);

			idMgr.PrivateKey.Value.Should().Be(pk);
			idMgr.AdpToken.Value.Should().Be(adp);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessToken.Empty.TokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(DateTime.MinValue);
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

			var idMgr = GetIdentity(Future);
			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.AdpToken.Value.Should().Be(AdpTokenValue);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));
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
