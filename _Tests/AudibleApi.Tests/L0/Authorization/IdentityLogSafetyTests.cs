using System.Collections;
using System.Reflection;
using System.Text;
using Authoriz.TokenPersistenceFixtures;

namespace Authoriz.IdentityLogSafetyTests;

/// <summary>
/// Logging reaches a secret by reflecting over public properties, never by calling <see cref="object.ToString"/>:
/// that is true of Serilog's structured destructuring and of Serilog.Exceptions walking the public properties of
/// a logged exception. A redacting ToString is no defense against either. These tests walk an <see cref="Identity"/>
/// the same way a logger would and assert the plaintext is not reachable.
/// </summary>
[TestClass]
public class ReflectiveDump
{
	[TestMethod]
	public void finds_no_token_plaintext()
	{
		var dump = ReflectPublicValues(Fixtures.CreateRegisteredIdentity());

		// proves the walk actually reached the identity's contents, so the assertions below are not vacuous
		dump.ShouldContain("device-serial");

		dump.ShouldNotContain(Fixtures.SampleAccessToken);
		dump.ShouldNotContain(Fixtures.SampleRefreshToken);
		dump.ShouldNotContain(Fixtures.SampleAdpToken);
		dump.ShouldNotContain(Fixtures.SamplePrivateKey.Trim()[..40]);
	}

	[TestMethod]
	public void finds_no_cookie_plaintext()
	{
		var dump = ReflectPublicValues(Fixtures.CreateRegisteredIdentity(twoCookies: true));

		// a cookie's name is not a secret, and staying readable is what makes a log useful
		dump.ShouldContain(Fixtures.SampleCookieName);

		dump.ShouldNotContain(Fixtures.SampleCookieValue);
		dump.ShouldNotContain(Fixtures.SampleCookieValue2);
		dump.ShouldNotContain(Fixtures.SampleStoreAuthCookie);
	}

	[TestMethod]
	public void finds_no_token_plaintext_on_the_tokens_themselves()
	{
		ReflectPublicValues(new RefreshToken(Fixtures.SampleRefreshToken))
			.ShouldNotContain(Fixtures.SampleRefreshToken);
		ReflectPublicValues(new AdpToken(Fixtures.SampleAdpToken))
			.ShouldNotContain(Fixtures.SampleAdpToken);
		ReflectPublicValues(new AudibleApi.Cryptography.PrivateKey(Fixtures.SamplePrivateKey))
			.ShouldNotContain(Fixtures.SamplePrivateKey.Trim()[..40]);
		ReflectPublicValues(new AccessToken(Fixtures.SampleAccessToken, Fixtures.SampleExpires))
			.ShouldNotContain(Fixtures.SampleAccessToken);
	}

	/// <summary>
	/// Mimics Serilog.Exceptions' ReflectionBasedDestructurer: public instance properties, recursively, to its
	/// default depth of 10, collecting everything a log line could end up containing.
	/// </summary>
	private static string ReflectPublicValues(object root, int maxDepth = 10)
	{
		var collected = new StringBuilder();
		var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

		void walk(object? value, int depth)
		{
			if (value is null || depth > maxDepth)
				return;

			if (value is string text)
			{
				collected.AppendLine(text);
				return;
			}

			var type = value.GetType();
			if (type.IsPrimitive || type.IsEnum || value is DateTime or TimeSpan or Uri)
			{
				collected.AppendLine(value.ToString());
				return;
			}

			if (value is IEnumerable items)
			{
				foreach (var item in items)
					walk(item, depth + 1);
				return;
			}

			// reference types can form cycles; boxed structs are always distinct so the depth limit bounds them
			if (!type.IsValueType && !seen.Add(value))
				return;

			foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				if (property.GetIndexParameters().Length > 0)
					continue;

				object? propertyValue;
				try
				{
					propertyValue = property.GetValue(value);
				}
				catch
				{
					// a throwing getter is recorded as the exception message by real destructurers, never as a value
					continue;
				}

				walk(propertyValue, depth + 1);
			}
		}

		walk(root, 0);
		return collected.ToString();
	}
}
