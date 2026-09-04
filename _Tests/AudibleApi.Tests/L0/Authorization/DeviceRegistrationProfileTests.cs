using System.Text;
using AudibleApi.Cryptography;

namespace Authorization.DeviceRegistrationTests;

[TestClass]
public class DeviceRegistrationProfileTests
{
	[TestMethod]
	public void default_is_current_android()
	{
		DeviceRegistrationProfile.Default.ShouldBe(DeviceRegistrationProfile.CurrentAndroid);
		DeviceRegistrationProfile.FromKind(DeviceRegistrationKind.CurrentAndroid)
			.ShouldBe(DeviceRegistrationProfile.CurrentAndroid);
	}

	[TestMethod]
	public void android_profiles_keep_the_widevine_device_type()
	{
		DeviceRegistrationProfile.CurrentAndroid.DeviceType.ShouldBe(Resources.DeviceType);
		DeviceRegistrationProfile.RetailAndroid.DeviceType.ShouldBe(Resources.DeviceType);
		DeviceRegistrationProfile.CurrentAndroid.IsAndroidAudibleApp.ShouldBeTrue();
		DeviceRegistrationProfile.RetailAndroid.IsAndroidAudibleApp.ShouldBeTrue();
		DeviceRegistrationProfile.Mkb79IPhone.IsAndroidAudibleApp.ShouldBeFalse();
	}

	[TestMethod]
	public void default_registration_options_use_audible_not_libation()
	{
		new RegistrationOptions().DeviceName.ShouldBe("Audible");
		new RegistrationOptions().Profile.ShouldBe(DeviceRegistrationProfile.CurrentAndroid);
	}

	[TestMethod]
	public void amazon_device_name_is_never_libation()
	{
		foreach (var kind in Enum.GetValues<DeviceRegistrationKind>())
			DeviceRegistrationProfile.FromKind(kind).AmazonDeviceName.ShouldNotContain("Libation");
	}
}

[TestClass]
public class RegistrationOptionsOAuthUrl
{
	[TestMethod]
	public void current_android_uses_android_oauth_surface()
	{
		var q = Query(new RegistrationOptions(DeviceRegistrationProfile.CurrentAndroid), Locales.Us);

		q["openid.assoc_handle"].ShouldBe("amzn_audible_android_aui_us");
		q["pageId"].ShouldBe("amzn_audible_android_aui_v2_dark_usus");
		q["openid.return_to"].ShouldBe("https://www.audible.com/ap/maplanding");
		q["disableLoginPrepopulate"].ShouldBe("1");
		q["forceMobileLayout"].ShouldBeNull();
		q["openid.oa2.client_id"].ShouldStartWith("device:");
	}

	[TestMethod]
	public void mkb79_iphone_uses_ios_oauth_surface()
	{
		var q = Query(new RegistrationOptions(DeviceRegistrationProfile.Mkb79IPhone), Locales.Us);

		q["openid.assoc_handle"].ShouldBe("amzn_audible_ios_us");
		q["pageId"].ShouldBe("amzn_audible_ios");
		q["openid.return_to"].ShouldBe("https://www.amazon.com/ap/maplanding");
		q["forceMobileLayout"].ShouldBe("true");
		q["disableLoginPrepopulate"].ShouldBeNull();
	}

	[TestMethod]
	public void mkb79_iphone_pre_amazon_stays_on_audible()
	{
		var locale = Localization.Get("pre-amazon - germany");
		var q = Query(new RegistrationOptions(DeviceRegistrationProfile.Mkb79IPhone), locale);

		q["openid.assoc_handle"].ShouldBe("amzn_audible_ios_lap_de");
		q["pageId"].ShouldBe("amzn_audible_ios_privatepool");
		q["openid.return_to"].ShouldBe("https://www.audible.de/ap/maplanding");
	}

