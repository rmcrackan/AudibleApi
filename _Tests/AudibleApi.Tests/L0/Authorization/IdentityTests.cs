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
using static AuthorizationShared.Shared.AccessTokenTemporality;

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
				OAuth2.Empty,
				new List<KeyValuePair<string, string>>()));
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				Locale.Empty,
				null,
				new List<KeyValuePair<string, string>>()));
			Assert.ThrowsException<ArgumentNullException>(() => new Identity(
				Locale.Empty,
				OAuth2.Empty,
				null));
		}

		[TestMethod]
		public void loads_cookies()
		{
			var idMgr = new Identity(Locale.Empty, OAuth2.Empty, new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("name1", "value1") });

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
}
