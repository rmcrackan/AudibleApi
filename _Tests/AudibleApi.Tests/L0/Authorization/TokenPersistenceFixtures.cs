using AudibleApi.Cryptography;
using Dinah.Core.Security;

namespace Authoriz.TokenPersistenceFixtures;

/// <summary>Shared fixtures for legacy / encrypted / hybrid Identity persistence tests.</summary>
internal static class Fixtures
{
	public const string SampleAccessToken = IdentitySerializationTests.IdentitySerialization.SampleAccessToken;
	public const string SampleRefreshToken = IdentitySerializationTests.IdentitySerialization.SampleRefreshToken;
	public const string SampleAdpToken = IdentitySerializationTests.IdentitySerialization.SampleAdpToken;
	public const string SamplePrivateKey = IdentitySerializationTests.IdentitySerialization.SamplePrivateKey;
	public const string SampleStoreAuthCookie = IdentitySerializationTests.IdentitySerialization.SampleStoreAuthCookie;
	public const string SampleCookieName = IdentitySerializationTests.IdentitySerialization.SampleCookieName;
	public const string SampleCookieValue = IdentitySerializationTests.IdentitySerialization.SampleCookieValue;
	public const string SampleCookieName2 = "ubid-main";
	public const string SampleCookieValue2 = "cookie-secret-value-2";
	public static readonly DateTime SampleExpires = IdentitySerializationTests.IdentitySerialization.SampleExpires;

	public static Identity CreateRegisteredIdentity(bool twoCookies = false)
	{
		var cookies = new List<KeyValuePair<string, SecretString>>
		{
			new(SampleCookieName, SampleCookieValue)
		};
		if (twoCookies)
			cookies.Add(new(SampleCookieName2, SampleCookieValue2));

		var identity = new Identity(Localization.Get("us"));
		identity.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken(SampleAccessToken, SampleExpires),
			new RefreshToken(SampleRefreshToken),
			cookies,
			deviceSerialNumber: "device-serial",
			deviceType: "device-type",
			amazonAccountId: "amzn-account",
			deviceName: "device-name",
			storeAuthenticationCookie: SampleStoreAuthCookie);
		return identity;
	}

	public static string LegacyJson()
		=> IdentitySerializationTests.IdentitySerialization.LegacyRegisteredIdentityJson();

	public static void ConfigureEncrypted(string masterKeyName = "audibleapi-tests-master-key")
	{
		var store = new MemoryOsSecretStore();
		var protector = new AesGcmSecretProtector(store, masterKeyName);
		IdentityTokenStorage.Configure(TokenStorageMethod.Encrypted, protector);
	}

	public static void ConfigurePlaintext()
		=> IdentityTokenStorage.Configure(TokenStorageMethod.Plaintext, protector: null);

	public static JObject Serialize(Identity identity)
		=> JObject.Parse(JsonConvert.SerializeObject(identity, Identity.GetJsonSerializerSettings()));

	public static Identity Load(JObject jo)
		=> Identity.FromJson(jo.ToString(Formatting.None));

	public static JObject FullyEncryptedObject(bool twoCookies = false)
	{
		ConfigureEncrypted();
		return Serialize(CreateRegisteredIdentity(twoCookies));
	}

	public static string Protect(string plaintext, string fieldName)
		=> IdentityTokenStorage.Protect(plaintext, $"AudibleApi.Identity|us|{fieldName}");

	public static void AssertAllSecretsUsable(Identity identity)
	{
		identity.IsValid.ShouldBeTrue();
		identity.ExistingAccessToken.TokenValue.ShouldBe(SampleAccessToken);
		identity.ExistingAccessToken.Expires.ShouldBe(SampleExpires);
		identity.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
		identity.AdpToken.ShouldNotBeNull().Value.ShouldBe(SampleAdpToken);
		identity.PrivateKey.ShouldNotBeNull().Value.ShouldBe(SamplePrivateKey);
		identity.StoreAuthenticationCookie.ShouldBe(SampleStoreAuthCookie);
		identity.Cookies.ShouldContain(c => c.Key == SampleCookieName && c.Value == SampleCookieValue);
	}

	public static bool IsEncryptedFlag(JToken? token)
		=> token?["IsEncrypted"]?.Type == JTokenType.Boolean && token["IsEncrypted"]!.Value<bool>();

	public static void AssertNoPlaintextSecretsInJson(JObject jo)
	{
		var json = jo.ToString(Formatting.None);
		json.ShouldNotContain(SampleAccessToken);
		json.ShouldNotContain(SampleRefreshToken);
		json.ShouldNotContain(SampleAdpToken);
		json.ShouldNotContain(SamplePrivateKey.Trim().Substring(0, 40));
		json.ShouldNotContain(SampleStoreAuthCookie);
		json.ShouldNotContain(SampleCookieValue);
	}

	public static string TamperBase64Url(string segment)
	{
		var padded = segment.Replace('-', '+').Replace('_', '/');
		padded += (padded.Length % 4) switch { 2 => "==", 3 => "=", _ => "" };
		var bytes = Convert.FromBase64String(padded);
		bytes[0] ^= 0x5A;
		return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
	}
}
