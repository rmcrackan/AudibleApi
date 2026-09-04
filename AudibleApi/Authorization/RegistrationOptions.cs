using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace AudibleApi.Authorization;

public record RegistrationOptions
{
	public DeviceRegistrationProfile Profile { get; }
	public string DeviceName => Profile.AmazonDeviceName;
	public string CodeVerifier { get; }
	public string ChallengeCode { get; }
	public string DeviceSerialNumber { get; }
	public string ClientID { get; }

	public RegistrationOptions(DeviceRegistrationProfile? profile = null)
	{
		Profile = profile ?? DeviceRegistrationProfile.Default;
		DeviceSerialNumber = build_device_serial(Profile);
		CodeVerifier = create_code_verifier();
		ClientID = build_client_id(DeviceSerialNumber, Profile.DeviceType);
		ChallengeCode = create_s256_code_challenge(CodeVerifier);
	}

	public Uri OAuthUrl(Locale locale)
	{
		string return_to;
		string assoc_handle;
		string page_id;

		if (Profile.UseIosLoginSurface)
		{
			// mkb79/Audible login.py: amazon maplanding except pre-Amazon username locales.
			return_to = $"{locale.LoginUri().GetOrigin()}/ap/maplanding";
			assoc_handle = locale.WithUsername
				? $"amzn_audible_ios_lap_{locale.CountryCode}"
				: $"amzn_audible_ios_{locale.CountryCode}";
			page_id = locale.WithUsername ? "amzn_audible_ios_privatepool" : "amzn_audible_ios";
		}
		else
		{
			// According to static analysis of the Audible v25.38.26 apk,
			// the return_to domain is always www.audible.TLD, even for private pool accounts.
			return_to = $"{locale.AudibleLoginUri().GetOrigin()}/ap/maplanding";
			assoc_handle = locale.WithUsername
				? $"amzn_audible_android_aui_lap_{locale.CountryCode}"
				: $"amzn_audible_android_aui_{locale.CountryCode}";
			page_id = locale.WithUsername
				? $"amzn_audible_android_privatepool_aui_v2_dark_{locale.CountryCode}"
				: $"amzn_audible_android_aui_v2_dark_us{locale.CountryCode}";
		}

		var oauth_params = new Dictionary<string, string>
		{
			{ "openid.pape.max_auth_age", "0"},
			{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
			{ "accountStatusPolicy", "P1"},
			{ "marketPlaceId", locale.MarketPlaceId},
			{ "pageId", page_id},
			{ "openid.return_to", return_to},
			{ "openid.assoc_handle", assoc_handle},
			{ "openid.oa2.response_type", "code"},
			{ "openid.mode", "checkid_setup"},
			{ "openid.ns.pape", "http://specs.openid.net/extensions/pape/1.0"},
			{ "openid.oa2.code_challenge_method", "S256"},
			{ "openid.ns.oa2", "http://www.amazon.com/ap/ext/oauth/2"},
			{ "openid.oa2.code_challenge", ChallengeCode },
			{ "openid.oa2.scope", "device_auth_access"},
			{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" },
			{ "openid.oa2.client_id", $"device:{ClientID}"},
			{ "openid.ns", "http://specs.openid.net/auth/2.0"},
		};

		if (Profile.UseIosLoginSurface)
			oauth_params["forceMobileLayout"] = "true";
		else
			oauth_params["disableLoginPrepopulate"] = "1";

		return new Uri(locale.LoginUri(), $"/ap/signin?{urlencode(oauth_params)}");
	}

	public CookieCollection GetSignInCookies(Locale locale)
	{
		var cookieDomain = $".{locale.LoginDomain()}.{locale.TopDomain}";

		if (Profile.UseIosLoginSurface)
		{
			return new CookieCollection
			{
				new Cookie("frc", create_ios_frc_cookie(), "/ap", cookieDomain),
				new Cookie("map-md", create_ios_map_md_cookie(), "/ap", cookieDomain),
				new Cookie("amzn-app-id", "MAPiOSLib/6.0/ToHideRetailLink", "/ap", cookieDomain),
			};
		}

		return new CookieCollection
		{
			new Cookie("frc", create_android_frc_cookie(locale, DeviceSerialNumber), "/ap", cookieDomain),
			new Cookie("map-md", create_android_map_md_cookie(), "/ap", cookieDomain),
			new Cookie("sid", "", "/", cookieDomain),
		};
	}

	private static string urlencode(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
		=> nameValuePairs
		.Select(kvp => $"{System.Web.HttpUtility.UrlEncode(kvp.Key)}={System.Web.HttpUtility.UrlEncode(kvp.Value)}")
		.Aggregate((a, b) => $"{a}&{b}");

	// https://github.com/mkb79/Audible/blob/master/src/audible/login.py
	private static string build_device_serial(DeviceRegistrationProfile profile)
	{
		if (profile.UseIosDeviceSerial)
			return Guid.NewGuid().ToString("N").ToUpperInvariant();

		Span<byte> serial_bytes = stackalloc byte[20];
		Random.Shared.NextBytes(serial_bytes);
		return Convert.ToHexStringLower(serial_bytes);
	}

	private static string create_code_verifier()
	{
		Span<byte> code_verifier = stackalloc byte[32];
		Random.Shared.NextBytes(code_verifier);
		return Base64Url.EncodeToString(code_verifier);
	}

	private static string build_client_id(string deviceSerialNumber, string deviceType)
	{
		var client_id_bytes = Encoding.UTF8.GetBytes($"{deviceSerialNumber}#{deviceType}");
		return Convert.ToHexStringLower(client_id_bytes);
	}

	private static string create_s256_code_challenge(string code_verifier)
	{
		var hash = SHA256.HashData(Encoding.ASCII.GetBytes(code_verifier));
		return Base64Url.EncodeToString(hash);
	}

	private string create_android_map_md_cookie()
	{
		var mapMd = new JObject
		{
			{ "device_registration_data",
				new JObject {
					{"software_version", Profile.SoftwareVersion }
				}
			},
			{ "app_identifier",
				new JObject {
					{"package", Profile.AppName },
					{"SHA-256", null },
					{"app_version", Profile.AppVersion },
					{"app_version_name", Profile.AppVersionName },
					{"app_sms_hash", null },
					{"map_version", Profile.MapVersion }
				}
			},
			{"app_info",
				new JObject {
					{ "auto_pv", 0 },
					{ "auto_pv_with_smsretriever", 1 },
					{ "smartlock_supported", 0 },
					{ "permission_runtime_grant", 2 },
				}
			}
		};

		return Convert.ToBase64String(Encoding.UTF8.GetBytes(mapMd.ToString(Newtonsoft.Json.Formatting.None)));
	}

	private static string create_ios_map_md_cookie()
	{
		var mapMd = new JObject
		{
			{ "device_user_dictionary", new JArray() },
			{ "device_registration_data", new JObject { { "software_version", "35602678" } } },
			{ "app_identifier", new JObject
				{
					{ "app_version", "3.56.2" },
					{ "bundle_id", "com.audible.iphone" }
				}
			}
		};

		return Convert.ToBase64String(Encoding.UTF8.GetBytes(mapMd.ToString(Newtonsoft.Json.Formatting.None))).TrimEnd('=');
	}

	private static string create_ios_frc_cookie()
	{
		Span<byte> token = stackalloc byte[313];
		Random.Shared.NextBytes(token);
		return Convert.ToBase64String(token.ToArray()).TrimEnd('=');
	}

	private string create_android_frc_cookie(Locale locale, string deviceSn)
	{
		var deviceInfo = new JObject
		{
			{ "ApplicationName", Profile.AppName },
			{ "ApplicationVersion", Profile.FrcApplicationVersion },
			{ "DeviceOSVersion", Profile.OsVersion },
			{ "DeviceName", Profile.FrcDeviceName },
			{ "ScreenWidthPixels", Profile.ScreenWidthPixels },
			{ "ThirdPartyDeviceId", deviceSn },
			{ "FirstPartyDeviceId", deviceSn },
			{ "ScreenHeightPixels", Profile.ScreenHeightPixels },
			{ "DeviceLanguage", locale.Language },
			{ "TimeZone", format_offset(DateTimeOffset.Now.Offset) },
			{ "Carrier", "T-Mobile" },
		};

		if (Profile.IncludeLocalIpInFrc)
		{
			IPAddress? ip;
			try
			{
				ip = NetworkInterface.GetAllNetworkInterfaces().Select(i => i.GetIPProperties()).SelectMany(GetAllIpAddresses)
					.OrderBy(a => a.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
					.OrderByDescending(a => !a.IsIPv6LinkLocal && !a.IsIPv6SiteLocal && !a.IsIPv6UniqueLocal).FirstOrDefault();
			}
			catch
			{
				ip = IPAddress.IPv6Any;
			}

			deviceInfo["IpAddress"] = ip?.ToString() ?? "0.0.0.0";
		}

		return FrcEncoder.Encode(deviceSn, deviceInfo.ToString(Newtonsoft.Json.Formatting.None));

		IEnumerable<IPAddress> GetAllIpAddresses(IPInterfaceProperties iPInterfaceProperties)
			=> iPInterfaceProperties.DnsAddresses.Select(a => a)
			.Concat(iPInterfaceProperties.GatewayAddresses.Select(a => a.Address))
			.Concat(iPInterfaceProperties.UnicastAddresses.Select(a => a.Address));
	}

	private static string format_offset(TimeSpan ts)
		=> (ts.Ticks < 0 ? "-" : "") + $"{ts:hh\\:mm}";
}
