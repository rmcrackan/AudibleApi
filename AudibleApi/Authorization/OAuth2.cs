using Dinah.Core;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace AudibleApi.Authorization;

public record OAuth2
{
	public static OAuth2 Empty => new(string.Empty);
	public string Code { get; }
	public RegistrationOptions? RegistrationOptions { get; set; }

	private OAuth2(string authCode) => Code = ArgumentValidator.EnsureNotNull(authCode, nameof(authCode));

	public static OAuth2? Parse(Uri uri)
		=> uri.IsAbsoluteUri
		? ParseQuery(uri?.Query)
		: Parse(uri?.OriginalString);

	public static OAuth2? Parse(string? url) => ParseQuery(url?.Split('?').Last());

	public static OAuth2? ParseQuery(string? urlQueryPortion)
	{
		if (string.IsNullOrWhiteSpace(urlQueryPortion))
			return null;

		// keys and values are already url-decoded
		var parameters = System.Web.HttpUtility.ParseQueryString(urlQueryPortion);

		const string tokenKey = "openid.oa2.authorization_code";

		return parameters.AllKeys.Contains(tokenKey) && parameters[tokenKey] is string value && !string.IsNullOrWhiteSpace(value) ? new OAuth2(value) : null;
	}

	public JObject GetRegistrationBody(Locale locale)
	{
		var profile = RegistrationOptions?.Profile ?? DeviceRegistrationProfile.Default;
		var serial = RegistrationOptions?.DeviceSerialNumber;
		JToken cookiesDomain = profile.UseIosLoginSurface
			? $".amazon.{locale.TopDomain}"
			: locale.AudibleLoginUri().ToString();

		var registrationData = new JObject
		{
			{ "domain", profile.RegistrationDataDomain },
			{ "device_type", profile.DeviceType },
			{ "device_serial", serial },
			{ "app_name",  profile.AppName },
			{ "app_version", profile.AppVersion },
			{ "device_model",  profile.DeviceModel },
			{ "os_version",  profile.OsVersion },
			{ "software_version",  profile.SoftwareVersion },
			{ "device_name",  $"%FIRST_NAME%%FIRST_NAME_POSSESSIVE_STRING%%DUPE_STRATEGY_1ST%{profile.AmazonDeviceName}" },
		};

		var authData = new JObject
		{
			{ "authorization_code", Code },
			{ "code_verifier", RegistrationOptions?.CodeVerifier },
			{ "code_algorithm", "SHA-256" },
			{ "client_domain", "DeviceLegacy" },
			{ "client_id", RegistrationOptions?.ClientID },
		};

		if (profile.UseGlobalAuthentication)
			authData["use_global_authentication"] = "true";

		var body = new JObject
		{
			{ "requested_token_type", new JArray
				{
					"bearer",
					"mac_dms",
					"store_authentication_cookie",
					"website_cookies"
				}
			},
			{ "cookies", new JObject
				{
					{ "domain", cookiesDomain },
					{ "website_cookies", new JArray() }
				}
			},
			{ "registration_data", registrationData },
			{ "auth_data", authData },
			{ "requested_extensions", new JArray
				{
					"device_info",
					"customer_info"
				}
			}
		};

		if (profile.IncludeDeviceMetadata)
		{
			body["device_metadata"] = new JObject
			{
				{ "device_os_family", profile.OsFamily },
				{ "device_type", profile.DeviceType },
				{ "device_serial", serial },
				{ "manufacturer",  profile.Manufacturer },
				{ "model", profile.DeviceModel },
				{ "os_version", profile.OsVersionNumber },
				{ "product", profile.DeviceProduct },
			};
		}

		return body;
	}
}