	[TestMethod]
	public void retail_android_keeps_android_oauth_surface()
	{
		var q = Query(new RegistrationOptions(DeviceRegistrationProfile.RetailAndroid), Locales.Us);

		q["openid.assoc_handle"].ShouldBe("amzn_audible_android_aui_us");
		q["pageId"].ShouldBe("amzn_audible_android_aui_v2_dark_usus");
		q["openid.return_to"].ShouldBe("https://www.audible.com/ap/maplanding");
	}

	private static System.Collections.Specialized.NameValueCollection Query(RegistrationOptions options, Locale locale)
	{
		var url = options.OAuthUrl(locale);
		url.Host.ShouldNotBeNull();
		return System.Web.HttpUtility.ParseQueryString(url.Query);
	}
}

[TestClass]
public class RegistrationBody
{
	[TestMethod]
	[DataRow(DeviceRegistrationKind.CurrentAndroid)]
	[DataRow(DeviceRegistrationKind.RetailAndroid)]
	[DataRow(DeviceRegistrationKind.Mkb79IPhone)]
	public void device_name_is_not_libation(DeviceRegistrationKind kind)
	{
		var body = Body(kind);
		var name = body["registration_data"]?["device_name"]?.ToString();
		name.ShouldNotBeNull();
		name.ShouldNotContain("Libation");
		name.ShouldContain("Audible");
	}

	[TestMethod]
	public void current_android_keeps_emulator_register_payload()
	{
		var options = new RegistrationOptions(DeviceRegistrationProfile.CurrentAndroid);
		var body = Body(options);
		var data = (JObject)body["registration_data"]!;

		data["domain"]!.ToString().ShouldBe("DeviceLegacy");
		data["device_type"]!.ToString().ShouldBe(Resources.DeviceType);
		data["device_model"]!.ToString().ShouldBe("sdk_gphone64_x86_64");
		data["app_name"]!.ToString().ShouldBe("com.audible.application");
		data["device_name"]!.ToString().ShouldEndWith("Audible");
		body["device_metadata"].ShouldNotBeNull();
		body["device_metadata"]!["product"]!.ToString().ShouldBe("sdk_phone64_x86_64");
		body["auth_data"]!["use_global_authentication"]!.ToString().ShouldBe("true");
		body["cookies"]!["domain"]!.ToString().ShouldContain("audible.com");
		ClientIdContains(options, Resources.DeviceType);
	}

	[TestMethod]
	public void mkb79_iphone_matches_audible_cli_register_payload()
	{
		var options = new RegistrationOptions(DeviceRegistrationProfile.Mkb79IPhone);
		var body = Body(options);
		var data = (JObject)body["registration_data"]!;

		data["domain"]!.ToString().ShouldBe("Device");
		data["device_type"]!.ToString().ShouldBe("A2CZJZGLK2JJVM");
		data["device_model"]!.ToString().ShouldBe("iPhone");
		data["app_name"]!.ToString().ShouldBe("Audible");
		data["app_version"]!.ToString().ShouldBe("3.56.2");
		data["os_version"]!.ToString().ShouldBe("15.0.0");
		data["device_name"]!.ToString().ShouldEndWith("Audible for iPhone");
		body["device_metadata"].ShouldBeNull();
		body["auth_data"]!["use_global_authentication"].ShouldBeNull();
		body["cookies"]!["domain"]!.ToString().ShouldBe(".amazon.com");
		options.DeviceSerialNumber.Length.ShouldBe(32);
		ClientIdContains(options, "A2CZJZGLK2JJVM");
	}

	[TestMethod]
	public void retail_android_is_android_without_emulator_strings()
	{
		var options = new RegistrationOptions(DeviceRegistrationProfile.RetailAndroid);
		var body = Body(options);
		var data = (JObject)body["registration_data"]!;

		data["device_type"]!.ToString().ShouldBe(Resources.DeviceType);
		data["device_model"]!.ToString().ShouldBe("Pixel 8");
		data["os_version"]!.ToString().ShouldContain("release-keys");
		data["os_version"]!.ToString().ShouldNotContain("ranchu");
		data["os_version"]!.ToString().ShouldNotContain("sdk_gphone");
		body["device_metadata"].ShouldNotBeNull();
		body["device_metadata"]!["product"]!.ToString().ShouldBe("shiba");
		body["device_metadata"]!["model"]!.ToString().ShouldBe("Pixel 8");
		ClientIdContains(options, Resources.DeviceType);
	}

