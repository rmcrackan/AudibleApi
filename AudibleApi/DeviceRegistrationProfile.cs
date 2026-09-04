using System;

namespace AudibleApi;

/// <summary>
/// Which virtual-device recipe to send to Amazon at login/register.
/// Existing tokens cannot be converted; the profile is used only for a new registration.
/// </summary>
public enum DeviceRegistrationKind
{
	/// <summary>Today's Android emulator registration. Default; Widevine-capable.</summary>
	CurrentAndroid = 0,

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
	public static DeviceRegistrationProfile RetailAndroid { get; } = CreateRetailAndroid();
	public static DeviceRegistrationProfile Default => CurrentAndroid;

	public DeviceRegistrationKind Kind { get; private init; }

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
			DeviceRegistrationKind.CurrentAndroid => CurrentAndroid,
			DeviceRegistrationKind.Mkb79IPhone => Mkb79IPhone,
			DeviceRegistrationKind.RetailAndroid => RetailAndroid,
			_ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown device registration kind.")
		};

	private static DeviceRegistrationProfile CreateCurrentAndroid()
		=> new()
		{
			Kind = DeviceRegistrationKind.CurrentAndroid,
			DeviceType = Resources.DeviceType,
			AmazonDeviceName = "Audible",
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
			FrcApplicationVersion = "2090254511",
			ScreenWidthPixels = "1344",
			ScreenHeightPixels = "2769",
			RegistrationDataDomain = "DeviceLegacy",
			IncludeDeviceMetadata = true,
			UseGlobalAuthentication = true,
			UseIosLoginSurface = false,
			IncludeLocalIpInFrc = true,
			UseIosDeviceSerial = false,
		};

	private static DeviceRegistrationProfile CreateRetailAndroid()
		=> new()
		{
			Kind = DeviceRegistrationKind.RetailAndroid,
			DeviceType = Resources.DeviceType,
			AmazonDeviceName = "Audible",
			AppName = Resources.AppName,
			AppVersion = Resources.AppVersion,
			AppVersionName = Resources.AppVersionName,
			DeviceModel = "Pixel 8",
			OsVersion = "google/shiba/shiba:14/AP2A.240805.005/12025142:user/release-keys",
			OsVersionNumber = Resources.OsVersionNumber,
			SoftwareVersion = Resources.SoftwareVersion,
			UserAgent = "Mozilla/5.0 (Linux; Android 14; Pixel 8 Build/AP2A.240805.005; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/113.0.5672.136 Mobile Safari/537.36",
			DownloadUserAgent = Resources.Download_User_Agent,
			OsFamily = Resources.OsFamily,
			Manufacturer = "Google",
			DeviceProduct = "shiba",
			MapVersion = Resources.MapVersion,
			FrcDeviceName = "Pixel 8",
			FrcApplicationVersion = Resources.AppVersion,
			ScreenWidthPixels = "1080",
			ScreenHeightPixels = "2400",
			RegistrationDataDomain = "DeviceLegacy",
			IncludeDeviceMetadata = true,
			UseGlobalAuthentication = true,
			UseIosLoginSurface = false,
			IncludeLocalIpInFrc = false,
			UseIosDeviceSerial = false,
		};

	private static DeviceRegistrationProfile CreateMkb79IPhone()
		=> new()
		{
			Kind = DeviceRegistrationKind.Mkb79IPhone,
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
