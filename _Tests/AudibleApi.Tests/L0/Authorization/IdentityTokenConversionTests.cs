using AudibleApi.Cryptography;
using static Authoriz.TokenPersistenceFixtures.Fixtures;
using static Authoriz.IdentityTokenConversionTests.ConversionTestPaths;

namespace Authoriz.IdentityTokenConversionTests;

[TestClass]
[DoNotParallelize]
public class ConvertInMemoryAndAlignment
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void alignment_legacy_plaintext_mismatches_encrypted_preference()
	{
		var identity = Identity.FromJson(LegacyJson());
		IdentityTokenConversion.GetAlignment(identity, TokenStorageMethod.Encrypted)
			.ShouldBe(TokenStorageAlignment.SomeMismatch);
		IdentityTokenConversion.GetAlignment(identity, TokenStorageMethod.Plaintext)
			.ShouldBe(TokenStorageAlignment.AllMatch);
	}

	[TestMethod]
	public void alignment_fully_encrypted_matches_encrypted_preference()
	{
		var identity = Load(FullyEncryptedObject());
		IdentityTokenConversion.GetAlignment(identity, TokenStorageMethod.Encrypted)
			.ShouldBe(TokenStorageAlignment.AllMatch);
		IdentityTokenConversion.GetAlignment(identity, TokenStorageMethod.Plaintext)
			.ShouldBe(TokenStorageAlignment.SomeMismatch);
	}

	[TestMethod]
	public void alignment_hybrid_is_mismatch_for_both_preferences()
	{
		var jo = FullyEncryptedObject();
		jo["RefreshToken"] = new JObject { ["Value"] = SampleRefreshToken };
		var identity = Load(jo);

		IdentityTokenConversion.GetAlignment(identity, TokenStorageMethod.Encrypted)
			.ShouldBe(TokenStorageAlignment.SomeMismatch);
		IdentityTokenConversion.GetAlignment(identity, TokenStorageMethod.Plaintext)
			.ShouldBe(TokenStorageAlignment.SomeMismatch);
	}
}

