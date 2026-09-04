using System;

namespace AudibleApi;

public static class Resources
{
	// iPhone registration constants live on DeviceRegistrationProfile.Mkb79IPhone.
	// DeviceType here is the Android Audible app id; Widevine is gated on it.

	//Android
	public const string User_Agent = "Mozilla/5.0 (Linux; Android 14; sdk_gphone64_x86_64 Build/UPB5.230623.003; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/113.0.5672.136 Mobile Safari/537.36";
	public const string Download_User_Agent = "com.audible.playersdk.player/3.96.1 (Linux;Android 14) AndroidXMedia3/1.3.0";
	public const string DeviceType = "A10KISP2GWF0E4";
	internal const string OsVersion = @"google/" + DeviceModel + "/emu64xa:14/UPB5.230623.003/10615560:userdebug/dev-keys";
	internal const string OsVersionNumber = "34";
	internal const string SoftwareVersion = "130050002";
	internal const string AppVersion = "2090253826";
	internal const string AppVersionName = "25.38.26";
	internal const string AppName = "com.audible.application";
	internal const string DeviceModel = "sdk_gphone64_x86_64";
	internal const string DeviceName = "ranchu/Google/" + DeviceModel;
	internal const string OsFamily = "android";
	internal const string Manufacturer = "Google";
	internal const string DeviceProduct = "sdk_phone64_x86_64";
	internal const string MapVersion = "MAPAndroidLib-1.3.40908.0";

	public static string LoginDomain(this Locale locale) => locale.WithUsername ? "audible" : "amazon";

	public static string RegisterDomain(this Locale locale) => $".amazon.{locale.TopDomain}";

	private static string _audibleApiUrl(this Locale locale) => $"https://api.audible.{locale.TopDomain}";
	public static Uri AudibleApiUri(this Locale locale) => new Uri(locale._audibleApiUrl());

	private static string _audibleLoginUrl(this Locale locale) => $"https://www.audible.{locale.TopDomain}";
	public static Uri AudibleLoginUri(this Locale locale) => new Uri(locale._audibleLoginUrl());

	private static string _amazonApiUrl(this Locale locale) => $"https://api.amazon.{locale.TopDomain}";
	public static Uri AmazonApiUri(this Locale locale) => new Uri(locale._amazonApiUrl());

	private static string _loginUrl(this Locale locale) => $"https://www.{locale.LoginDomain()}.{locale.TopDomain}";
	public static Uri LoginUri(this Locale locale) => new Uri(locale._loginUrl());

	private static string _registrationUrl(this Locale locale) => $"https://api.{locale.LoginDomain()}.{locale.TopDomain}";
	public static Uri RegistrationUri(this Locale locale) => new Uri(locale._registrationUrl());
}
