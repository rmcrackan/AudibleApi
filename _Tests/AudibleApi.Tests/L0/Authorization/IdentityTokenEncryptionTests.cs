using AudibleApi.Cryptography;
using static Authoriz.TokenPersistenceFixtures.Fixtures;

namespace Authoriz.IdentityTokenEncryptionTests;

[TestClass]
[DoNotParallelize]
public class LegacyAndExplicitPlaintext
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void legacy_missing_IsEncrypted_loads_all_secrets_as_plaintext()
	{
		var identity = Identity.FromJson(LegacyJson());
		AssertAllSecretsUsable(identity);
		identity.SecretPersistence.AccessToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.RefreshToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.AdpToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.PrivateKey.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.StoreAuthenticationCookie.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.CookieLoadedEncrypted.ShouldAllBe(x => x == false);
	}

	[TestMethod]
	public void explicit_IsEncrypted_false_on_all_object_secrets_loads_plaintext()
	{
		var jo = JObject.Parse(LegacyJson());
		foreach (var path in new[] { "RefreshToken", "AdpToken", "PrivateKey" })
			jo[path]!["IsEncrypted"] = false;
		jo["ExistingAccessToken"]!["IsEncrypted"] = false;
		jo["StoreAuthenticationCookie"] = new JObject
		{
			["Value"] = SampleStoreAuthCookie,
			["IsEncrypted"] = false
		};
		jo["Cookies"]![0]!["Value"] = new JObject
		{
			["Value"] = SampleCookieValue,
			["IsEncrypted"] = false
		};

		var identity = Load(jo);
		AssertAllSecretsUsable(identity);
		identity.SecretPersistence.RefreshToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.AccessToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.StoreAuthenticationCookie.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.CookieLoadedEncrypted[0].ShouldBeFalse();
	}

	[TestMethod]
	public void plaintext_write_method_roundtrip_omits_IsEncrypted()
	{
		ConfigurePlaintext();
		var jo = Serialize(CreateRegisteredIdentity());
		jo.SelectTokens("$..IsEncrypted").ShouldBeEmpty();
		AssertAllSecretsUsable(Load(jo));
	}

	[TestMethod]
	public void legacy_persister_file_roundtrip_remains_plaintext()
	{
		ConfigurePlaintext();
		var path = Path.Combine(Path.GetTempPath(), $"identity-legacy-{Guid.NewGuid():N}.json");
		try
		{
			File.WriteAllText(path, LegacyJson());
			using (var loaded = new IdentityPersister(path))
				AssertAllSecretsUsable(loaded.Identity);

			using (var loaded = new IdentityPersister(path))
			{
				loaded.Identity.Update(new AccessToken("Atna|_LEGACY_REFRESH_", SampleExpires));
			}

			var onDisk = JObject.Parse(File.ReadAllText(path));
			onDisk["ExistingAccessToken"]!["TokenValue"]!.Value<string>().ShouldBe("Atna|_LEGACY_REFRESH_");
			(onDisk["ExistingAccessToken"]!["IsEncrypted"]?.Value<bool>() ?? false).ShouldBeFalse();
			onDisk["RefreshToken"]!["Value"]!.Value<string>().ShouldBe(SampleRefreshToken);
			onDisk.SelectTokens("$..IsEncrypted[?(@ == true)]").ShouldBeEmpty();
		}
		finally
		{
			if (File.Exists(path))
				File.Delete(path);
		}
	}
}

