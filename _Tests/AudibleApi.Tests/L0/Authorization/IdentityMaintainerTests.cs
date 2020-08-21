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

namespace Authoriz.IdentityMaintainerTests
{
	[TestClass]
	public class CreateAsync_identity
	{
		[TestMethod]
		public async Task access_from_L0_throws()
			=> await Assert.ThrowsExceptionAsync<MethodAccessException>(() => IdentityMaintainer.CreateAsync(new Mock<IIdentity>().Object));
	}

	[TestClass]
	public class CreateAsync_id_auth_sysDateTime
	{
		[TestMethod]
		public async Task null_identity_throws()
			=> await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
			IdentityMaintainer.CreateAsync(
				null,
				new Mock<IAuthorize>().Object,
				new Mock<ISystemDateTime>().Object
				));

		[TestMethod]
		public async Task null_authorize_throws()
			=> await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
			IdentityMaintainer.CreateAsync(
				new Mock<IIdentity>().Object,
				null,
				new Mock<ISystemDateTime>().Object
				));

		[TestMethod]
		public async Task null_systemDateTime_throws()
			=> await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
			IdentityMaintainer.CreateAsync(
				new Mock<IIdentity>().Object,
				new Mock<IAuthorize>().Object,
				null
				));

		[TestMethod]
		public async Task invalid_identity_registers()
		{
			var identity = new Mock<IIdentity>();
			identity
				.Setup(a => a.ExistingAccessToken)
				.Returns(new AccessToken("Atna|", DateTime.MaxValue));
			identity
				.Setup(a => a.IsValid)
				.Returns(false);

			List<string> log = new List<string>();

			var mockAuth = new Mock<IAuthorize>();
			mockAuth
				.Setup(a => a.RegisterAsync(It.IsAny<AccessToken>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
				.ReturnsAsync(JObject.Parse(AuthenticateResponse))
				.Callback(() => log.Add("called"));

			await IdentityMaintainer.CreateAsync(
				identity.Object,
				mockAuth.Object,
				new Mock<ISystemDateTime>().Object
				);

			log.Count.Should().Be(1);
			log[0].Should().Be("called");
		}

		[TestMethod]
		public async Task valid_identity_does_not_register()
		{
			var identity = new Mock<IIdentity>();
			identity
				.Setup(a => a.ExistingAccessToken)
				.Returns(new AccessToken("Atna|", DateTime.MaxValue));
			identity
				.Setup(a => a.IsValid)
				.Returns(true);

			List<string> log = new List<string>();

			var mockAuth = new Mock<IAuthorize>();
			mockAuth
				.Setup(a => a.RegisterAsync(It.IsAny<AccessToken>(), It.IsAny<IEnumerable<KeyValuePair<string, string>>>()))
				.ReturnsAsync(JObject.Parse(AuthenticateResponse))
				.Callback(() => log.Add("called"));

			await IdentityMaintainer.CreateAsync(
				identity.Object,
				mockAuth.Object,
				new Mock<ISystemDateTime>().Object
				);

			log.Count.Should().Be(0);
		}
	}

	[TestClass]
	public class GetAccessTokenAsync
	{
		[TestMethod]
		public async Task no_refresh_needed()
		{
			var handler = HttpMock.GetHandler();
			var now = StaticSystemDateTime.Past.UtcNow;

			// with testing time, don't assume greater accuracy than ms
			var times = new DateTime[]
			{
				DateTime.MaxValue.Date.AddMinutes(-1),
				now.AddMinutes(1)
			};

			foreach (var t in times)
			{
				var accessToken = await _test_refresh(handler, t, 0);

				accessToken.Expires.Should().Be(t);
				accessToken.TokenValue.Should().Be(AccessTokenValue);
			}
		}

		[TestMethod]
		public async Task with_refresh()
		{
			var handler = HttpMock.GetHandler(RefreshTokenResponse);
			var now = StaticSystemDateTime.Past.UtcNow;

			// with testing time, don't assume greater accuracy than ms
			var times = new DateTime[]
			{
				DateTime.MinValue.Date.AddMinutes(1),
				now,
				now.AddTicks(-1)
			};

			foreach (var t in times)
			{
				var accessToken = await _test_refresh(handler, t, 1);

				accessToken.Expires.Should().Be(now.AddHours(1));
				accessToken.TokenValue.Should().Be(AccessTokenValue);
			}
		}

		private async Task<AccessToken> _test_refresh(HttpMessageHandler handler, DateTime expires, int expectedSaveCount)
		{
			var json = GetIdentityJson(Future).Replace(GetAccessTokenExpires(Future), expires.ToString());

			var log = new List<string>();
			var identity = Identity.FromJson(json);
			identity.Updated += (_, __) => log.Add("saved");
			var sharer = new HttpClientSharer(handler);
			var systemDateTime = StaticSystemDateTime.Past;
			var maintainer = await IdentityMaintainer.CreateAsync(
				identity,
				new Authorize(Locale.Empty, sharer, systemDateTime),
				systemDateTime);

			var returnToken = await maintainer.GetAccessTokenAsync();
			log.Count.Should().Be(expectedSaveCount);

			// 2nd call should not need a refresh
			var token2 = await maintainer.GetAccessTokenAsync();
			log.Count.Should().Be(expectedSaveCount);

			returnToken.Should().Be(token2);
			returnToken.Expires.Should().Be(returnToken.Expires);
			returnToken.TokenValue.Should().Be(returnToken.TokenValue);

			return returnToken;
		}
	}

	// GetAdpTokenAsync , GetPrivateKeyAsync:
	// GetAccessTokenAsync and EnsureStateAsync provide ample test coverage

	[TestClass]
	public class RegisterAsync
	{
		[TestMethod]
		public async Task valid_parse_invokes_update_event()
		{
			var handler = HttpMock.GetHandler(AuthenticateResponse);
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
			var maintainer = new MockIdMaintainer(idMgr, handler);
			idMgr.Updated += (_, __) => log.Add("updated");

			await maintainer.RegisterAsync();

			log.Count.Should().Be(1);
		}
	}

	[TestClass]
	public class DeregisterAsync
	{
		[TestMethod]
		public async Task failure_throws()
		{
			var handler = HttpMock.GetHandler(AuthenticateResponse, HttpStatusCode.GatewayTimeout);
			var idMgr = GetIdentity(Future);
			var maintainer = new MockIdMaintainer(idMgr, handler);

			await Assert.ThrowsExceptionAsync<RegistrationException>(() => maintainer.DeregisterAsync());
		}

		[TestMethod]
		public async Task success_clears_values()
		{
			var handler = HttpMock.GetHandler(AuthenticateResponse);
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
			var maintainer = new MockIdMaintainer(idMgr, handler);
			idMgr.Updated += (_, __) => log.Add("updated");

			// verify initial load
			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.AdpToken.Value.Should().Be(AdpTokenValue);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Future));
			idMgr.RefreshToken.Should().Be(RefreshTokenValue);

			await maintainer.DeregisterAsync();

			// update invoked
			log.Count.Should().Be(1);

			// unchanged
			idMgr.PrivateKey.Value.Should().Be(PrivateKeyValueNewLines);
			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);

			// cleared
			idMgr.AdpToken.Should().BeNull();
			idMgr.RefreshToken.Should().BeNull();
			idMgr.ExistingAccessToken.Expires.Should().Be(DateTime.MinValue);
		}
	}

	[TestClass]
	public class RefreshAccessTokenAsync
	{
		[TestMethod]
		public async Task current_token_noop()
		{
			var handler = HttpMock.GetHandler();
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
			var maintainer = new MockIdMaintainer(idMgr, handler);
			idMgr.Updated += (_, __) => log.Add("updated");

			await maintainer.RefreshAccessTokenAsync();

			log.Count.Should().Be(0);
		}

		[TestMethod]
		public async Task expired_token_refreshes()
		{
			// new token: prefix with FOO
			var response = RefreshTokenResponse.Replace(
				@"""access_token"": """,
				@"""access_token"": ""Atna|FOO"
			);
			var handler = HttpMock.GetHandler(response);

			// init w/expired token
			var log = new List<string>();
			var idMgr = GetIdentity(Expired);
			var maintainer = new MockIdMaintainer(idMgr, handler);

			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Expired));

			await maintainer.RefreshAccessTokenAsync();

			// token is refreshed
			idMgr.ExistingAccessToken.TokenValue.Should().Be("Atna|FOO" + AccessTokenValue);
			var now = StaticSystemDateTime.Past.UtcNow;
			idMgr.ExistingAccessToken.Expires.Should().Be(now.AddHours(1));

			// updated is invoked
			log.Count.Should().Be(1);
		}
	}

	[TestClass]
	public class EnsureStateAsync
	{
		[TestMethod]
		public async Task refresh_succeeds_current()
		{
			// refresh access token succeeds. expire date is current

			// copied from RefreshAccessTokenAsync.current_token_noop

			var handler = HttpMock.GetHandler();
			var log = new List<string>();

			var idMgr = GetIdentity(Future);
			var maintainer = new MockIdMaintainer(idMgr, handler);
			idMgr.Updated += (_, __) => log.Add("updated");

			await maintainer.EnsureStateAsync();

			log.Count.Should().Be(0);
		}

		[TestMethod]
		public async Task refresh_succeeds_expired()
		{
			// refresh access token succeeds. expire date is expired

			// copied from RefreshAccessTokenAsync.expired_token_refreshes

			// new token: prefix with FOO
			var response = RefreshTokenResponse.Replace(
				@"""access_token"": """,
				@"""access_token"": ""Atna|FOO"
			);
			var handler = HttpMock.GetHandler(response);

			// init w/expired token
			var log = new List<string>();
			var idMgr = GetIdentity(Expired);
			var maintainer = new MockIdMaintainer(idMgr, handler);

			idMgr.Updated += (_, __) => log.Add("updated");

			idMgr.ExistingAccessToken.TokenValue.Should().Be(AccessTokenValue);
			idMgr.ExistingAccessToken.Expires.Should().Be(GetAccessTokenExpires_Parsed(Expired));

			await maintainer.EnsureStateAsync();

			// token is refreshed
			idMgr.ExistingAccessToken.TokenValue.Should().Be("Atna|FOO" + AccessTokenValue);
			var now = StaticSystemDateTime.Past.UtcNow;
			idMgr.ExistingAccessToken.Expires.Should().Be(now.AddHours(1));

			// updated is invoked
			log.Count.Should().Be(1);
		}

		[TestMethod]
		public async Task reregister_succeeds()
		{
			// refresh throws, deregister succeeds, register succeeds

			var statusCode = HttpStatusCode.OK;
			var _3rdReturnString = AuthenticateResponse.Replace(
				@"""access_token"": """,
				@"""access_token"": ""Atna|FOO"
			);

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup("Dispose", ItExpr.IsAny<bool>())
				.Verifiable();

			handlerMock
				.Protected()
				.SetupSequence<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
			#region 1st handler call: RefreshAccessTokenAsync throws
				.ThrowsAsync(new Exception())
			#endregion
			#region 2nd handler call: DeregisterAsync passes
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = statusCode,
					Content = new StringContent("doesn't matter"),
					RequestMessage = new HttpRequestMessage()
				})
			#endregion
			#region 3rd handler call: RegisterAsync passes
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = statusCode,
					Content = new StringContent(_3rdReturnString),
					RequestMessage = new HttpRequestMessage()
				})
			#endregion
				;
			var handler = handlerMock.Object;

			var log = new List<string>();
			// expired token
			var idMgr = GetIdentity(Expired);
			var maintainer = new MockIdMaintainer(idMgr, handler);
			idMgr.Updated += (_, __) => log.Add("updated");

			await maintainer.EnsureStateAsync();

			// token is refreshed
			idMgr.ExistingAccessToken.TokenValue.Should().Be("Atna|FOO" + AccessTokenValue);
			var now = StaticSystemDateTime.Past.UtcNow;
			idMgr.ExistingAccessToken.Expires.Should().Be(now.AddHours(1));

			// updated is invoked by DeregisterAsync and RegisterAsync
			log.Count.Should().Be(2);
		}

		[TestMethod]
		public async Task register_throws()
		{
			// refresh throws, deregister succeeds, register throws

			var statusCode = HttpStatusCode.OK;

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup("Dispose", ItExpr.IsAny<bool>())
				.Verifiable();

			handlerMock
				.Protected()
				.SetupSequence<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
			#region 1st handler call: RefreshAccessTokenAsync throws
				.ThrowsAsync(new Exception())
			#endregion
			#region 2nd handler call: DeregisterAsync passes
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = statusCode,
					Content = new StringContent("doesn't matter"),
					RequestMessage = new HttpRequestMessage()
				})
			#endregion
			#region 3rd handler call: RegisterAsync throws
				.ThrowsAsync(new Exception())
			#endregion
				;
			var handler = handlerMock.Object;

			var log = new List<string>();
			// expired token
			var idMgr = GetIdentity(Expired);
			var maintainer = new MockIdMaintainer(idMgr, handler);
			idMgr.Updated += (_, __) => log.Add("updated");

			var ex = await Assert.ThrowsExceptionAsync<RegistrationException>(() => maintainer.EnsureStateAsync());
			ex.Message.Should().Be("Error ensuring valid state");

			idMgr.AdpToken.Should().BeNull();
			idMgr.RefreshToken.Should().BeNull();
			idMgr.ExistingAccessToken.Expires.Should().Be(DateTime.MinValue);

			// updated is invoked by DeregisterAsync only
			log.Count.Should().Be(1);
		}

		[TestMethod]
		public async Task deregister_throws()
		{
			// refresh throws, deregister throws

			var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
			handlerMock
				.Protected()
				.Setup("Dispose", ItExpr.IsAny<bool>())
				.Verifiable();

			handlerMock
				.Protected()
				.SetupSequence<Task<HttpResponseMessage>>(
					"SendAsync",
					ItExpr.IsAny<HttpRequestMessage>(),
					ItExpr.IsAny<CancellationToken>()
				)
			#region 1st handler call: RefreshAccessTokenAsync throws
				.ThrowsAsync(new Exception())
			#endregion
			#region 2nd handler call: DeregisterAsync throws timeout
				.ReturnsAsync(new HttpResponseMessage()
				{
					StatusCode = HttpStatusCode.GatewayTimeout,
					Content = new StringContent("doesn't matter"),
					RequestMessage = new HttpRequestMessage()
				})
			#endregion
				;
			var handler = handlerMock.Object;

			var log = new List<string>();
			// expired token
			var idMgr = GetIdentity(Expired);
			var maintainer = new MockIdMaintainer(idMgr, handler);

			idMgr.Updated += (_, __) => log.Add("updated");

			var ex = await Assert.ThrowsExceptionAsync<RegistrationException>(() => maintainer.EnsureStateAsync());
			ex.Message.Should().Be("Error ensuring valid state");

			// updated never invoked
			log.Count.Should().Be(0);
		}
	}
}
