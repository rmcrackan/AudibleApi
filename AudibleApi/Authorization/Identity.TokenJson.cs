using AudibleApi.Cryptography;
using Dinah.Core.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace AudibleApi.Authorization;

public partial class Identity
{
	/// <summary>
	/// Serializes identity secrets with optional <c>IsEncrypted</c> metadata.
	/// Missing / false = plaintext; true = encrypted. Encryption state is never inferred from value contents.
	/// </summary>
	internal sealed class IdentityJsonConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
			=> objectType == typeof(Identity);

		public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
		{
			if (reader.TokenType == JsonToken.Null)
				return null;

			var jo = JObject.Load(reader);
			return ReadIdentity(jo);
		}

		public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
		{
			if (value is not Identity identity)
				throw new JsonSerializationException("IdentityJsonConverter can only write Identity values.");

			WriteIdentity(identity).WriteTo(writer);
		}

		internal static Identity ReadIdentity(JObject jo)
		{
			var localeName = jo["LocaleName"]?.Value<string>()?.Trim() ?? string.Empty;
			var locale = string.IsNullOrWhiteSpace(localeName) ? Locale.Empty : Localization.Get(localeName);
			var identity = new Identity(locale);

			var accessTokenToken = jo["ExistingAccessToken"] as JObject
				?? throw new JsonReaderException("ExistingAccessToken not found on Identity");
			var accessEncrypted = ReadIsEncrypted(accessTokenToken, "ExistingAccessToken");
			var accessRaw = accessTokenToken["TokenValue"]?.Value<string>()
				?? throw new JsonReaderException("TokenValue not found on AccessToken");
			var accessPlaintext = accessEncrypted
				? Decrypt(accessRaw, localeName, "ExistingAccessToken")
				: accessRaw;
			var expires = accessTokenToken["Expires"]?.Value<DateTime>()
				?? throw new JsonReaderException("Expires not found on AccessToken");
			identity.ExistingAccessToken = new AccessToken(accessPlaintext, expires);
			identity.SecretPersistence.AccessToken.LoadedEncrypted = accessEncrypted;
			identity.SecretPersistence.AccessToken.Dirty = false;

			identity.PrivateKey = ReadOptionalStrongType(jo["PrivateKey"], localeName, "PrivateKey", v => new PrivateKey(v), identity.SecretPersistence.PrivateKey);
			identity.AdpToken = ReadOptionalStrongType(jo["AdpToken"], localeName, "AdpToken", v => new AdpToken(v), identity.SecretPersistence.AdpToken);
			identity.RefreshToken = ReadOptionalStrongType(jo["RefreshToken"], localeName, "RefreshToken", v => new RefreshToken(v), identity.SecretPersistence.RefreshToken);

			identity.DeviceSerialNumber = jo["DeviceSerialNumber"]?.Value<string>();
			identity.DeviceType = jo["DeviceType"]?.Value<string>();
			identity.AmazonAccountId = jo["AmazonAccountId"]?.Value<string>();
			identity.DeviceName = jo["DeviceName"]?.Value<string>();

			var (storeCookie, storeEncrypted) = ReadStringSecret(jo["StoreAuthenticationCookie"], localeName, "StoreAuthenticationCookie");
			identity.StoreAuthenticationCookie = storeCookie;
			identity.SecretPersistence.StoreAuthenticationCookie.LoadedEncrypted = storeEncrypted;
			identity.SecretPersistence.StoreAuthenticationCookie.Dirty = false;

			identity._cookies = ReadCookies(jo["Cookies"], localeName, identity.SecretPersistence);
			identity.SecretPersistence.CookiesDirty = false;

			identity.IsValid = !string.IsNullOrWhiteSpace(localeName)
				&& identity.PrivateKey is not null
				&& identity.AdpToken is not null
				&& identity.RefreshToken is not null;

			return identity;
		}

