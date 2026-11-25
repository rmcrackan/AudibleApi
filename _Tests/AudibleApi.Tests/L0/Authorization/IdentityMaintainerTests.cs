using AudibleApi.Authorization;
using Authoriz.TokenPersistenceFixtures;
using Dinah.Core;
using System.Collections.Generic;
using System.Net.Http;

namespace Authoriz.IdentityMaintainerTests;

[TestClass]
public class EnsureStateAsync
{
	[TestMethod]
	public async Task refresh_failure_preserves_error_without_deregistering_or_invalidating_identity()
	{
		var now = DateTime.UtcNow;
		var refreshException = new InvalidOperationException("simulated refresh failure");

		var identity = Substitute.For<IIdentity>();
		identity.IsValid.Returns(true);
		identity.Locale.Returns(Locales.Us);
		identity.RefreshToken.Returns(new RefreshToken("Atnr|test-refresh"));
		identity.ExistingAccessToken.Returns(new AccessToken("Atna|just-expired", now.AddMilliseconds(-100)));
		identity.Cookies.Returns(Array.Empty<KeyValuePair<string, SecretString>>());

		var authorize = Substitute.For<IAuthorize>();
		authorize
			.RefreshAccessTokenAsync(Arg.Any<RefreshToken>())
			.Returns<Task<AccessToken>>(_ => throw refreshException);
		// Even if Amazon would accept /auth/deregister within clock skew, EnsureStateAsync must never call it.
		authorize
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, SecretString>>>())
			.Returns(true);

		var clock = Substitute.For<ISystemDateTime>();
		clock.UtcNow.Returns(now);

		var ex = await Should.ThrowAsync<RegistrationException>(
			() => IdentityMaintainer.CreateAsync(identity, authorize, clock));

		ex.Message.ShouldBe("Error ensuring valid state");
		ex.InnerException.ShouldBeSameAs(refreshException);

		await authorize.DidNotReceive()
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, SecretString>>>());
		identity.DidNotReceive().Invalidate();
	}

	[TestMethod]
	public async Task transient_refresh_failure_preserves_real_identity_and_next_call_recovers()
	{
		Fixtures.ConfigurePlaintext();
		var identity = Fixtures.CreateRegisteredIdentity();
		var now = Fixtures.SampleExpires.AddMinutes(5);
		var refreshed = new AccessToken("Atna|recovered", now.AddHours(1));

		var authorize = Substitute.For<IAuthorize>();
		authorize
			.RefreshAccessTokenAsync(Arg.Any<RefreshToken>())
			.Returns(
				_ => Task.FromException<AccessToken>(new HttpRequestException("transient 503")),
				_ => Task.FromResult(refreshed));
		authorize
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, SecretString>>>())
			.Returns(true);

		var clock = Substitute.For<ISystemDateTime>();
		clock.UtcNow.Returns(now);

		// First call hits transient refresh failure: must throw, but keep tokens intact.
		var ex = await Should.ThrowAsync<RegistrationException>(
			() => IdentityMaintainer.CreateAsync(identity, authorize, clock));
		ex.InnerException.ShouldBeOfType<HttpRequestException>();
		identity.IsValid.ShouldBeTrue();
		identity.AdpToken.ShouldNotBeNull();
		identity.RefreshToken.ShouldNotBeNull();

		// Second call with the same persisted identity recovers cleanly.
		var maintainer = await IdentityMaintainer.CreateAsync(identity, authorize, clock);
		(await maintainer.GetAccessTokenAsync()).ShouldBe(refreshed);
		await authorize.DidNotReceive()
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, SecretString>>>());
	}

	[TestMethod]
	public async Task concurrent_calls_when_expired_refresh_access_token_only_once()
	{
		Fixtures.ConfigurePlaintext();
		var identity = Fixtures.CreateRegisteredIdentity();
		var now = Fixtures.SampleExpires.AddMinutes(5);
		var refreshed = new AccessToken("Atna|single-refresh", now.AddHours(1));

		var clock = Substitute.For<ISystemDateTime>();
		clock.UtcNow.Returns(now.AddHours(-2));

		var authorize = Substitute.For<IAuthorize>();
		var refreshStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var allowRefreshComplete = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
		var refreshCalls = 0;

		authorize
			.RefreshAccessTokenAsync(Arg.Any<RefreshToken>())
			.Returns(async _ =>
			{
				Interlocked.Increment(ref refreshCalls);
				refreshStarted.TrySetResult();
				await allowRefreshComplete.Task;
				return refreshed;
			});

		// Create maintainer before expiry, then advance clock past expiry.
		var maintainer = await IdentityMaintainer.CreateAsync(identity, authorize, clock);
		clock.UtcNow.Returns(now);

		var callers = Enumerable.Range(0, 10).Select(_ => maintainer.GetAccessTokenAsync()).ToArray();
		await refreshStarted.Task;
		allowRefreshComplete.SetResult();
		var results = await Task.WhenAll(callers);

		refreshCalls.ShouldBe(1);
		results.ShouldAllBe(t => t == refreshed);
	}

	[TestMethod]
	public async Task successful_refresh_does_not_attempt_deregister()
	{
		var newToken = new AccessToken("Atna|refreshed", DateTime.UtcNow.AddHours(1));

		var identity = Substitute.For<IIdentity>();
		identity.IsValid.Returns(true);
		identity.Locale.Returns(Locales.Us);
		identity.RefreshToken.Returns(new RefreshToken("Atnr|test-refresh"));
		identity.ExistingAccessToken.Returns(new AccessToken("Atna|expired", DateTime.UtcNow.AddHours(-2)));

		var authorize = Substitute.For<IAuthorize>();
		authorize
			.RefreshAccessTokenAsync(Arg.Any<RefreshToken>())
			.Returns(Task.FromResult(newToken));

		var clock = Substitute.For<ISystemDateTime>();
		clock.UtcNow.Returns(DateTime.UtcNow);

		var maintainer = await IdentityMaintainer.CreateAsync(identity, authorize, clock);
		maintainer.ShouldNotBeNull();

		await authorize.DidNotReceive()
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, SecretString>>>());
		identity.Received(1).Update(newToken);
	}
}
