using AudibleApi.Authorization;
using AudibleApi.Common;
using AudibleApi.Cryptography;
using Authoriz.TokenPersistenceFixtures;
using Dinah.Core.Net.Http;

namespace AudibleApi.Tests.L0;

[TestClass]
public class AuthenticationMethodsTests
{
	private const string SampleAccessToken = "Atna|access_token_characterization";

	private class TestIdentityMaintainer : IIdentityMaintainer
	{
		public ISystemDateTime SystemDateTime => StaticSystemDateTime.Future;
		public Locale Locale => Localization.Get("us");
		public string? DeviceSerialNumber => "serial";
		public string? DeviceType => "type";
		public string? AmazonAccountId => "amazon id";

		public virtual Task<AccessToken?> GetAccessTokenAsync()
			=> Task.FromResult<AccessToken?>(new AccessToken(SampleAccessToken, new DateTime(2200, 1, 1)));

		public virtual Task<AdpToken?> GetAdpTokenAsync()
			=> Task.FromResult<AdpToken?>(new AdpToken(Fixtures.SampleAdpToken));

		public virtual Task<PrivateKey?> GetPrivateKeyAsync()
			=> Task.FromResult<PrivateKey?>(new PrivateKey(Fixtures.SamplePrivateKey));
	}

	private class EmptyTokenIdentityMaintainer : TestIdentityMaintainer
	{
		public override Task<AccessToken?> GetAccessTokenAsync()
			=> Task.FromResult<AccessToken?>(AccessToken.Empty);
	}

	private class NullTokenIdentityMaintainer : TestIdentityMaintainer
	{
		public override Task<AccessToken?> GetAccessTokenAsync()
			=> Task.FromResult<AccessToken?>(null);
	}

	private static Api CreateTestApi(ApiAuthenticationMethod method, IIdentityMaintainer? maintainer = null)
	{
		var sharer = new HttpClientSharer(HttpMock.GetHandler("{}"));
		return new Api(maintainer ?? new TestIdentityMaintainer(), sharer)
		{
			AuthenticationMethods = method
		};
	}

	[TestMethod]
	public void defaults_to_AdpSignature_and_BearerToken()
	{
		var sharer = new HttpClientSharer(HttpMock.GetHandler("{}"));
		var api = new Api(new TestIdentityMaintainer(), sharer);
		api.AuthenticationMethods.ShouldBe(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken);
	}