[TestClass]
[DoNotParallelize]
public class EncryptedPersistence
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void encrypted_roundtrip_all_secrets_usable_and_not_persisted_plaintext()
	{
		var jo = FullyEncryptedObject();
		AssertNoPlaintextSecretsInJson(jo);

		IsEncryptedFlag(jo["RefreshToken"]).ShouldBeTrue();
		IsEncryptedFlag(jo["AdpToken"]).ShouldBeTrue();
		IsEncryptedFlag(jo["PrivateKey"]).ShouldBeTrue();
		IsEncryptedFlag(jo["ExistingAccessToken"]).ShouldBeTrue();
		IsEncryptedFlag(jo["StoreAuthenticationCookie"]).ShouldBeTrue();
		IsEncryptedFlag(jo["Cookies"]![0]!["Value"]).ShouldBeTrue();

		AssertAllSecretsUsable(Load(jo));
	}

	[TestMethod]
	public void encrypted_persister_file_roundtrip()
	{
		ConfigureEncrypted();
		var path = Path.Combine(Path.GetTempPath(), $"identity-enc-{Guid.NewGuid():N}.json");
		try
		{
			using (new IdentityPersister(CreateRegisteredIdentity(), path)) { }

			var onDisk = JObject.Parse(File.ReadAllText(path));
			AssertNoPlaintextSecretsInJson(onDisk);

			using var loaded = new IdentityPersister(path);
			AssertAllSecretsUsable(loaded.Identity);
		}
		finally
		{
			if (File.Exists(path))
				File.Delete(path);
		}
	}

	[TestMethod]
	public void encrypted_resave_without_updates_keeps_all_fields_encrypted_and_usable()
	{
		var first = FullyEncryptedObject();
		var loaded = Load(first);
		var second = Serialize(loaded);

		IsEncryptedFlag(second["RefreshToken"]).ShouldBeTrue();
		IsEncryptedFlag(second["ExistingAccessToken"]).ShouldBeTrue();
		AssertAllSecretsUsable(Load(second));
	}

	[TestMethod]
	public void access_token_expires_remains_plaintext_when_token_encrypted()
	{
		var jo = FullyEncryptedObject();
		jo["ExistingAccessToken"]!["Expires"]!.Value<DateTime>().ShouldBe(SampleExpires);
		IsEncryptedFlag(jo["ExistingAccessToken"]).ShouldBeTrue();
	}
}

