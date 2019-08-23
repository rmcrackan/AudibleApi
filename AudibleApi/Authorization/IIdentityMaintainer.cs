using System;
using System.Threading.Tasks;
using Dinah.Core;

namespace AudibleApi.Authorization
{
	/// <summary>
	/// Keeps IIdentity up to date
	/// </summary>
	public interface IIdentityMaintainer
	{
		ISystemDateTime SystemDateTime { get; }

		Task<AccessToken> GetAccessTokenAsync();
		Task<AdpToken> GetAdpTokenAsync();
		Task<PrivateKey> GetPrivateKeyAsync();
	}
}