	private static JObject Body(DeviceRegistrationKind kind)
		=> Body(new RegistrationOptions(DeviceRegistrationProfile.FromKind(kind)));

	private static JObject Body(RegistrationOptions options)
	{
		var oauth = OAuth2.Parse("https://www.amazon.com/ap/maplanding?openid.oa2.authorization_code=abc")
			?? throw new InvalidOperationException("parse failed");
		oauth.RegistrationOptions = options;
		return oauth.GetRegistrationBody(Locales.Us);
	}

	private static void ClientIdContains(RegistrationOptions options, string deviceType)
	{
		var expected = Convert.ToHexStringLower(Encoding.UTF8.GetBytes($"{options.DeviceSerialNumber}#{deviceType}"));
		options.ClientID.ShouldBe(expected);
	}
}

[TestClass]
public class SignInCookies
{
	[TestMethod]
	public void current_android_sends_frc_map_md_and_sid()
	{
		var options = new RegistrationOptions(DeviceRegistrationProfile.CurrentAndroid);
		var cookies = options.GetSignInCookies(Locales.Us).Cast<Cookie>().ToDictionary(c => c.Name, c => c);

		cookies.Keys.OrderBy(k => k).ShouldBe(["frc", "map-md", "sid"]);
		DecodeMapMd(cookies["map-md"].Value)["app_identifier"]!["package"]!.ToString()
			.ShouldBe("com.audible.application");

		var frc = JObject.Parse(FrcEncoder.Decode(options.DeviceSerialNumber, cookies["frc"].Value));
		frc["DeviceName"]!.ToString().ShouldContain("ranchu");
		frc["IpAddress"].ShouldNotBeNull();
	}

	[TestMethod]
	public void mkb79_iphone_sends_ios_init_cookies()
	{
		var cookies = new RegistrationOptions(DeviceRegistrationProfile.Mkb79IPhone)
			.GetSignInCookies(Locales.Us)
			.Cast<Cookie>()
			.ToDictionary(c => c.Name, c => c);

		cookies.Keys.OrderBy(k => k).ShouldBe(["amzn-app-id", "frc", "map-md"]);
		cookies["amzn-app-id"].Value.ShouldBe("MAPiOSLib/6.0/ToHideRetailLink");
		cookies.ContainsKey("sid").ShouldBeFalse();

		var mapMd = DecodeMapMd(cookies["map-md"].Value);
		mapMd["app_identifier"]!["bundle_id"]!.ToString().ShouldBe("com.audible.iphone");
		mapMd["app_identifier"]!["app_version"]!.ToString().ShouldBe("3.56.2");
	}

	[TestMethod]
	public void retail_android_frc_is_not_an_emulator_and_has_no_local_ip()
	{
		var options = new RegistrationOptions(DeviceRegistrationProfile.RetailAndroid);
		var frcCookie = options.GetSignInCookies(Locales.Us).Cast<Cookie>().Single(c => c.Name == "frc");
		var frc = JObject.Parse(FrcEncoder.Decode(options.DeviceSerialNumber, frcCookie.Value));

		frc["DeviceName"]!.ToString().ShouldBe("Pixel 8");
		frc["DeviceOSVersion"]!.ToString().ShouldContain("release-keys");
		frc["DeviceOSVersion"]!.ToString().ShouldNotContain("userdebug");
		frc["IpAddress"].ShouldBeNull();
	}

	private static JObject DecodeMapMd(string value)
	{
		var padded = value.PadRight(value.Length + (4 - value.Length % 4) % 4, '=');
		return JObject.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(padded)));
	}
}