[TestClass]
[DoNotParallelize]
public class HybridPersistence
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	[DataRow("RefreshToken")]
	[DataRow("AdpToken")]
	[DataRow("PrivateKey")]
	public void hybrid_one_strongtype_plaintext_rest_encrypted_is_usable(string plaintextField)
	{
		var jo = FullyEncryptedObject();
		var plaintext = plaintextField switch
		{
			"RefreshToken" => SampleRefreshToken,
			"AdpToken" => SampleAdpToken,
			"PrivateKey" => SamplePrivateKey,
			_ => throw new ArgumentOutOfRangeException(nameof(plaintextField))
		};
		jo[plaintextField] = new JObject { ["Value"] = plaintext };

		var identity = Load(jo);
		AssertAllSecretsUsable(identity);
		FieldLoadedEncrypted(identity, plaintextField).ShouldBeFalse();
		identity.SecretPersistence.AccessToken.LoadedEncrypted.ShouldBeTrue();
	}

	[TestMethod]
	public void hybrid_access_token_plaintext_rest_encrypted_is_usable()
	{
		var jo = FullyEncryptedObject();
		jo["ExistingAccessToken"] = new JObject
		{
			["TokenValue"] = SampleAccessToken,
			["Expires"] = SampleExpires
		};

		var identity = Load(jo);
		AssertAllSecretsUsable(identity);
		identity.SecretPersistence.AccessToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.RefreshToken.LoadedEncrypted.ShouldBeTrue();
	}

	[TestMethod]
	public void hybrid_store_auth_cookie_legacy_string_with_encrypted_tokens_is_usable()
	{
		var jo = FullyEncryptedObject();
		jo["StoreAuthenticationCookie"] = SampleStoreAuthCookie;

		var identity = Load(jo);
		AssertAllSecretsUsable(identity);
		identity.SecretPersistence.StoreAuthenticationCookie.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.RefreshToken.LoadedEncrypted.ShouldBeTrue();
	}

	[TestMethod]
	public void hybrid_mixed_cookie_values_are_usable()
	{
		ConfigureEncrypted();
		var jo = Serialize(CreateRegisteredIdentity(twoCookies: true));

		// Leave first cookie encrypted; force second cookie to plaintext string.
		jo["Cookies"]![1]!["Value"] = SampleCookieValue2;

		var identity = Load(jo);
		identity.Cookies.Count().ShouldBe(2);
		identity.Cookies.Single(c => c.Key == SampleCookieName).Value.ShouldBe(SampleCookieValue);
		identity.Cookies.Single(c => c.Key == SampleCookieName2).Value.ShouldBe(SampleCookieValue2);
		identity.SecretPersistence.CookieLoadedEncrypted[0].ShouldBeTrue();
		identity.SecretPersistence.CookieLoadedEncrypted[1].ShouldBeFalse();
		identity.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
	}

	[TestMethod]
	public void hybrid_only_refresh_token_encrypted_in_legacy_file_is_usable()
	{
		ConfigureEncrypted();
		var jo = JObject.Parse(LegacyJson());
		jo["RefreshToken"] = new JObject
		{
			["Value"] = Protect(SampleRefreshToken, "RefreshToken"),
			["IsEncrypted"] = true
		};

		var identity = Load(jo);
		AssertAllSecretsUsable(identity);
		identity.SecretPersistence.RefreshToken.LoadedEncrypted.ShouldBeTrue();
		identity.SecretPersistence.AccessToken.LoadedEncrypted.ShouldBeFalse();
		identity.SecretPersistence.PrivateKey.LoadedEncrypted.ShouldBeFalse();
	}

	[TestMethod]
	[DataRow("RefreshToken")]
	[DataRow("AdpToken")]
	[DataRow("PrivateKey")]
	[DataRow("ExistingAccessToken")]
	[DataRow("StoreAuthenticationCookie")]
	public void hybrid_only_one_field_encrypted_rest_legacy_plaintext_is_usable(string encryptedField)
	{
		ConfigureEncrypted();
		var jo = JObject.Parse(LegacyJson());

		switch (encryptedField)
		{
			case "RefreshToken":
				jo["RefreshToken"] = EncryptedValueObject(SampleRefreshToken, "RefreshToken");
				break;
			case "AdpToken":
				jo["AdpToken"] = EncryptedValueObject(SampleAdpToken, "AdpToken");
				break;
			case "PrivateKey":
				jo["PrivateKey"] = EncryptedValueObject(SamplePrivateKey, "PrivateKey");
				break;
			case "ExistingAccessToken":
				jo["ExistingAccessToken"] = new JObject
				{
					["TokenValue"] = Protect(SampleAccessToken, "ExistingAccessToken"),
					["Expires"] = SampleExpires,
					["IsEncrypted"] = true
				};
				break;
			case "StoreAuthenticationCookie":
				jo["StoreAuthenticationCookie"] = EncryptedValueObject(SampleStoreAuthCookie, "StoreAuthenticationCookie");
				break;
		}

		var identity = Load(jo);
		AssertAllSecretsUsable(identity);
		FieldLoadedEncrypted(identity, encryptedField).ShouldBeTrue();
	}

	[TestMethod]
	public void hybrid_file_is_not_treated_as_corrupt()
	{
		var jo = FullyEncryptedObject();
		jo["RefreshToken"] = new JObject { ["Value"] = SampleRefreshToken };
		jo["AdpToken"] = new JObject { ["Value"] = SampleAdpToken, ["IsEncrypted"] = false };

		var identity = Load(jo);
		identity.IsValid.ShouldBeTrue();
		AssertAllSecretsUsable(identity);
	}

	static JObject EncryptedValueObject(string plaintext, string fieldName)
		=> new()
		{
			["Value"] = Protect(plaintext, fieldName),
			["IsEncrypted"] = true
		};

	static bool FieldLoadedEncrypted(Identity identity, string fieldName)
		=> fieldName switch
		{
			"RefreshToken" => identity.SecretPersistence.RefreshToken.LoadedEncrypted,
			"AdpToken" => identity.SecretPersistence.AdpToken.LoadedEncrypted,
			"PrivateKey" => identity.SecretPersistence.PrivateKey.LoadedEncrypted,
			"ExistingAccessToken" => identity.SecretPersistence.AccessToken.LoadedEncrypted,
			"StoreAuthenticationCookie" => identity.SecretPersistence.StoreAuthenticationCookie.LoadedEncrypted,
			_ => throw new ArgumentOutOfRangeException(nameof(fieldName))
		};
}

