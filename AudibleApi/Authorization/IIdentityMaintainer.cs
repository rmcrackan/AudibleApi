using AudibleApi.Cryptography;
using Dinah.Core;
using System.Threading.Tasks;

namespace AudibleApi.Authorization;

/// <summary>
/// Keeps IIdentity up to date
/// </summary>
public interface IIdentityMaintainer
{
	ISystemDateTime SystemDateTime { get; }
	Locale Locale { get; }
	string? DeviceSerialNumber { get; }
	string? DeviceType { get; }
	string? AmazonAccountId { get; }
	Task<AccessToken?> GetAccessTokenAsync();
	Task<AdpToken?> GetAdpTokenAsync();
	Task<PrivateKey?> GetPrivateKeyAsync();
}
