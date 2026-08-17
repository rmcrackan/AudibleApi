using AudibleApi.Cryptography;
using Dinah.Core;
using Dinah.Core.Security;
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

	// cookies are a list instead of Dictionary<string, string> b/c of duplicates.
	// values are session credentials, so they are held as secrets: the name identifies a cookie in a log, the
	// value must never appear in one
	protected List<KeyValuePair<string, SecretString>>? _cookies { private get; set; }
	public IEnumerable<KeyValuePair<string, SecretString>> Cookies => _cookies?.AsReadOnly() ?? [];

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
	public SecretString StoreAuthenticationCookie { get; private set; }

	/// <summary>Per-field encryption persistence state. Not serialized.</summary>
	[JsonIgnore]
	internal IdentitySecretPersistence SecretPersistence { get; } = new();

	protected Identity() { LocaleName = string.Empty; }

	public Identity(Locale locale)
	{
		LocaleName = ArgumentValidator.EnsureNotNull(locale, nameof(locale)).Name;
		ExistingAccessToken = AccessToken.Empty;
		_cookies = new();
	}

	public Identity(Locale locale, OAuth2 authorization, IEnumerable<KeyValuePair<string, SecretString>>? cookies)
	{
		LocaleName = ArgumentValidator.EnsureNotNull(locale, nameof(locale)).Name;
		Authorization = ArgumentValidator.EnsureNotNull(authorization, nameof(authorization));
		ExistingAccessToken = AccessToken.Empty;
		_cookies = cookies?.ToList();
		SecretPersistence.CookiesDirty = true;
	}

	public void Update(AccessToken accessToken)
	{
		ExistingAccessToken = ArgumentValidator.EnsureNotNull(accessToken, nameof(accessToken));
		SecretPersistence.AccessToken.Dirty = true;
		Updated?.Invoke(this, new EventArgs());
	}

	public void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken, IEnumerable<KeyValuePair<string, SecretString>>? cookies, string? deviceSerialNumber = null, string? deviceType = null, string? amazonAccountId = null, string? deviceName = null, SecretString storeAuthenticationCookie = default)
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
		StoreAuthenticationCookie = storeAuthenticationCookie.Reveal() ?? string.Empty;

		SecretPersistence.MarkAllSecretsDirty();

		Updated?.Invoke(this, new EventArgs());

		IsValid = true;
	}

	public void Invalidate()
	{
		AdpToken = null;
		RefreshToken = null;
		ExistingAccessToken?.Invalidate();
		SecretPersistence.AdpToken.Dirty = true;
		SecretPersistence.RefreshToken.Dirty = true;
		SecretPersistence.AccessToken.Dirty = true;

		Updated?.Invoke(this, new EventArgs());

		IsValid = false;
	}
}
