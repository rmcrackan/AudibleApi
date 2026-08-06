using AudibleApi.Authorization;
using Dinah.Core;
using System.Collections.Generic;

namespace Authoriz.IdentityMaintainerTests;

[TestClass]
public class EnsureStateAsync
{
	[TestMethod]
	public async Task refresh_failure_is_preserved_when_deregister_recovery_fails()
	{
		var refreshException = new InvalidOperationException("simulated refresh failure");

		var identity = Substitute.For<IIdentity>();
		identity.IsValid.Returns(true);
		identity.Locale.Returns(Locales.Us);
		identity.RefreshToken.Returns(new RefreshToken("Atnr|test-refresh"));
		identity.ExistingAccessToken.Returns(new AccessToken("Atna|expired", DateTime.UtcNow.AddHours(-2)));
		identity.Cookies.Returns(Array.Empty<KeyValuePair<string, string?>>());

		var authorize = Substitute.For<IAuthorize>();
		authorize
			.RefreshAccessTokenAsync(Arg.Any<RefreshToken>())
			.Returns<Task<AccessToken>>(_ => throw refreshException);
		authorize
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, string?>>>())
			.Returns(false);

		var clock = Substitute.For<ISystemDateTime>();
		clock.UtcNow.Returns(DateTime.UtcNow);

		var ex = await Should.ThrowAsync<RegistrationException>(
			() => IdentityMaintainer.CreateAsync(identity, authorize, clock));

		ex.Message.ShouldBe("Error ensuring valid state");
		ex.InnerException.ShouldBeOfType<AggregateException>();

		var flat = Flatten(ex).ToList();
		flat.ShouldContain(refreshException);
		flat.Any(e => e is RegistrationException && e.Message == "Unable to deregister").ShouldBeTrue();
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
			.DeregisterAsync(Arg.Any<AccessToken>(), Arg.Any<IEnumerable<KeyValuePair<string, string?>>>());
		identity.Received(1).Update(newToken);
	}

	private static IEnumerable<Exception> Flatten(Exception ex)
	{
		var stack = new Stack<Exception>();
		stack.Push(ex);
		while (stack.Count > 0)
		{
			var current = stack.Pop();
			yield return current;

			if (current is AggregateException aggregate)
			{
				foreach (var inner in aggregate.InnerExceptions)
					stack.Push(inner);
			}
			else if (current.InnerException is not null)
			{
				stack.Push(current.InnerException);
			}
		}
	}
}
