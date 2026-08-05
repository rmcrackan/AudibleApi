using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dinah.Core;
using Dinah.Core.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization;

/// <summary>
/// Converts identity secrets between storage methods with all-or-nothing file persistence.
/// </summary>
public static class IdentityTokenConversion
{
	/// <summary>
	/// Compare an identity's persisted encryption metadata to <paramref name="method"/>.
	/// Does not require decryption when metadata is present.
	/// </summary>
	public static TokenStorageAlignment GetAlignment(Identity identity, TokenStorageMethod method)
	{
		ArgumentNullException.ThrowIfNull(identity);

		try
		{
			var wantEncrypted = method == TokenStorageMethod.Encrypted;
			var states = CollectFieldStates(identity).ToList();
			if (states.Count == 0)
				return TokenStorageAlignment.NoApplicableTokens;

			return states.All(encrypted => encrypted == wantEncrypted)
				? TokenStorageAlignment.AllMatch
				: TokenStorageAlignment.SomeMismatch;
		}
		catch
		{
			return TokenStorageAlignment.Indeterminate;
		}
	}

	/// <summary>
	/// Mark all applicable secrets dirty so the next serialize uses the process write method.
	/// Caller should typically use <see cref="IdentityTokenStorage.RunWithWriteMethod"/>.
	/// </summary>
	public static void ConvertInMemory(Identity identity)
	{
		ArgumentNullException.ThrowIfNull(identity);
		EnsureEncryptedFieldsReadable(identity);
		identity.SecretPersistence.MarkAllSecretsDirty();
	}

	/// <summary>
	/// Convert a file whose root JSON object is a single <see cref="Identity"/>, or convert the Identity at <paramref name="jsonPath"/>.
	/// Creates a verified backup, writes atomically, and reloads to verify. On failure the original file is left intact.
	/// </summary>
	public static IdentityTokenConversionResult ConvertAndPersist(string path, TokenStorageMethod targetMethod, string? jsonPath = null)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(path, nameof(path));
		if (!File.Exists(path))
			throw new FileNotFoundException("Identity file not found.", path);

		var originalText = File.ReadAllText(path);
		TokenStorageAlignment alignmentBefore = TokenStorageAlignment.Indeterminate;