[TestClass]
[DoNotParallelize]
public class ConvertAndPersistSingleIdentity
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void plaintext_file_to_encrypted_creates_backup_and_converts()
	{
		ConfigureEncrypted();
		var path = TempPath("to-enc");
		try
		{
			File.WriteAllText(path, LegacyJson());
			var before = File.ReadAllText(path);

			var result = IdentityTokenConversion.ConvertAndPersist(path, TokenStorageMethod.Encrypted);

			result.Succeeded.ShouldBeTrue();
			result.Changed.ShouldBeTrue();
			result.BackupPath.ShouldNotBeNull();
			File.Exists(result.BackupPath!).ShouldBeTrue();
			File.ReadAllText(result.BackupPath!).ShouldBe(before);

			var onDisk = JObject.Parse(File.ReadAllText(path));
			AssertNoPlaintextSecretsInJson(onDisk);
			AssertAllSecretsUsable(Identity.FromJson(File.ReadAllText(path)));
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void encrypted_file_to_plaintext_converts_all_secrets()
	{
		ConfigureEncrypted();
		var path = TempPath("to-plain");
		try
		{
			File.WriteAllText(path, FullyEncryptedObject().ToString(Formatting.Indented));

			var result = IdentityTokenConversion.ConvertAndPersist(path, TokenStorageMethod.Plaintext);

			result.Succeeded.ShouldBeTrue();
			result.Changed.ShouldBeTrue();

			var onDisk = JObject.Parse(File.ReadAllText(path));
			onDisk.SelectTokens("$..IsEncrypted").Where(t => t.Type == JTokenType.Boolean && t.Value<bool>())
				.ShouldBeEmpty();
			onDisk["RefreshToken"]!["Value"]!.Value<string>().ShouldBe(SampleRefreshToken);
			AssertAllSecretsUsable(Identity.FromJson(File.ReadAllText(path)));
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void already_matching_is_idempotent_no_rewrite()
	{
		ConfigureEncrypted();
		var path = TempPath("idempotent");
		try
		{
			File.WriteAllText(path, FullyEncryptedObject().ToString(Formatting.Indented));
			var before = File.ReadAllText(path);

			var result = IdentityTokenConversion.ConvertAndPersist(path, TokenStorageMethod.Encrypted);

			result.Succeeded.ShouldBeTrue();
			result.Changed.ShouldBeFalse();
			result.BackupPath.ShouldBeNull();
			File.ReadAllText(path).ShouldBe(before);
			Directory.GetFiles(Path.GetDirectoryName(path)!, "*.bak").ShouldBeEmpty();
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void hybrid_to_encrypted_converts_remaining_plaintext()
	{
		ConfigureEncrypted();
		var path = TempPath("hybrid-enc");
		try
		{
			var jo = FullyEncryptedObject();
			jo["RefreshToken"] = new JObject { ["Value"] = SampleRefreshToken };
			File.WriteAllText(path, jo.ToString(Formatting.Indented));

			var result = IdentityTokenConversion.ConvertAndPersist(path, TokenStorageMethod.Encrypted);
			result.Succeeded.ShouldBeTrue();
			result.Changed.ShouldBeTrue();

			var after = JObject.Parse(File.ReadAllText(path));
			IsEncryptedFlag(after["RefreshToken"]).ShouldBeTrue();
			AssertNoPlaintextSecretsInJson(after);
			AssertAllSecretsUsable(Identity.FromJson(File.ReadAllText(path)));
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void failed_conversion_preserves_original_and_keeps_backup_if_created()
	{
		ConfigureEncrypted();
		var path = TempPath("fail-preserve");
		try
		{
			File.WriteAllText(path, LegacyJson());
			var before = File.ReadAllText(path);

			// Explicit Encrypted conversion must fail closed without a protector (unlike
			// ordinary persistence, which falls back to plaintext on encrypt failure).
			IdentityTokenStorage.Configure(TokenStorageMethod.Encrypted, protector: null);
			var result = IdentityTokenConversion.ConvertAndPersist(path, TokenStorageMethod.Encrypted);

			result.Succeeded.ShouldBeFalse();
			result.Changed.ShouldBeFalse();
			result.Error.ShouldNotBeNullOrWhiteSpace();
			result.Error!.ShouldNotContain(SampleRefreshToken);
			File.ReadAllText(path).ShouldBe(before);
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void decrypt_failure_during_convert_preserves_original()
	{
		ConfigureEncrypted();
		var path = TempPath("bad-cipher");
		try
		{
			var jo = FullyEncryptedObject();
			jo["RefreshToken"]!["Value"] = "v1.AAAA.BBBB.CCCC";
			File.WriteAllText(path, jo.ToString(Formatting.Indented));
			var before = File.ReadAllText(path);

			var result = IdentityTokenConversion.ConvertAndPersist(path, TokenStorageMethod.Plaintext);

			result.Succeeded.ShouldBeFalse();
			File.ReadAllText(path).ShouldBe(before);
			result.FailedCategories.ShouldContain("RefreshToken");
			result.Error.ShouldNotBeNull();
			result.Error.ShouldNotContain(SampleRefreshToken);
		}
		finally
		{
			CleanupPath(path);
		}
	}
}

[TestClass]
[DoNotParallelize]
public class ConvertAllIdentitiesInFileTests
{
	[TestInitialize]
	public void Init() => IdentityTokenStorage.Reset();

	[TestCleanup]
	public void Cleanup() => IdentityTokenStorage.Reset();

	[TestMethod]
	public void converts_multiple_identities_all_or_nothing()
	{
		ConfigureEncrypted();
		var path = TempPath("multi");
		try
		{
			var file = new JObject
			{
				["Accounts"] = new JArray
				{
					new JObject
					{
						["AccountId"] = "a@example.com",
						["IdentityTokens"] = JObject.Parse(LegacyJson())
					},
					new JObject
					{
						["AccountId"] = "b@example.com",
						["IdentityTokens"] = JObject.Parse(LegacyJson())
					}
				},
				["Cdm"] = null
			};
			File.WriteAllText(path, file.ToString(Formatting.Indented));

			var result = IdentityTokenConversion.ConvertAllIdentitiesInFile(path, TokenStorageMethod.Encrypted);
			result.Succeeded.ShouldBeTrue();
			result.Changed.ShouldBeTrue();
			result.BackupPath.ShouldNotBeNull();

			var after = JObject.Parse(File.ReadAllText(path));
			after["Accounts"]!.Count().ShouldBe(2);
			foreach (var account in after["Accounts"]!)
			{
				var tokens = (JObject)account["IdentityTokens"]!;
				IsEncryptedFlag(tokens["RefreshToken"]).ShouldBeTrue();
				AssertAllSecretsUsable(Identity.FromJson(tokens.ToString(Formatting.None)));
			}
			after["Accounts"]![0]!["AccountId"]!.Value<string>().ShouldBe("a@example.com");
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void multi_identity_failure_preserves_original_file()
	{
		ConfigureEncrypted();
		var path = TempPath("multi-fail");
		try
		{
			var good = FullyEncryptedObject();
			var bad = FullyEncryptedObject();
			bad["RefreshToken"]!["Value"] = "v1.AAAA.BBBB.CCCC";

			var file = new JObject
			{
				["Accounts"] = new JArray
				{
					new JObject { ["AccountId"] = "good", ["IdentityTokens"] = good },
					new JObject { ["AccountId"] = "bad", ["IdentityTokens"] = bad }
				}
			};
			File.WriteAllText(path, file.ToString(Formatting.Indented));
			var before = File.ReadAllText(path);

			var result = IdentityTokenConversion.ConvertAllIdentitiesInFile(path, TokenStorageMethod.Plaintext);
			result.Succeeded.ShouldBeFalse();
			File.ReadAllText(path).ShouldBe(before);
		}
		finally
		{
			CleanupPath(path);
		}
	}

	[TestMethod]
	public void second_conversion_to_same_method_is_idempotent()
	{
		ConfigureEncrypted();
		var path = TempPath("multi-idem");
		try
		{
			var file = new JObject
			{
				["Accounts"] = new JArray
				{
					new JObject { ["IdentityTokens"] = JObject.Parse(LegacyJson()) }
				}
			};
			File.WriteAllText(path, file.ToString(Formatting.Indented));

			IdentityTokenConversion.ConvertAllIdentitiesInFile(path, TokenStorageMethod.Encrypted).Changed.ShouldBeTrue();
			var afterFirst = File.ReadAllText(path);

			var second = IdentityTokenConversion.ConvertAllIdentitiesInFile(path, TokenStorageMethod.Encrypted);
			second.Succeeded.ShouldBeTrue();
			second.Changed.ShouldBeFalse();
			File.ReadAllText(path).ShouldBe(afterFirst);
		}
		finally
		{
			CleanupPath(path);
		}
	}
}

file static class ConversionTestPaths
{
	public static string TempPath(string prefix)
		=> Path.Combine(Path.GetTempPath(), $"identity-conv-{prefix}-{Guid.NewGuid():N}.json");

	public static void CleanupPath(string path)
	{
		var dir = Path.GetDirectoryName(path);
		if (dir is null)
			return;
		foreach (var file in Directory.GetFiles(dir, Path.GetFileName(path) + "*"))
		{
			try { File.Delete(file); } catch { /* cleanup */ }
		}
		if (File.Exists(path))
		{
			try { File.Delete(path); } catch { /* cleanup */ }
		}
	}
}
