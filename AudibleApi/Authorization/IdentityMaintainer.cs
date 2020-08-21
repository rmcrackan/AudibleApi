using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
	public class IdentityMaintainer : IIdentityMaintainer
	{
		public ISystemDateTime SystemDateTime { get; }
		private IIdentity _identity { get; }
		private IAuthorize _authorize { get; }

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
			_identity = identity ?? throw new ArgumentNullException(nameof(identity));
			_authorize = authorize ?? throw new ArgumentNullException(nameof(authorize));
			SystemDateTime = systemDateTime ?? throw new ArgumentNullException(nameof(systemDateTime));
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
			var authRegister = await _authorize.RegisterAsync(_identity.ExistingAccessToken, _identity.Cookies.ToKeyValuePair());
			RegistrationParser.ParseRegistrationIntoIdentity(authRegister, _identity, SystemDateTime);
		}

		protected async Task DeregisterAsync()
		{
			var success = await _authorize.DeregisterAsync(_identity.ExistingAccessToken, _identity.Cookies.ToKeyValuePair());
			if (!success)
				throw new RegistrationException("Unable to deregister");

			_identity.Invalidate();
		}
	}
}
