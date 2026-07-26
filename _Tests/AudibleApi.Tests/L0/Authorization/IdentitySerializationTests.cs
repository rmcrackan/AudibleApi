using AudibleApi.Cryptography;

namespace Authoriz.IdentitySerializationTests;

/// <summary>
/// Characterization: today's AccountsSettings/Identity tokens are plaintext with no encryption metadata.
/// These tests lock the legacy JSON shape before encryption work lands.
/// </summary>
[TestClass]
[DoNotParallelize]
public class IdentitySerialization
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	public const string SampleAccessToken = "Atna|_CHAR_ACCESS_";
	public const string SampleRefreshToken = "Atnr|_CHAR_REFRESH_";
	public const string SampleAdpToken = "{enc:abcdefg}{key:1234}{iv:56789}{name:QURQVG9rZW5FbmNyeXB0aW9uS2V5}{serial:Mg==}";
	public const string SampleStoreAuthCookie = "store-auth-cookie-value";
	public const string SampleCookieName = "session-id";
	public const string SampleCookieValue = "cookie-secret-value";
	public static readonly DateTime SampleExpires = new(2200, 1, 1, 12, 0, 0, DateTimeKind.Utc);

	// Valid RSA private key used only as fixture material (not a real account secret).
	public const string SamplePrivateKey = @"