		internal static JObject WriteIdentity(Identity identity)
		{
			var locale = identity.LocaleName;
			var jo = new JObject
			{
				["LocaleName"] = locale
			};

			jo["ExistingAccessToken"] = WriteAccessToken(identity.ExistingAccessToken, identity.SecretPersistence.AccessToken, locale);

			WriteStrongType(jo, "PrivateKey", identity.PrivateKey?.Reveal(), identity.SecretPersistence.PrivateKey, locale);
			WriteStrongType(jo, "AdpToken", identity.AdpToken?.Reveal(), identity.SecretPersistence.AdpToken, locale);
			WriteStrongType(jo, "RefreshToken", identity.RefreshToken?.Reveal(), identity.SecretPersistence.RefreshToken, locale);

			jo["DeviceSerialNumber"] = identity.DeviceSerialNumber;
			jo["DeviceType"] = identity.DeviceType;
			jo["AmazonAccountId"] = identity.AmazonAccountId;
			jo["DeviceName"] = identity.DeviceName;

			jo["StoreAuthenticationCookie"] = WriteStringSecret(
				identity.StoreAuthenticationCookie.Reveal(),
				identity.SecretPersistence.StoreAuthenticationCookie,
				locale,
				"StoreAuthenticationCookie");

			jo["Cookies"] = WriteCookies(identity, locale);

			identity.SecretPersistence.ClearDirtyAfterWrite();
			return jo;
		}

		private static JObject WriteAccessToken(AccessToken accessToken, IdentitySecretPersistence.FieldState state, string locale)
		{
			var encrypt = state.ShouldEncrypt();
			var tokenValue = accessToken.Reveal();
			if (encrypt)
				encrypt = TryProtect(tokenValue, locale, "ExistingAccessToken", out tokenValue);

			var jo = new JObject
			{
				["TokenValue"] = tokenValue,
				["Expires"] = accessToken.Expires
			};
			if (encrypt)
				jo["IsEncrypted"] = true;

			state.SetWritten(encrypt);
			return jo;
		}

		private static T? ReadOptionalStrongType<T>(JToken? token, string locale, string fieldName, Func<string, T> factory, IdentitySecretPersistence.FieldState state)
			where T : class
		{
			if (token is null || token.Type == JTokenType.Null)
			{
				state.LoadedEncrypted = false;
				state.Dirty = false;
				return null;
			}

			var (raw, encrypted) = ReadObjectSecret(token, fieldName);
			state.LoadedEncrypted = encrypted;
			state.Dirty = false;

			var plaintext = encrypted ? Decrypt(raw, locale, fieldName) : raw;
			return factory(plaintext);
		}

		private static void WriteStrongType(JObject parent, string fieldName, string? plaintext, IdentitySecretPersistence.FieldState state, string locale)
		{
			if (plaintext is null)
			{
				parent[fieldName] = null;
				state.SetWritten(false);
				return;
			}

			var encrypt = state.ShouldEncrypt();
			var value = plaintext;
			if (encrypt)
				encrypt = TryProtect(plaintext, locale, fieldName, out value);

			var obj = new JObject { ["Value"] = value };
			if (encrypt)
				obj["IsEncrypted"] = true;
			parent[fieldName] = obj;
			state.SetWritten(encrypt);
		}

		private static (string? value, bool encrypted) ReadStringSecret(JToken? token, string locale, string fieldName)
		{
			if (token is null || token.Type == JTokenType.Null)
				return (null, false);

			if (token.Type == JTokenType.String)
				return (token.Value<string>(), false);

			var (raw, encrypted) = ReadObjectSecret(token, fieldName);
			if (!encrypted)
				return (raw, false);

			return (Decrypt(raw, locale, fieldName), true);
		}

		private static JToken WriteStringSecret(string? plaintext, IdentitySecretPersistence.FieldState state, string locale, string fieldName)
		{
			if (plaintext is null)
			{
				state.SetWritten(false);
				return JValue.CreateNull();
			}

			var encrypt = state.ShouldEncrypt();
			if (!encrypt)
			{
				state.SetWritten(false);
				return new JValue(plaintext);
			}

			if (!TryProtect(plaintext, locale, fieldName, out var protectedValue))
			{
				state.SetWritten(false);
				return new JValue(plaintext);
			}

			state.SetWritten(true);
			return new JObject
			{
				["Value"] = protectedValue,
				["IsEncrypted"] = true
			};
		}

		private static List<KeyValuePair<string, SecretString>> ReadCookies(JToken? token, string locale, IdentitySecretPersistence persistence)
		{
			persistence.CookieLoadedEncrypted.Clear();
			var list = new List<KeyValuePair<string, SecretString>>();
			if (token is not JArray arr)
				return list;

			foreach (var item in arr)
			{
				if (item is not JObject cookieObj)
					throw new JsonReaderException("Cookie entry must be an object.");

				var key = cookieObj["Key"]?.Value<string>()
					?? throw new JsonReaderException("Cookie Key is required.");
				var aadField = CookieAadField(key);
				var valueToken = cookieObj["Value"];
				if (valueToken is null || valueToken.Type == JTokenType.Null)
				{
					list.Add(new KeyValuePair<string, SecretString>(key, null));
					persistence.CookieLoadedEncrypted.Add(false);
					continue;
				}

				if (valueToken.Type == JTokenType.String)
				{
					list.Add(new KeyValuePair<string, SecretString>(key, valueToken.Value<string>()));
					persistence.CookieLoadedEncrypted.Add(false);
					continue;
				}

				var (raw, encrypted) = ReadObjectSecret(valueToken, aadField);
				var plaintext = encrypted ? Decrypt(raw, locale, aadField) : raw;
				list.Add(new KeyValuePair<string, SecretString>(key, plaintext));
				persistence.CookieLoadedEncrypted.Add(encrypted);
			}

			return list;
		}

