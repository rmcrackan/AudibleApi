using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace AudibleApi.Authorization;

public partial class Identity
{
	[JsonConstructor]
	protected Identity(string localeName, AccessToken existingAccessToken, PrivateKey privateKey, AdpToken adpToken, RefreshToken refreshToken, List<KeyValuePair<string, string?>> cookies)
	{
		LocaleName = localeName?.Trim() ?? string.Empty;
		IsValid = !string.IsNullOrWhiteSpace(localeName);
		ExistingAccessToken = ArgumentValidator.EnsureNotNull(existingAccessToken, nameof(existingAccessToken));

		if (privateKey is null)
			IsValid = false;
		else
			PrivateKey = new PrivateKey(privateKey);

		if (adpToken is null)
			IsValid = false;
		else
			AdpToken = new AdpToken(adpToken);

		if (refreshToken is null)
			IsValid = false;
		else
			RefreshToken = new RefreshToken(refreshToken);

		_cookies = cookies;
	}

	public static Identity FromJson(string json, string? jsonPath = null)
		=> Dinah.Core.JsonHelper.FromJson<Identity>(json, jsonPath, GetJsonSerializerSettings());

	public static JsonSerializerSettings GetJsonSerializerSettings()
	{
		var settings = new JsonSerializerSettings();
		settings.Converters.Add(new IdentityJsonConverter());
		return settings;
	}
}