-----BEGIN RSA PRIVATE KEY-----
MIIEpgIBAAKCAQEA5nPbGSVDmlEH2tJa6kz/P2HI8IeirhfPHdmi+X/nsb9i3WNf
tmEdZxfK26IValQDXvBH17a1gr0HD6pYse1XsV2w0HxiW1RW+ZnjL8/fzPdkSOb+
4xKlqRopCueBSdDGgAF06spZ3IeHLfEFOJX4dO1Y73pFBUkA0k53LT12L2Tjay/r
buZHJqIzxmwja7/nkiWL0Xo7UySHtQACYsKEatu6yHBS+cPTlGR/qeUpeJTHwDLP
7ZQ7kWzJGY1mfInYekjlZLsMsWswso3pg1vPyHgxzM2BWhY8m6mlXQ9G/USxBTib
MNuMtpR73XsgamneFCc+Uv1cxw7ofZ41YOOAbQIDAQABAoIBAQDIre8HkKm0Aggj
B7df/TjxCsgenR6PF/Cmf9UqC7XJ1W3UeCrq+NrP4aonZJfdhdeBnyAQuuyJMu6p
N6ARISuSKpJEm2xTN7idluJ9yjmLlYtg6LbhKmXUQhGniz3M999DrQERTLDAF80h
tpbjVcWMnPsrX4AnQBFVEjs5zCHU1hD+X463EmUHBWyT975jbZ8Fy7/fTzkdzLnn
qE5lROALr2MCAAwQRFbRE6dd52vnXaBrVcAtRzjATts3WG3+SNi2Fm/OrYqQcY9e
lBexNviT8VcldOAMrO10E2u0d+tvxFzwB3ABMvaVamrEZky4XSfB6aLzpD0JJj1s
UHnIiVwJAoGBAPl8nLll/J9rud/N2HiAX2YkP0MC0HW4yM3KxLtXKyXrP5qBpaci
wTDUmSWEEE3GUJMM1Z4d9tl9Lz2MhU2KqkEvLI3kQ7aUu33PYUBGMVcUzhFQ49lU
Nzz8YB183iqo31o/DKk2Cr5gI7SykQZ0gn/urZkEJeErLzlhPXcyeY5jAoGBAOx4
CGucVdv5MbdXZP8jVzxuvUlSp7BIQJ2phQXDFBNApFKnZn7yBYBx7dqzleymGm+R
INZAurg3SNw4nvbQc3Z2dJ8I+n5ErjFCKp1IedVxx1eMEfecTwrQZuUwLISIyjqF
czSJNwcNqzCx67z397/Cg5K/0pu6uIe0r7xozcbvAoGBAOOvZ9CDVPOg+rdXQvFm
Jqou9lUPonNtOkUlgjl+qfAnK5q0KxvHSgxoWYO1bLOuAybQlbuBmSCPcKd5MMa9
f/eRN9YetfVQ83Mz6YshBDJ22EFRUz+p7eeIY6dFp/PCvmO8Gq/qlA996dglBtmf
RuG+T0vQT0mZgbWaGuBHfkwFAoGBAMOLg1MRxgKRMKavk6pU3EfyP3+J5XemWCDI
1WLtbgV5uClNmzmxBBGypQHs7jbzKPtHpULn5kB+HzdVb0clG8ZDsK7u6s5OF0pO
sBS+oVl7rF/eSeFcFhUYP26ZhsbWo3z/bERuj926VO2AxDPRTsP5o3pQPGZhY0V9
irGgbUJrAoGBAOseS3J4BqYM4R3Hr7cRAhvzSjIkeTcDF1zTOa4FZDHBxZ6g2PNq
8ekhtfn1zPczsPTF1vNuqEISKLxaPkVPiw0mtaZQjVwpF/IOxMNjWVLp6oJf8Mm2
BxlXqPnQ4mG66oqSFQgDEmFdMhRb2of6xL1gYYL62C80G2T7QtmPfSab
-----END RSA PRIVATE KEY-----
";

	public static Identity CreateRegisteredIdentity()
	{
		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken(SampleAccessToken, SampleExpires),
			new RefreshToken(SampleRefreshToken),
			new List<KeyValuePair<string, string?>> { new(SampleCookieName, SampleCookieValue) },
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: SampleStoreAuthCookie);
		return identity;
	}

	/// <summary>Hand-written legacy JSON: no IsEncrypted metadata anywhere.</summary>
	public static string LegacyRegisteredIdentityJson()
	{
		var expires = JsonConvert.SerializeObject(SampleExpires);
		var privateKeyJson = JsonConvert.SerializeObject(SamplePrivateKey);
		return $$"""
			{
			  "LocaleName": "us",
			  "ExistingAccessToken": {
			    "TokenValue": "{{SampleAccessToken}}",
			    "Expires": {{expires}}
			  },
			  "PrivateKey": {
			    "Value": {{privateKeyJson}}
			  },
			  "AdpToken": {
			    "Value": "{{SampleAdpToken}}"
			  },
			  "RefreshToken": {
			    "Value": "{{SampleRefreshToken}}"
			  },
			  "DeviceSerialNumber": "device-serial",
			  "DeviceType": "device-type",
			  "AmazonAccountId": "amzn-account",
			  "DeviceName": "device-name",
			  "StoreAuthenticationCookie": "{{SampleStoreAuthCookie}}",
			  "Cookies": [
			    {
			      "Key": "{{SampleCookieName}}",
			      "Value": "{{SampleCookieValue}}"
			    }
			  ]
			}
			""";
	}

		[TestMethod]
		public void null_json_token_deserializes_as_null_identity()
		{
			var identity = JsonConvert.DeserializeObject<Identity>("null", Identity.GetJsonSerializerSettings());
			identity.ShouldBeNull();
		}

		[TestMethod]
		public void serialize_registered_identity_has_no_IsEncrypted_metadata()
	{
		var json = JsonConvert.SerializeObject(CreateRegisteredIdentity(), Identity.GetJsonSerializerSettings());
		var jo = JObject.Parse(json);

		jo.SelectTokens("$..IsEncrypted").ShouldBeEmpty();
	}

	[TestMethod]
	public void serialize_registered_identity_stores_secret_fields_as_plaintext()
	{
		var json = JsonConvert.SerializeObject(CreateRegisteredIdentity(), Identity.GetJsonSerializerSettings());
		var jo = JObject.Parse(json);

		jo["ExistingAccessToken"]!["TokenValue"]!.Value<string>().ShouldBe(SampleAccessToken);
		jo["RefreshToken"]!["Value"]!.Value<string>().ShouldBe(SampleRefreshToken);
		jo["AdpToken"]!["Value"]!.Value<string>().ShouldBe(SampleAdpToken);
		jo["PrivateKey"]!["Value"]!.Value<string>().ShouldBe(SamplePrivateKey);
		jo["StoreAuthenticationCookie"]!.Value<string>().ShouldBe(SampleStoreAuthCookie);
		jo["Cookies"]![0]!["Value"]!.Value<string>().ShouldBe(SampleCookieValue);
	}

	[TestMethod]
	public void fromJson_legacy_without_IsEncrypted_loads_plaintext_tokens()
	{
		var identity = Identity.FromJson(LegacyRegisteredIdentityJson());

		identity.IsValid.ShouldBeTrue();
		identity.Locale.Name.ShouldBe("us");
		identity.ExistingAccessToken.TokenValue.ShouldBe(SampleAccessToken);
		identity.ExistingAccessToken.Expires.ShouldBe(SampleExpires);
		identity.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
		identity.AdpToken.ShouldNotBeNull().Value.ShouldBe(SampleAdpToken);
		identity.PrivateKey.ShouldNotBeNull().Value.ShouldBe(SamplePrivateKey);
		identity.StoreAuthenticationCookie.ShouldBe(SampleStoreAuthCookie);
		identity.Cookies.Single().Key.ShouldBe(SampleCookieName);
		identity.Cookies.Single().Value.ShouldBe(SampleCookieValue);
	}

	[TestMethod]
	public void roundtrip_preserves_plaintext_token_values()
	{
		var settings = Identity.GetJsonSerializerSettings();
		var json = JsonConvert.SerializeObject(CreateRegisteredIdentity(), settings);
		var reloaded = Identity.FromJson(json);

		reloaded.ExistingAccessToken.TokenValue.ShouldBe(SampleAccessToken);
		reloaded.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
		reloaded.AdpToken.ShouldNotBeNull().Value.ShouldBe(SampleAdpToken);
		reloaded.PrivateKey.ShouldNotBeNull().Value.ShouldBe(SamplePrivateKey);
		reloaded.StoreAuthenticationCookie.ShouldBe(SampleStoreAuthCookie);
		reloaded.Cookies.Single().Value.ShouldBe(SampleCookieValue);
		JObject.Parse(json).SelectTokens("$..IsEncrypted").ShouldBeEmpty();
	}

	[TestMethod]
	public void identityPersister_file_roundtrip_is_plaintext_without_IsEncrypted()
	{
		var path = Path.Combine(Path.GetTempPath(), $"identity-char-{Guid.NewGuid():N}.json");
		try
		{
			using (var persister = new IdentityPersister(CreateRegisteredIdentity(), path))
			{
				persister.Identity.ExistingAccessToken.TokenValue.ShouldBe(SampleAccessToken);
			}

			var onDisk = File.ReadAllText(path);
			var jo = JObject.Parse(onDisk);
			jo.SelectTokens("$..IsEncrypted").ShouldBeEmpty();
			jo["RefreshToken"]!["Value"]!.Value<string>().ShouldBe(SampleRefreshToken);

			using var loaded = new IdentityPersister(path);
			loaded.Identity.ExistingAccessToken.TokenValue.ShouldBe(SampleAccessToken);
			loaded.Identity.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
			loaded.Identity.IsValid.ShouldBeTrue();
		}
		finally
		{
			if (File.Exists(path))
				File.Delete(path);
		}
	}
}