[TestClass]
[DoNotParallelize]
public class PartialUpdatePreservation
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void refresh_access_token_to_plaintext_preserves_encrypted_siblings()
	{
		var encryptedJson = FullyEncryptedObject();
		var loaded = Load(encryptedJson);

		// Keep protector: encrypted siblings must still be rewritten encrypted.
		IdentityTokenStorage.Configure(TokenStorageMethod.Plaintext, IdentityTokenStorage.Protector);
		loaded.Update(new AccessToken("Atna|_REFRESHED_", SampleExpires));
		var after = Serialize(loaded);

		after["ExistingAccessToken"]!["TokenValue"]!.Value<string>().ShouldBe("Atna|_REFRESHED_");
		(after["ExistingAccessToken"]!["IsEncrypted"]?.Value<bool>() ?? false).ShouldBeFalse();

		IsEncryptedFlag(after["RefreshToken"]).ShouldBeTrue();
		IsEncryptedFlag(after["AdpToken"]).ShouldBeTrue();
		IsEncryptedFlag(after["PrivateKey"]).ShouldBeTrue();
		IsEncryptedFlag(after["StoreAuthenticationCookie"]).ShouldBeTrue();
		IsEncryptedFlag(after["Cookies"]![0]!["Value"]).ShouldBeTrue();

		// Same protector/master key is still configured; only WriteMethod changed.
		var reloaded = Load(after);
		reloaded.ExistingAccessToken.TokenValue.ShouldBe("Atna|_REFRESHED_");
		reloaded.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
		reloaded.PrivateKey.ShouldNotBeNull().Value.ShouldBe(SamplePrivateKey);
	}

	[TestMethod]
	public void refresh_access_token_to_encrypted_preserves_plaintext_siblings()
	{
		ConfigurePlaintext();
		var legacyLoaded = Identity.FromJson(LegacyJson());

		ConfigureEncrypted();
		legacyLoaded.Update(new AccessToken("Atna|_NEW_ENC_", SampleExpires));
		var after = Serialize(legacyLoaded);

		IsEncryptedFlag(after["ExistingAccessToken"]).ShouldBeTrue();
		after["ExistingAccessToken"]!["TokenValue"]!.Value<string>().ShouldNotBe("Atna|_NEW_ENC_");

		(after["RefreshToken"]!["IsEncrypted"]?.Value<bool>() ?? false).ShouldBeFalse();
		after["RefreshToken"]!["Value"]!.Value<string>().ShouldBe(SampleRefreshToken);
		after["PrivateKey"]!["Value"]!.Value<string>().ShouldBe(SamplePrivateKey);
		after["StoreAuthenticationCookie"]!.Type.ShouldBe(JTokenType.String);
		after["StoreAuthenticationCookie"]!.Value<string>().ShouldBe(SampleStoreAuthCookie);

		var reloaded = Load(after);
		reloaded.ExistingAccessToken.TokenValue.ShouldBe("Atna|_NEW_ENC_");
		reloaded.RefreshToken.ShouldNotBeNull().Value.ShouldBe(SampleRefreshToken);
		reloaded.StoreAuthenticationCookie.ShouldBe(SampleStoreAuthCookie);
	}

	[TestMethod]
	public void full_update_marks_all_secrets_dirty_and_applies_write_method()
	{
		var loaded = Load(FullyEncryptedObject());
		IdentityTokenStorage.Configure(TokenStorageMethod.Plaintext, IdentityTokenStorage.Protector);

		loaded.Update(
			new PrivateKey(SamplePrivateKey),
			new AdpToken(SampleAdpToken),
			new AccessToken(SampleAccessToken, SampleExpires),
			new RefreshToken(SampleRefreshToken),
			new List<KeyValuePair<string, string?>> { new(SampleCookieName, SampleCookieValue) },
			storeAuthenticationCookie: SampleStoreAuthCookie);

		var after = Serialize(loaded);
		after.SelectTokens("$..IsEncrypted").ShouldBeEmpty();
		AssertAllSecretsUsable(Load(after));
	}
}