		private static JArray WriteCookies(Identity identity, string locale)
		{
			var arr = new JArray();
			var persistence = identity.SecretPersistence;
			var cookies = identity._cookies ?? [];
			var newLoaded = new List<bool>();

			for (var i = 0; i < cookies.Count; i++)
			{
				var cookie = cookies[i];
				var loadedEncrypted = i < persistence.CookieLoadedEncrypted.Count && persistence.CookieLoadedEncrypted[i];
				var encrypt = IdentityTokenStorage.ShouldEncrypt(persistence.CookiesDirty, loadedEncrypted);
				var aadField = CookieAadField(cookie.Key);

				JToken valueToken;
				var cookieValue = cookie.Value.Reveal();
				if (cookieValue is null)
				{
					valueToken = JValue.CreateNull();
					encrypt = false;
				}
				else if (encrypt && TryProtect(cookieValue, locale, aadField, out var protectedValue))
				{
					valueToken = new JObject
					{
						["Value"] = protectedValue,
						["IsEncrypted"] = true
					};
				}
				else
				{
					encrypt = false;
					// the revealed string, not the SecretString: JValue takes object, so passing the secret
					// itself would compile and quietly persist "[REDACTED ...]" in place of the cookie
					valueToken = new JValue(cookieValue);
				}

				arr.Add(new JObject
				{
					["Key"] = cookie.Key,
					["Value"] = valueToken
				});
				newLoaded.Add(encrypt);
			}

			persistence.CookieLoadedEncrypted.Clear();
			persistence.CookieLoadedEncrypted.AddRange(newLoaded);
			return arr;
		}

		private static (string value, bool encrypted) ReadObjectSecret(JToken token, string fieldName)
		{
			if (token is not JObject jo)
				throw new JsonReaderException($"{fieldName} must be a JSON object or string.");

			var encrypted = ReadIsEncrypted(jo, fieldName);
			var raw = jo["Value"]?.Value<string>()
				?? throw new JsonReaderException($"Value not found on {fieldName}");
			return (raw, encrypted);
		}

		private static bool ReadIsEncrypted(JObject jo, string fieldName)
		{
			var token = jo["IsEncrypted"];
			if (token is null || token.Type == JTokenType.Null)
				return false;
			if (token.Type != JTokenType.Boolean)
				throw new JsonReaderException($"IsEncrypted for {fieldName} must be a boolean.");
			return token.Value<bool>();
		}

		/// <summary>
		/// Try to encrypt. On failure, log an error and return plaintext so persistence
		/// does not break the host (encryption is preferred but not required).
		/// </summary>
		private static bool TryProtect(string plaintext, string locale, string fieldName, out string value)
		{
			try
			{
				value = IdentityTokenStorage.Protect(plaintext, Aad(locale, fieldName));
				return true;
			}
			catch (Exception ex) when (ex is SecretProtectionException or OsSecretStoreUnavailableException or InvalidOperationException)
			{
				Serilog.Log.Error(
					new IdentityTokenEncryptException(fieldName, ex),
					"Failed to encrypt identity field {FieldName} (locale {Locale}). Saving as plaintext so the app can continue. " +
					"Encryption will be retried on the next write when a protector is available.",
					fieldName,
					locale);
				value = plaintext;
				return false;
			}
		}

		private static string Decrypt(string payload, string locale, string fieldName)
		{
			try
			{
				return IdentityTokenStorage.Unprotect(payload, Aad(locale, fieldName));
			}
			catch (Exception ex) when (ex is SecretProtectionException or OsSecretStoreUnavailableException or InvalidOperationException)
			{
				throw new IdentityTokenDecryptException(fieldName, ex);
			}
		}

		private static string CookieAadField(string cookieKey) => $"Cookies:{cookieKey}";

		private static string Aad(string locale, string fieldName)
			=> $"AudibleApi.Identity|{locale}|{fieldName}";
	}
}
