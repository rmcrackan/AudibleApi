using AudibleApi.Cryptography;
using Authoriz.TokenPersistenceFixtures;

namespace RequestSigningTests;

/// <summary>
/// Signing is the one place where redacting a secret by accident would break the product silently: no compiler
/// error, no exception, no log entry - just every authenticated request rejected by Audible. These tests pin the
/// real values into the header and the signed payload.
/// </summary>
[TestClass]
public class Signing
{
	private static readonly DateTime SampleDate = new(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
	private const string RequestUrl = "https://api.audible.com/1.0/library";

	[TestMethod]
	public void sends_the_real_adp_token_in_the_header()
	{
		var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl);

		request.SignRequest(SampleDate, new AdpToken(Fixtures.SampleAdpToken), new PrivateKey(Fixtures.SamplePrivateKey));

		request.Headers.GetValues("x-adp-token").Single().ShouldBe(Fixtures.SampleAdpToken);
		request.Headers.GetValues("x-adp-token").Single().ShouldNotContain("REDACTED");
	}

	[TestMethod]
	public void signs_a_payload_ending_in_the_real_adp_token()
	{
		var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl);
		var privateKey = new PrivateKey(Fixtures.SamplePrivateKey);
		var date = SampleDate.ToRfc3339String();

		var signature = request.CalculateSignature(SampleDate, new AdpToken(Fixtures.SampleAdpToken), privateKey);

		// rebuilt here rather than asserted as a magic constant, so a change to the payload shape is visible
		var expectedPayload = $"GET\n{RequestUrl}\n{date}\n\n{Fixtures.SampleAdpToken}";
		signature.ShouldBe($"{privateKey.SignMessage(expectedPayload)}:{date}");
	}

	[TestMethod]
	public void a_redacted_token_would_not_produce_the_same_signature()
	{
		var request = new HttpRequestMessage(HttpMethod.Get, RequestUrl);
		var privateKey = new PrivateKey(Fixtures.SamplePrivateKey);
		var date = SampleDate.ToRfc3339String();

		var signature = request.CalculateSignature(SampleDate, new AdpToken(Fixtures.SampleAdpToken), privateKey);

		var redactedPayload = $"GET\n{RequestUrl}\n{date}\n\n{new AdpToken(Fixtures.SampleAdpToken)}";
		signature.ShouldNotBe($"{privateKey.SignMessage(redactedPayload)}:{date}");
	}
}
