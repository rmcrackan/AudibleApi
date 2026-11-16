using AudibleApi.Cryptography;

namespace AudibleApi.Tests.L0;

[TestClass]
public class AaxWorkaroundTests
{
	private const string Asin = "B000000001";
	private const string DownloadUrl = "https://cds.audible.com/download?asin=B000000001&codec=LC_128_44100_stereo";

	private class StubIdentityMaintainer : IIdentityMaintainer
	{
		public ISystemDateTime SystemDateTime => StaticSystemDateTime.Future;
		public Locale Locale => Localization.Get("us");
		public string? DeviceSerialNumber => "serial";
		public string? DeviceType => "type";
		public string? AmazonAccountId => "amazon id";
		public Task<AccessToken?> GetAccessTokenAsync()
			=> Task.FromResult<AccessToken?>(new AccessToken("Atna|aax_workaround", new DateTime(2200, 1, 1)));
		public Task<AdpToken?> GetAdpTokenAsync() => throw new NotImplementedException();
		public Task<PrivateKey?> GetPrivateKeyAsync() => throw new NotImplementedException();
	}

	/// <summary>
	/// Refuses every license request with the Sable error Audible returns for Widevine-only titles, answers the
	/// library lookup with the given codecs, and the AAX download request with a redirect.
	/// </summary>
	private class FakeAudible(string availableCodecsJson) : HttpMessageHandler
	{
		public List<Uri> Requests { get; } = new();

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var uri = request.RequestUri!;
			Requests.Add(uri);

			var response = uri.AbsolutePath switch
			{
				var p when p.EndsWith("/licenserequest") => new HttpResponseMessage(HttpStatusCode.BadRequest)
				{
					Content = new StringContent($$"""{"error_code":"000307","message":"Unable to retrieve asset details from Sable(AssetInfos), for marketplaceId:ATVPDKIKX0DER, asin:{{Asin}}, acr:null"}""")
				},
				var p when p.StartsWith("/FionaCDEServiceEngine/FSDownloadContent") => new HttpResponseMessage(HttpStatusCode.Found)
				{
					Headers = { Location = new Uri(DownloadUrl) }
				},
				_ => new HttpResponseMessage(HttpStatusCode.OK)
				{
					Content = new StringContent($$$"""{"item":{"asin":"{{{Asin}}}","available_codecs":{{{availableCodecsJson}}}}}""")
				},
			};
			response.RequestMessage = request;
			return Task.FromResult(response);
		}
	}

	private static (Api, FakeAudible) CreateApi(string availableCodecsJson)
	{
		var audible = new FakeAudible(availableCodecsJson);
		var api = new Api(new StubIdentityMaintainer(), new HttpClientSharer(audible))
		{
			AuthenticationMethods = ApiAuthenticationMethod.BearerToken
		};
		return (api, audible);
	}

	[TestMethod]
	public async Task returns_download_url_when_book_lists_codecs()
	{
		var (api, audible) = CreateApi("""[{"enhanced_codec":"LC_64_22050_stereo"},{"enhanced_codec":"LC_128_44100_stereo"}]""");

		var url = await api.DownloadAaxWorkaroundAsync(Asin);

		url.ShouldBe(DownloadUrl);
		audible.Requests.Count(r => r.AbsolutePath.StartsWith("/1.0/library/")).ShouldBe(1);
		audible.Requests.Last().Query.ShouldContain("codec=LC_128_44100_stereo");
	}

	[TestMethod]
	public async Task throws_when_book_lists_no_codecs()
	{
		var (api, audible) = CreateApi("[]");

		await Should.ThrowAsync<ApplicationException>(() => api.DownloadAaxWorkaroundAsync(Asin));

		// one retry with every response group, and no download request
		audible.Requests.Count.ShouldBe(2);
		audible.Requests.ShouldAllBe(r => r.AbsolutePath.StartsWith("/1.0/library/"));
	}

	[TestMethod]
	public async Task license_request_error_is_rethrown_without_trying_aax()
	{
		var (api, audible) = CreateApi("""[{"enhanced_codec":"LC_128_44100_stereo"}]""");

		var ex = await Should.ThrowAsync<ApiErrorException>(() => api.GetDownloadLicenseAsync(Asin));

		ex.JsonMessage.ShouldNotBeNull().ShouldContain("000307");
		audible.Requests.ShouldHaveSingleItem().AbsolutePath.ShouldEndWith("/licenserequest");
	}
}