[TestClass]
[DoNotParallelize]
public class FailureSafety
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void malformed_IsEncrypted_string_fails()
	{
		var json = """
			{
			  "LocaleName": "us",
			  "ExistingAccessToken": { "TokenValue": "Atna|x", "Expires": "2200-01-01T00:00:00Z" },
			  "RefreshToken": { "Value": "Atnr|x", "IsEncrypted": "yes" }
			}
			""";
		Should.Throw<JsonReaderException>(() => Identity.FromJson(json));
	}

	[TestMethod]
	public void malformed_IsEncrypted_number_fails()
	{
		var jo = JObject.Parse(LegacyJson());
		jo["RefreshToken"]!["IsEncrypted"] = 1;
		Should.Throw<JsonReaderException>(() => Load(jo));
	}

	[TestMethod]
	public void encrypted_object_missing_Value_fails()
	{
		ConfigureEncrypted();
		var jo = JObject.Parse(LegacyJson());
		jo["RefreshToken"] = new JObject { ["IsEncrypted"] = true };
		Should.Throw<JsonReaderException>(() => Load(jo));
	}

	[TestMethod]
	public void invalid_ciphertext_fails_without_exposing_secrets()
	{
		ConfigureEncrypted();
		var jo = JObject.Parse(LegacyJson());
		jo["ExistingAccessToken"] = new JObject
		{
			["TokenValue"] = "v1.not-valid-ciphertext",
			["Expires"] = SampleExpires,
			["IsEncrypted"] = true
		};

		var ex = Should.Throw<IdentityTokenDecryptException>(() => Load(jo));
		ex.FieldName.ShouldBe("ExistingAccessToken");
		ex.Message.ShouldContain("Failed to decrypt");
		ex.ToString().ShouldNotContain(SampleAccessToken);
		ex.ToString().ShouldNotContain(SampleRefreshToken);
	}

	[TestMethod]
	public void tampered_ciphertext_fails_authentication()
	{
		var jo = FullyEncryptedObject();
		var value = jo["RefreshToken"]!["Value"]!.Value<string>()!;
		var parts = value.Split('.');
		parts[2] = TamperBase64Url(parts[2]);
		jo["RefreshToken"]!["Value"] = string.Join('.', parts);

		var ex = Should.Throw<IdentityTokenDecryptException>(() => Load(jo));
		ex.FieldName.ShouldBe("RefreshToken");
		ex.ToString().ShouldNotContain(SampleRefreshToken);
	}

	[TestMethod]
	public void wrong_aad_field_binding_fails_decrypt()
	{
		ConfigureEncrypted();
		var jo = JObject.Parse(LegacyJson());
		// Encrypt with RefreshToken AAD but place under AdpToken.
		jo["AdpToken"] = new JObject
		{
			["Value"] = Protect(SampleAdpToken, "RefreshToken"),
			["IsEncrypted"] = true
		};

		var ex = Should.Throw<IdentityTokenDecryptException>(() => Load(jo));
		ex.FieldName.ShouldBe("AdpToken");
	}

	[TestMethod]
	public void decryption_failure_does_not_overwrite_source_file()
	{
		ConfigureEncrypted();
		var path = Path.Combine(Path.GetTempPath(), $"identity-enc-fail-{Guid.NewGuid():N}.json");
		try
		{
			using (new IdentityPersister(CreateRegisteredIdentity(), path)) { }

			var jo = JObject.Parse(File.ReadAllText(path));
			jo["RefreshToken"]!["Value"] = "v1.AAAA.BBBB.CCCC";
			File.WriteAllText(path, jo.ToString(Formatting.Indented));
			var tampered = File.ReadAllText(path);

			Should.Throw<IdentityTokenDecryptException>(() => new IdentityPersister(path));
			File.ReadAllText(path).ShouldBe(tampered);
		}
		finally
		{
			if (File.Exists(path))
				File.Delete(path);
		}
	}

	[TestMethod]
	public void encrypted_write_without_protector_fails_closed()
	{
		IdentityTokenStorage.Configure(TokenStorageMethod.Encrypted, protector: null);
		var ex = Should.Throw<IdentityTokenEncryptException>(() => Serialize(CreateRegisteredIdentity()));
		ex.FieldName.ShouldNotBeNullOrWhiteSpace();
	}

	[TestMethod]
	public void encrypted_read_without_protector_fails_closed()
	{
		var jo = FullyEncryptedObject();
		IdentityTokenStorage.Reset();
		var ex = Should.Throw<IdentityTokenDecryptException>(() => Load(jo));
		ex.FieldName.ShouldNotBeNullOrWhiteSpace();
	}
}
