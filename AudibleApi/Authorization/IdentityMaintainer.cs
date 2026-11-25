using AudibleApi.Cryptography;
using Dinah.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AudibleApi.Authorization;

public class IdentityMaintainer : IIdentityMaintainer
{
	public ISystemDateTime SystemDateTime { get; }
	public Locale Locale => _identity.Locale;

	private IIdentity _identity { get; }
	private IAuthorize _authorize { get; }
	private readonly SemaphoreSlim _refreshLock = new(1, 1);

	public string? DeviceSerialNumber => _identity.DeviceSerialNumber;

	public string? DeviceType => _identity.DeviceType;

	public string? AmazonAccountId => _identity.AmazonAccountId;

	public static async Task<IdentityMaintainer> CreateAsync(IIdentity identity)
	{
		ArgumentNullException.ThrowIfNull(identity, nameof(identity));
		StackBlocker.ApiTestBlocker();

		var authorize = new Authorize(identity.Locale);
		var systemDateTime = new SystemDateTime();

		return await CreateAsync(identity, authorize, systemDateTime);
	}
	public static async Task<IdentityMaintainer> CreateAsync(IIdentity identity, IAuthorize authorize, ISystemDateTime systemDateTime)
	{
		var maintainer = new IdentityMaintainer(identity, authorize, systemDateTime);

		if (!identity.IsValid)
			await maintainer.RegisterAsync();

		await maintainer.EnsureStateAsync();
		return maintainer;
	}

	protected IdentityMaintainer(IIdentity identity, IAuthorize authorize, ISystemDateTime systemDateTime)
	{
		_identity = ArgumentValidator.EnsureNotNull(identity, nameof(identity));
		_authorize = ArgumentValidator.EnsureNotNull(authorize, nameof(authorize));
		SystemDateTime = ArgumentValidator.EnsureNotNull(systemDateTime, nameof(systemDateTime));
	}

	public async Task<AccessToken?> GetAccessTokenAsync()
	{
		await EnsureStateAsync();
		return _identity.ExistingAccessToken;
	}

	public async Task<AdpToken?> GetAdpTokenAsync()
	{
		await EnsureStateAsync();
		return _identity.AdpToken;
	}

	public async Task<PrivateKey?> GetPrivateKeyAsync()
	{
		await EnsureStateAsync();
		return _identity.PrivateKey;
	}

	/// <summary>
	/// Reloads expired access tokens.
	/// </summary>
	protected async Task EnsureStateAsync()
	{
		if (!NeedsAccessTokenRefresh())
			return;

		await _refreshLock.WaitAsync();
		try
		{
			await RefreshAccessTokenAsync();
		}
		catch (Exception refreshEx)
		{
			// Do not log token values; message/type only.
			// Never attempt DeregisterAsync()/RegisterAsync() here: OAuth2 PKCE registration requires
			// a fresh interactive authorization code (_identity.Authorization is null for stored identities),
			// and calling DeregisterAsync() when an access token just expired can succeed within server
			// clock skew and permanently wipe stored credentials on a transient refresh failure.
			Serilog.Log.Error(
				refreshEx,
				"Access token refresh failed ({ExceptionType}): {Message}",
				refreshEx.GetType().FullName,
				refreshEx.Message);

			throw new RegistrationException("Error ensuring valid state", refreshEx);
		}
		finally
		{
			_refreshLock.Release();
		}
	}

	private bool NeedsAccessTokenRefresh()
		=> _identity.RefreshToken is not null && SystemDateTime.UtcNow >= _identity.ExistingAccessToken.Expires;

	protected async Task RefreshAccessTokenAsync()
	{
		if (!NeedsAccessTokenRefresh())
			return;

		// is expired. refresh
		_identity.Update(await _authorize.RefreshAccessTokenAsync(_identity.RefreshToken!));
	}

	protected async Task RegisterAsync()
	{
		if (_identity.Authorization is null)
			throw new RegistrationException("Cannot register without authorization data");
		var authRegister = await _authorize.RegisterAsync(_identity.Authorization);
		RegistrationParser.ParseRegistrationIntoIdentity(authRegister, _identity, SystemDateTime);
	}

	/// <summary>
	/// Deregisters the device and invalidates the current identity. Retained for protected subclass use.
	/// </summary>
	protected async Task DeregisterAsync()
	{
		var success = await _authorize.DeregisterAsync(_identity.ExistingAccessToken, _identity.Cookies);
		if (!success)
			throw new RegistrationException("Unable to deregister");

		_identity.Invalidate();
	}
}
