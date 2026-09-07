using System;
using System.Collections.Generic;

namespace AudibleApi;

/// <summary>
/// Which virtual-device recipe to send to Amazon at login/register.
/// Existing tokens cannot be converted; the profile is used only for a new registration.
/// </summary>
public enum DeviceRegistrationKind
{
	/// <summary>Today's Android emulator registration. Default; Widevine-capable.</summary>
	CurrentAndroid = 0,

    //
    // Note: this one isn't really needed, however keep it just in case we have an error in the future. If one or the other device registrations stops working, there's a backup already available.
    // https://github.com/rmcrackan/Libation/issues/2021#issuecomment-5564352730
    // iPhone does not support Widevine: https://github.com/rmcrackan/Libation/issues/2021#issuecomment-5564352730
    //
    /// <summary>mkb79/audible-cli iPhone registration. Not Widevine-capable.</summary>
    Mkb79IPhone = 1,

	/// <summary>
	/// Same Android Audible app device type as <see cref="CurrentAndroid"/> (Widevine-capable)
	/// but a retail Pixel fingerprint instead of the emulator.
	/// </summary>
	RetailAndroid = 2,
}

/// <summary>
/// Constants and flags for one Audible/Amazon device-registration recipe.
/// Live Android app identifiers used for Widevine stay on <see cref="Resources"/>.
/// </summary>
public sealed class DeviceRegistrationProfile
{
	public static DeviceRegistrationProfile CurrentAndroid { get; } = CreateCurrentAndroid();
	public static DeviceRegistrationProfile Mkb79IPhone { get; } = CreateMkb79IPhone();
	public static DeviceRegistrationProfile Default => CurrentAndroid;
	public static IEnumerable<DeviceRegistrationProfile> AllProfiles => [CurrentAndroid, Mkb79IPhone];

	public DeviceRegistrationKind Kind { get; private init; }
	public string Description { get; private init; } = "";

	/// <summary>Amazon device type id. Android Audible app is <see cref="Resources.DeviceType"/>.</summary>
	public string DeviceType { get; private init; } = "";

	/// <summary>Shown on Amazon's device list. Never a third-party product name.</summary>
	public string AmazonDeviceName { get; private init; } = "";

	public string AppName { get; private init; } = "";
	public string AppVersion { get; private init; } = "";
	public string? AppVersionName { get; private init; }
	public string DeviceModel { get; private init; } = "";
	public string OsVersion { get; private init; } = "";
	public string? OsVersionNumber { get; private init; }
	public string SoftwareVersion { get; private init; } = "";
	public string UserAgent { get; private init; } = "";
	public string DownloadUserAgent { get; private init; } = "";

	public string? OsFamily { get; private init; }
	public string? Manufacturer { get; private init; }
	public string? DeviceProduct { get; private init; }
	public string? MapVersion { get; private init; }
	public string? FrcDeviceName { get; private init; }
	public string? FrcApplicationVersion { get; private init; }
	public string? ScreenWidthPixels { get; private init; }
	public string? ScreenHeightPixels { get; private init; }

	public string RegistrationDataDomain { get; private init; } = "DeviceLegacy";
	public bool IncludeDeviceMetadata { get; private init; }
	public bool UseGlobalAuthentication { get; private init; }
	public bool UseIosLoginSurface { get; private init; }
	public bool IncludeLocalIpInFrc { get; private init; }
	public bool UseIosDeviceSerial { get; private init; }

	public bool IsAndroidAudibleApp => DeviceType == Resources.DeviceType;

	public static DeviceRegistrationProfile FromKind(DeviceRegistrationKind kind)
		=> kind switch
		{
			DeviceRegistrationKind.CurrentAndroid or DeviceRegistrationKind.RetailAndroid => CurrentAndroid,
			DeviceRegistrationKind.Mkb79IPhone => Mkb79IPhone,
			_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown device registration kind.")
		};

	private static DeviceRegistrationProfile CreateCurrentAndroid()
		=> new()
		{
			Kind = DeviceRegistrationKind.CurrentAndroid,
			Description = "Android emulator (default)",
			DeviceType = Resources.DeviceType,
			AmazonDeviceName = "Audible for Android",
			AppName = Resources.AppName,
			AppVersion = Resources.AppVersion,
			AppVersionName = Resources.AppVersionName,
			DeviceModel = Resources.DeviceModel,
			OsVersion = Resources.OsVersion,
			OsVersionNumber = Resources.OsVersionNumber,
			SoftwareVersion = Resources.SoftwareVersion,
			UserAgent = Resources.User_Agent,
			DownloadUserAgent = Resources.Download_User_Agent,
			OsFamily = Resources.OsFamily,
			Manufacturer = Resources.Manufacturer,
			DeviceProduct = Resources.DeviceProduct,
			MapVersion = Resources.MapVersion,
			FrcDeviceName = Resources.DeviceName,
			FrcApplicationVersion = Resources.AppVersion,
			ScreenWidthPixels = "1080",
			ScreenHeightPixels = "2400",
			RegistrationDataDomain = "DeviceLegacy",
			IncludeDeviceMetadata = true,
			UseGlobalAuthentication = true,
			UseIosLoginSurface = false,
			IncludeLocalIpInFrc = true,
			UseIosDeviceSerial = false,
		};

	private static DeviceRegistrationProfile CreateMkb79IPhone()
		=> new()
		{
			Kind = DeviceRegistrationKind.Mkb79IPhone,
			Description = "iPhone / audible-cli (experimental; no Widevine)",
			DeviceType = "A2CZJZGLK2JJVM",
			AmazonDeviceName = "Audible for iPhone",
			AppName = "Audible",
			AppVersion = "3.56.2",
			AppVersionName = null,
			DeviceModel = "iPhone",
			OsVersion = "15.0.0",
			OsVersionNumber = null,
			SoftwareVersion = "35602678",
			UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148",
			DownloadUserAgent = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0",
			OsFamily = null,
			Manufacturer = null,
			DeviceProduct = null,
			MapVersion = null,
			FrcDeviceName = null,
			FrcApplicationVersion = null,
			ScreenWidthPixels = null,
			ScreenHeightPixels = null,
			RegistrationDataDomain = "Device",
			IncludeDeviceMetadata = false,
			UseGlobalAuthentication = false,
			UseIosLoginSurface = true,
			IncludeLocalIpInFrc = false,
			UseIosDeviceSerial = true,
		};
}