	[TestMethod]
	public async Task apply_adp_signature_only()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.AdpSignature);
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		await api.ApplyAuthenticationAsync(request);

		request.Headers.Contains("x-adp-token").ShouldBeTrue();
		request.Headers.Contains("x-adp-alg").ShouldBeTrue();
		request.Headers.Contains("x-adp-signature").ShouldBeTrue();
		request.Headers.Contains("x-amz-access-token").ShouldBeFalse();
	}

	[TestMethod]
	public async Task apply_bearer_token_only()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.BearerToken);
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		await api.ApplyAuthenticationAsync(request);

		request.Headers.Contains("x-adp-token").ShouldBeFalse();
		request.Headers.Contains("x-adp-alg").ShouldBeFalse();
		request.Headers.Contains("x-adp-signature").ShouldBeFalse();
		request.Headers.GetValues("x-amz-access-token").Single().ShouldBe(SampleAccessToken);
	}

	[TestMethod]
	public async Task apply_adp_and_bearer_token()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken);
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		await api.ApplyAuthenticationAsync(request);

		request.Headers.Contains("x-adp-token").ShouldBeTrue();
		request.Headers.Contains("x-adp-alg").ShouldBeTrue();
		request.Headers.Contains("x-adp-signature").ShouldBeTrue();
		request.Headers.GetValues("x-amz-access-token").Single().ShouldBe(SampleAccessToken);
	}

	[TestMethod]
	public async Task apply_none()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.None);
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		await api.ApplyAuthenticationAsync(request);

		request.Headers.Contains("x-adp-token").ShouldBeFalse();
		request.Headers.Contains("x-adp-alg").ShouldBeFalse();
		request.Headers.Contains("x-adp-signature").ShouldBeFalse();
		request.Headers.Contains("x-amz-access-token").ShouldBeFalse();
	}

	[TestMethod]
	public void identity_has_required_tokens_checks_method_requirements()
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(Fixtures.SamplePrivateKey),
			new AdpToken(Fixtures.SampleAdpToken),
			new AccessToken(SampleAccessToken, new DateTime(2200, 1, 1)),
			new RefreshToken("Atnr|refresh"),
			null);

		identity.HasRequiredTokens(ApiAuthenticationMethod.None).ShouldBeTrue();
		identity.HasRequiredTokens(ApiAuthenticationMethod.AdpSignature).ShouldBeTrue();
		identity.HasRequiredTokens(ApiAuthenticationMethod.BearerToken).ShouldBeTrue();
		identity.HasRequiredTokens(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken).ShouldBeTrue();

		// Identity without tokens
		var emptyIdentity = new Identity(Localization.Get("us"));
		emptyIdentity.HasRequiredTokens(ApiAuthenticationMethod.None).ShouldBeTrue();
		emptyIdentity.HasRequiredTokens(ApiAuthenticationMethod.AdpSignature).ShouldBeFalse();
		emptyIdentity.HasRequiredTokens(ApiAuthenticationMethod.BearerToken).ShouldBeFalse();
		emptyIdentity.HasRequiredTokens(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken).ShouldBeFalse();

		// Invalidate clears AdpToken and RefreshToken
		identity.Invalidate();
		identity.HasRequiredTokens(ApiAuthenticationMethod.None).ShouldBeTrue();
		identity.HasRequiredTokens(ApiAuthenticationMethod.AdpSignature).ShouldBeFalse();
		identity.HasRequiredTokens(ApiAuthenticationMethod.BearerToken).ShouldBeFalse();
		identity.HasRequiredTokens(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken).ShouldBeFalse();

		// Identity with valid RefreshToken but empty AccessToken can still satisfy BearerToken
		var identityWithRefreshOnly = new Identity(Localization.Get("us"));
		identityWithRefreshOnly.Update(
			new PrivateKey(Fixtures.SamplePrivateKey),
			new AdpToken(Fixtures.SampleAdpToken),
			AccessToken.Empty,
			new RefreshToken("Atnr|refresh"),
			null);
		identityWithRefreshOnly.HasRequiredTokens(ApiAuthenticationMethod.BearerToken).ShouldBeTrue();
	}

	[TestMethod]
	public async Task api_has_required_tokens_async_checks_maintainer_tokens()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken);
		(await api.HasRequiredTokensAsync(ApiAuthenticationMethod.AdpSignature)).ShouldBeTrue();
		(await api.HasRequiredTokensAsync(ApiAuthenticationMethod.BearerToken)).ShouldBeTrue();
		(await api.HasRequiredTokensAsync(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken)).ShouldBeTrue();
		(await api.HasRequiredTokensAsync((ApiAuthenticationMethod)8)).ShouldBeFalse();
	}

	[TestMethod]
	public async Task apply_adp_and_bearer_with_empty_access_token_does_not_attach_empty_bearer_header()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, new EmptyTokenIdentityMaintainer());
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		await api.ApplyAuthenticationAsync(request);

		request.Headers.Contains("x-adp-token").ShouldBeTrue();
		request.Headers.Contains("x-adp-alg").ShouldBeTrue();
		request.Headers.Contains("x-adp-signature").ShouldBeTrue();
		request.Headers.Contains("x-amz-access-token").ShouldBeFalse();
	}

	[TestMethod]
	public async Task apply_bearer_only_with_empty_access_token_throws()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.BearerToken, new EmptyTokenIdentityMaintainer());
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => api.ApplyAuthenticationAsync(request));
		ex.Message.ShouldContain("Identity maintainer access token is null or empty");
	}

	[TestMethod]
	public async Task apply_bearer_only_with_null_access_token_throws()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.BearerToken, new NullTokenIdentityMaintainer());
		var request = new HttpRequestMessage(HttpMethod.Get, "https://api.audible.com/1.0/library");

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => api.ApplyAuthenticationAsync(request));
		ex.Message.ShouldContain("Identity maintainer access token is null or empty");
	}

	[TestMethod]
	public async Task ad_hoc_authenticated_get_with_access_token_throws_if_empty()
	{
		var sharer = new HttpClientSharer(HttpMock.GetHandler("{}"));
		var api = new Api(new EmptyTokenIdentityMaintainer(), sharer)
		{
			AuthenticationMethods = ApiAuthenticationMethod.BearerToken
		};

		var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => api.AdHocAuthenticatedGetWithAccessTokenAsync("https://api.audible.com/1.0/library", sharer.GetSharedHttpClient("https://api.audible.com")));
		ex.Message.ShouldContain("Identity maintainer access token is empty");
	}

	[TestMethod]
	public async Task has_required_tokens_async_with_empty_access_token_returns_false()
	{
		var api = CreateTestApi(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken, new EmptyTokenIdentityMaintainer());
		(await api.HasRequiredTokensAsync(ApiAuthenticationMethod.AdpSignature)).ShouldBeTrue();
		(await api.HasRequiredTokensAsync(ApiAuthenticationMethod.BearerToken)).ShouldBeFalse();
		(await api.HasRequiredTokensAsync(ApiAuthenticationMethod.AdpSignature | ApiAuthenticationMethod.BearerToken)).ShouldBeFalse();
	}
}
