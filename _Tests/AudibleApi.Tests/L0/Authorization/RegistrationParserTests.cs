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
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authoriz.RegistrationParserTests
{
	[TestClass]
	public class ParseRegistrationIntoIdentity
	{
		[TestMethod]
		public void null_authRegister_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() =>
				RegistrationParser.ParseRegistrationIntoIdentity(
					null,
					new Mock<IIdentity>().Object,
					new Mock<ISystemDateTime>().Object));

		[TestMethod]
		public void null_identity_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() =>
				RegistrationParser.ParseRegistrationIntoIdentity(
					new JObject(),
					null,
					new Mock<ISystemDateTime>().Object));

		[TestMethod]
		public void null_systemDateTime_throws()
			=> Assert.ThrowsException<ArgumentNullException>(() =>
				RegistrationParser.ParseRegistrationIntoIdentity(
					new JObject(),
					new Mock<IIdentity>().Object,
					null));

		[TestMethod]
		public void parse_real_string()
		{
			var authRegister = JObject.Parse(AuthenticateResponse);
			var id = Identity.FromJson(IdentityJson_Future);
			var sysDateTime = StaticSystemDateTime.Past;

			RegistrationParser.ParseRegistrationIntoIdentity(authRegister, id, sysDateTime);

			id.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			id.AdpToken.Value.Should().Be(AdpTokenValue);
			id.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			var now = StaticSystemDateTime.Past.UtcNow;
			var expires = now.AddSeconds(3600);
			id.ExistingAccessToken.Expires.Should().Be(expires);

			id.RefreshToken.Should().Be(RefreshTokenValue);
		}
	}
}
