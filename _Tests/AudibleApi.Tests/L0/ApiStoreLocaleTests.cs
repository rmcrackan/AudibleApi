using AudibleApi.Cryptography;
using Dinah.Core.Net.Http;

namespace ApiStoreLocaleTests;

/// <summary>
/// Records which base addresses an <see cref="Api"/> asks for, so a test can tell the marketplace it reads
/// from apart from the marketplace it authenticates against.
/// </summary>
internal class RecordingSharer : IHttpClientSharer
{
	public List<Uri> Requested { get; } = new();

	private readonly HttpClientSharer _inner = new(HttpMock.GetHandler("{}"));

	public IHttpClientActions GetSharedHttpClient(string target)
		=> GetSharedHttpClient(new Uri(target));

	public IHttpClientActions GetSharedHttpClient(Uri target)
	{
		Requested.Add(target);
		return _inner.GetSharedHttpClient(target);
	}
}

internal class StubIdentityMaintainer : IIdentityMaintainer
{
	public StubIdentityMaintainer(string localeName) => Locale = Localization.Get(localeName);

	public ISystemDateTime SystemDateTime => StaticSystemDateTime.Future;
	public Locale Locale { get; }
	public string? DeviceSerialNumber => "serial";
	public string? DeviceType => "type";
	public string? AmazonAccountId => "amazon id";
	public Task<AccessToken?> GetAccessTokenAsync()
		=> Task.FromResult<AccessToken?>(new AccessToken("Atna|token", new DateTime(2200, 1, 1)));
	public Task<AdpToken?> GetAdpTokenAsync() => throw new NotImplementedException();
	public Task<PrivateKey?> GetPrivateKeyAsync() => throw new NotImplementedException();
}

[TestClass]
public class StoreLocale
{
	[TestMethod]
	public async Task defaults_to_the_identitys_own_marketplace()
	{
		var sharer = new RecordingSharer();
		var api = new Api(new StubIdentityMaintainer("us"), sharer);

		await api.AdHocNonAuthenticatedGetAsync("/1.0/library");

		sharer.Requested.ShouldContain(new Uri("https://api.audible.com"));
	}

	[TestMethod]
	public async Task sends_store_calls_to_the_store_marketplace()
	{
		var sharer = new RecordingSharer();
		// a 'ca' registration reading the 'us' storefront: the case this whole feature exists for
		var api = new Api(new StubIdentityMaintainer("ca"), Localization.Get("us"), sharer);

		await api.AdHocNonAuthenticatedGetAsync("/1.0/library");

		sharer.Requested.ShouldContain(new Uri("https://api.audible.com"));
		sharer.Requested.ShouldNotContain(new Uri("https://api.audible.ca"));
	}

	[TestMethod]
	public async Task keeps_identity_calls_on_the_registered_marketplace()
	{
		var sharer = new RecordingSharer();
		var api = new Api(new StubIdentityMaintainer("ca"), Localization.Get("us"), sharer);

		// /user/profile proves who you are, so it belongs to the marketplace that issued the tokens
		await api.UserProfileAsync();

		sharer.Requested.ShouldContain(new Uri("https://api.amazon.ca"));
		sharer.Requested.ShouldNotContain(new Uri("https://api.amazon.com"));
	}

	[TestMethod]
	public async Task a_null_store_locale_means_no_change_at_all()
	{
		var sharer = new RecordingSharer();
		var api = new Api(new StubIdentityMaintainer("uk"), storeLocale: null, sharer);

		await api.AdHocNonAuthenticatedGetAsync("/1.0/library");

		sharer.Requested.ShouldContain(new Uri("https://api.audible.co.uk"));
	}

	[TestMethod]
	public void each_marketplace_gets_its_own_client()
	{
		var sharer = new HttpClientSharer(HttpMock.GetHandler("{}"));

		var us = sharer.GetSharedHttpClient(Localization.Get("us").AudibleApiUri());
		var ca = sharer.GetSharedHttpClient(Localization.Get("ca").AudibleApiUri());

		us.ShouldNotBe(ca);
	}
}
