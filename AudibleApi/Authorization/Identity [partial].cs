using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AudibleApi.Authorization;

/// <summary>
/// In-memory handling of identity: Manages Audible API's state of authorization/authentication keys, tokens, and cookies. Maintains valid state
/// </summary>
public partial class Identity : IIdentity
{
	public static Identity Empty { get; } = new Identity(Locale.Empty);

	public event EventHandler? Updated;

	[JsonProperty]
	private string LocaleName { get; }
	[JsonIgnore]
	public Locale Locale => Localization.Get(LocaleName);

	[JsonIgnore]
	public bool IsValid { get; private set; }

	[JsonRequired]
	public AccessToken ExistingAccessToken
	{
		get => field ?? throw new RegistrationException($"{nameof(ExistingAccessToken)} must be set from {nameof(Update)}() or from JSON deserialization.");
		protected set => field = value;
	}

	public PrivateKey? PrivateKey { get; private set; }

	public AdpToken? AdpToken { get; private set; }

	public RefreshToken? RefreshToken { get; private set; }

	// cookies are a list instead of Dictionary<string, string> b/c of duplicates
	protected List<KeyValuePair<string, string?>>? _cookies { private get; set; }
	public IEnumerable<KeyValuePair<string, string?>> Cookies => _cookies?.AsReadOnly() ?? [];

	[JsonProperty]
	public string? DeviceSerialNumber { get; private set; }

	[JsonIgnore]
	public OAuth2? Authorization { get; private set; }

	[JsonProperty]
	public string? DeviceType { get; private set; }

	[JsonProperty]
	public string? AmazonAccountId { get; private set; }

	[JsonProperty]
	public string? DeviceName { get; private set; }

	[JsonProperty]
	public string? StoreAuthenticationCookie { get; private set; }

	protected Identity() { LocaleName = string.Empty; }

	public Identity(Locale locale)
	{
		LocaleName = ArgumentValidator.EnsureNotNull(locale, nameof(locale)).Name;
		ExistingAccessToken = AccessToken.Empty;
		_cookies = new();
	}

	public Identity(Locale locale, OAuth2 authorization, IEnumerable<KeyValuePair<string, string?>>? cookies)
	{
		LocaleName = ArgumentValidator.EnsureNotNull(locale, nameof(locale)).Name;
		Authorization = ArgumentValidator.EnsureNotNull(authorization, nameof(authorization));
		ExistingAccessToken = AccessToken.Empty;
		_cookies = cookies?.ToList();
	}

	public void Update(AccessToken accessToken)
	{
		ExistingAccessToken = ArgumentValidator.EnsureNotNull(accessToken, nameof(accessToken));
		Updated?.Invoke(this, new EventArgs());
	}

	public void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken, IEnumerable<KeyValuePair<string, string?>>? cookies, string? deviceSerialNumber = null, string? deviceType = null, string? amazonAccountId = null, string? deviceName = null, string? storeAuthenticationCookie = null)
	{
		PrivateKey = ArgumentValidator.EnsureNotNull(privateKey, nameof(privateKey));
		AdpToken = ArgumentValidator.EnsureNotNull(adpToken, nameof(adpToken));
		ExistingAccessToken = ArgumentValidator.EnsureNotNull(accessToken, nameof(accessToken));
		RefreshToken = ArgumentValidator.EnsureNotNull(refreshToken, nameof(refreshToken));

		_cookies = cookies?.ToList();

		DeviceSerialNumber = deviceSerialNumber ?? string.Empty;
		DeviceType = deviceType ?? string.Empty;
		AmazonAccountId = amazonAccountId ?? string.Empty;
		DeviceName = deviceName ?? string.Empty;
		StoreAuthenticationCookie = storeAuthenticationCookie ?? string.Empty;

		Updated?.Invoke(this, new EventArgs());

		IsValid = true;
	}

	public void Invalidate()
	{
		AdpToken = null;
		RefreshToken = null;
		ExistingAccessToken?.Invalidate();

		Updated?.Invoke(this, new EventArgs());

		IsValid = false;
	}
}
