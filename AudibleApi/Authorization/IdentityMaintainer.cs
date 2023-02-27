using System;
using System.Threading.Tasks;
using AudibleApi.Cryptography;
using Dinah.Core;

namespace AudibleApi.Authorization
{
    public class IdentityMaintainer : IIdentityMaintainer
	{
		public ISystemDateTime SystemDateTime { get; }
		public Locale Locale => _identity.Locale;

		private IIdentity _identity { get; }
		private IAuthorize _authorize { get; }

		public string DeviceSerialNumber => _identity.DeviceSerialNumber;

		public string DeviceType => _identity.DeviceType;

		public string AmazonAccountId => _identity.AmazonAccountId;

		public static async Task<IdentityMaintainer> CreateAsync(IIdentity identity)
		{
			StackBlocker.ApiTestBlocker();

			var authorize = new Authorize(identity?.Locale);
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

		public async Task<AccessToken> GetAccessTokenAsync()
		{
			await EnsureStateAsync();
			return _identity.ExistingAccessToken;
		}

		public async Task<AdpToken> GetAdpTokenAsync()
		{
			await EnsureStateAsync();
			return _identity.AdpToken;
		}

		public async Task<PrivateKey> GetPrivateKeyAsync()
		{
			await EnsureStateAsync();
			return _identity.PrivateKey;
		}

		/// <summary>
		/// Ensures a full valid state. Loads missing values. Reloads expired values.
		/// </summary>
		protected async Task EnsureStateAsync()
		{
			try
			{
				await RefreshAccessTokenAsync();
			}
			catch
			{
				try
				{
					await DeregisterAsync();
					await RegisterAsync();
				}
				catch (Exception ex)
				{
					throw new RegistrationException("Error ensuring valid state", ex);
				}
			}
		}

		protected async Task RefreshAccessTokenAsync()
		{
			if (SystemDateTime.UtcNow < _identity.ExistingAccessToken.Expires)
				return;

			// is expired. refresh
			_identity.Update(await _authorize.RefreshAccessTokenAsync(_identity.RefreshToken));
		}

		protected async Task RegisterAsync()
		{
			var authRegister = await _authorize.RegisterAsync(_identity.Authorization);
			RegistrationParser.ParseRegistrationIntoIdentity(authRegister, _identity, SystemDateTime);
		}

		protected async Task DeregisterAsync()
		{
			var success = await _authorize.DeregisterAsync(_identity.ExistingAccessToken, _identity.Cookies);
			if (!success)
				throw new RegistrationException("Unable to deregister");

			_identity.Invalidate();
		}
	}
}