		try
		{
			if (string.IsNullOrWhiteSpace(jsonPath))
			{
				var identity = Identity.FromJson(originalText);
				alignmentBefore = GetAlignment(identity, targetMethod);
				if (alignmentBefore == TokenStorageAlignment.AllMatch)
					return IdentityTokenConversionResult.NoChanges(alignmentBefore);
				if (alignmentBefore == TokenStorageAlignment.NoApplicableTokens)
					return IdentityTokenConversionResult.NoChanges(alignmentBefore);

				var convertedJson = SerializeConverted(identity, targetMethod);
				ValidateIdentityJson(convertedJson);
				return PersistReplacement(path, originalText, convertedJson, alignmentBefore);
			}

			var root = JObject.Parse(originalText);
			var token = root.SelectToken(jsonPath)
				?? throw new JsonSerializationException("No match found at JSONPath.");
			if (token is not JObject identityObj)
				throw new JsonSerializationException("JSONPath did not target an Identity object.");

			var pathIdentity = Identity.FromJson(identityObj.ToString(Formatting.None));
			alignmentBefore = GetAlignment(pathIdentity, targetMethod);
			if (alignmentBefore is TokenStorageAlignment.AllMatch or TokenStorageAlignment.NoApplicableTokens)
				return IdentityTokenConversionResult.NoChanges(alignmentBefore);

			var convertedIdentityJson = SerializeConverted(pathIdentity, targetMethod);
			ValidateIdentityJson(convertedIdentityJson);
			token.Replace(JObject.Parse(convertedIdentityJson));

			var newFileJson = root.ToString(Formatting.Indented);
			ValidateContainsIdentityAtPath(newFileJson, jsonPath);
			return PersistReplacement(path, originalText, newFileJson, alignmentBefore);
		}
		catch (Exception ex) when (ex is not FileNotFoundException)
		{
			EnsureUnchanged(path, originalText);
			return IdentityTokenConversionResult.Failure(
				alignmentBefore,
				SafeError(ex),
				failedCategories: GuessFailedCategory(ex));
		}
	}

	/// <summary>
	/// Convert every Identity-like object in a JSON file (for example multiple accounts in one settings file).
	/// All-or-nothing: any conversion failure leaves the original file unchanged.
	/// </summary>
	public static IdentityTokenConversionResult ConvertAllIdentitiesInFile(string path, TokenStorageMethod targetMethod)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(path, nameof(path));
		if (!File.Exists(path))
			throw new FileNotFoundException("JSON file not found.", path);

		var originalText = File.ReadAllText(path);
		var alignmentBefore = TokenStorageAlignment.Indeterminate;

		try
		{
			var root = JObject.Parse(originalText);
			var identityObjects = FindIdentityObjects(root).ToList();
			if (identityObjects.Count == 0)
				return IdentityTokenConversionResult.NoChanges(TokenStorageAlignment.NoApplicableTokens);

			var alignments = new List<TokenStorageAlignment>();
			foreach (var identityObj in identityObjects)
			{
				var identity = Identity.FromJson(identityObj.ToString(Formatting.None));
				var alignment = GetAlignment(identity, targetMethod);
				alignments.Add(alignment);
				if (alignment == TokenStorageAlignment.Indeterminate)
					return IdentityTokenConversionResult.Failure(alignment, "Token storage alignment could not be determined.");

				if (alignment == TokenStorageAlignment.AllMatch || alignment == TokenStorageAlignment.NoApplicableTokens)
					continue;

				var convertedJson = SerializeConverted(identity, targetMethod);
				ValidateIdentityJson(convertedJson);
				identityObj.Replace(JObject.Parse(convertedJson));
			}

			alignmentBefore = CombineAlignments(alignments);
			if (alignmentBefore is TokenStorageAlignment.AllMatch or TokenStorageAlignment.NoApplicableTokens)
				return IdentityTokenConversionResult.NoChanges(alignmentBefore);

			var newFileJson = root.ToString(Formatting.Indented);
			ValidateAllIdentitiesReadable(newFileJson);
			return PersistReplacement(path, originalText, newFileJson, alignmentBefore);
		}
		catch (Exception ex) when (ex is not FileNotFoundException)
		{
			EnsureUnchanged(path, originalText);
			return IdentityTokenConversionResult.Failure(
				alignmentBefore,
				SafeError(ex),
				failedCategories: GuessFailedCategory(ex));
		}
	}

	private static string SerializeConverted(Identity identity, TokenStorageMethod targetMethod)
	{
		string? json = null;
		IdentityTokenStorage.RunWithWriteMethod(targetMethod, () =>
		{
			ConvertInMemory(identity);
			json = JsonConvert.SerializeObject(identity, Formatting.Indented, Identity.GetJsonSerializerSettings());
		});
		var converted = json ?? throw new InvalidOperationException("Identity conversion produced no JSON.");

		// Normal persistence may fall back to plaintext when Protect fails; explicit Encrypted
		// conversion must not report success unless ciphertext was actually written.
		if (targetMethod == TokenStorageMethod.Encrypted
			&& GetAlignment(identity, TokenStorageMethod.Encrypted) != TokenStorageAlignment.AllMatch)
		{
			throw new InvalidOperationException(
				"Encrypted conversion requires a working protector; identity tokens were not encrypted.");
		}

		return converted;
	}

	private static IdentityTokenConversionResult PersistReplacement(string path, string originalText, string newJson, TokenStorageAlignment alignmentBefore)
	{
		// Validate before touching the original.
		ValidateJsonReadable(newJson);

		string? backupPath = null;
		try
		{
			backupPath = AtomicFileWriter.CreateBackup(path);
			AtomicFileWriter.WriteAllText(path, newJson, validateTempFile: temp =>
			{
				var tempText = File.ReadAllText(temp);
				ValidateJsonReadable(tempText);
				ValidateAllIdentitiesReadable(tempText);
			});

			// Reload verify from the replaced file.
			ValidateAllIdentitiesReadable(File.ReadAllText(path));
			return IdentityTokenConversionResult.Success(alignmentBefore, backupPath);
		}
		catch (Exception ex)
		{
			EnsureUnchanged(path, originalText);
			return IdentityTokenConversionResult.Failure(alignmentBefore, SafeError(ex), GuessFailedCategory(ex));
		}
	}

	private static void EnsureEncryptedFieldsReadable(Identity identity)
	{
		var persistence = identity.SecretPersistence;
		var needsProtector =
			persistence.AccessToken.LoadedEncrypted
			|| persistence.RefreshToken.LoadedEncrypted
			|| persistence.AdpToken.LoadedEncrypted
			|| persistence.PrivateKey.LoadedEncrypted
			|| persistence.StoreAuthenticationCookie.LoadedEncrypted
			|| persistence.CookieLoadedEncrypted.Any(x => x);

		if (needsProtector && IdentityTokenStorage.Protector is null)
			throw new InvalidOperationException("Encrypted tokens are present but no protector is configured.");
	}

	private static IEnumerable<bool> CollectFieldStates(Identity identity)
	{
		var persistence = identity.SecretPersistence;

		// Access token is always present on Identity.
		yield return persistence.AccessToken.LoadedEncrypted;

		if (identity.RefreshToken is not null)
			yield return persistence.RefreshToken.LoadedEncrypted;
		if (identity.AdpToken is not null)
			yield return persistence.AdpToken.LoadedEncrypted;
		if (identity.PrivateKey is not null)
			yield return persistence.PrivateKey.LoadedEncrypted;
		if (!string.IsNullOrEmpty(identity.StoreAuthenticationCookie))
			yield return persistence.StoreAuthenticationCookie.LoadedEncrypted;

		var cookies = identity.Cookies.ToList();
		for (var i = 0; i < cookies.Count; i++)
		{
			if (cookies[i].Value is null)
				continue;
			var encrypted = i < persistence.CookieLoadedEncrypted.Count && persistence.CookieLoadedEncrypted[i];
			yield return encrypted;
		}
	}

	private static IEnumerable<JObject> FindIdentityObjects(JToken root)
	{
		if (root is JObject rootObj && IsIdentityObject(rootObj))
			yield return rootObj;

		if (root is not JContainer container)
			yield break;

		foreach (var obj in container.Descendants().OfType<JObject>())
		{
			if (IsIdentityObject(obj))
				yield return obj;
		}
	}

	private static bool IsIdentityObject(JObject obj)
		=> obj["LocaleName"] is not null
			&& obj["ExistingAccessToken"] is JObject;

	private static void ValidateIdentityJson(string json)
	{
		var identity = Identity.FromJson(json);
		_ = identity.ExistingAccessToken.TokenValue;
	}

	private static void ValidateContainsIdentityAtPath(string fileJson, string jsonPath)
	{
		var root = JObject.Parse(fileJson);
		var token = root.SelectToken(jsonPath) as JObject
			?? throw new JsonSerializationException("Converted JSONPath identity missing after write validation.");
		ValidateIdentityJson(token.ToString(Formatting.None));
	}

	private static void ValidateAllIdentitiesReadable(string json)
	{
		var token = JToken.Parse(json);
		if (token is JObject root && IsIdentityObject(root))
		{
			ValidateIdentityJson(json);
			return;
		}

		foreach (var identityObj in FindIdentityObjects(token))
			ValidateIdentityJson(identityObj.ToString(Formatting.None));
	}

	private static void ValidateJsonReadable(string json)
		=> _ = JToken.Parse(json);

	private static void EnsureUnchanged(string path, string originalText)
	{
		try
		{
			if (File.Exists(path) && File.ReadAllText(path) != originalText)
				File.WriteAllText(path, originalText);
		}
		catch
		{
			// Best effort restore; callers still receive the failure result.
		}
	}

	private static TokenStorageAlignment CombineAlignments(IReadOnlyList<TokenStorageAlignment> alignments)
	{
		if (alignments.Count == 0)
			return TokenStorageAlignment.NoApplicableTokens;
		if (alignments.Any(a => a == TokenStorageAlignment.Indeterminate))
			return TokenStorageAlignment.Indeterminate;
		if (alignments.All(a => a is TokenStorageAlignment.AllMatch or TokenStorageAlignment.NoApplicableTokens))
		{
			return alignments.Any(a => a == TokenStorageAlignment.AllMatch)
				? TokenStorageAlignment.AllMatch
				: TokenStorageAlignment.NoApplicableTokens;
		}
		return TokenStorageAlignment.SomeMismatch;
	}

	private static string SafeError(Exception ex)
	{
		// Do not include exception ToString() payloads that might echo JSON secrets.
		var message = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
		if (ex.InnerException is not null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
			return $"{message} ({ex.InnerException.GetType().Name})";
		return message;
	}

	private static string[] GuessFailedCategory(Exception ex)
	{
		var text = ex.Message;
		foreach (var category in new[]
		{
			"ExistingAccessToken", "RefreshToken", "AdpToken", "PrivateKey",
			"StoreAuthenticationCookie", "Cookies"
		})
		{
			if (text.Contains(category, StringComparison.Ordinal))
				return [category];
		}
		return ["Identity"];
	}
}
