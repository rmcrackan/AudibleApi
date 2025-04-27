using System;

namespace AudibleApi
{
	public static class Resources
	{
        /*
		internal const string User_Agent = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148";
		public const string Download_User_Agent = "Audible/671 CFNetwork/1240.0.4 Darwin/20.6.0";
		internal const string DeviceType = "A2CZJZGLK2JJVM";
		internal const string OsVersion = "15.0.0";
		internal const string SoftwareVersion = "35602678";
		internal const string AppVersion = "3.56.2";
		internal const string AppName = "Audible";
		internal const string DeviceModel = "iPhone";
		*/

        //Android
        internal const string User_Agent = "Dalvik/2.1.0 (Linux; U; Android 14; Android SDK built for x86_64 Build/UE1A.230829.036.A1)";
		public const string Download_User_Agent = "com.audible.playersdk.player/3.96.1 (Linux;Android 14) AndroidXMedia3/1.3.0";
		public const string DeviceType = "A10KISP2GWF0E4";
		internal const string OsVersion = "Android/"+ DeviceProduct + "/emu64x:14/UE1A.230829.036.A1/11228894:userdebug/test-keys";
		internal const string OsVersionNumber = "34";
		internal const string SoftwareVersion = "130050002";
		internal const string AppVersion = "177102";
		internal const string AppName = "com.audible.application";
		internal const string DeviceModel = "Android SDK built for x86_64";
		internal const string OsFamily = "android";
		internal const string DeviceProduct = "sdk_phone64_x86_64";

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
}
