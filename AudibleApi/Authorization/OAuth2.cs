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
		return new JObject
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
					{ "domain", locale.AudibleLoginUri() },
					{ "website_cookies", new JArray() }
				}
			},
			{ "registration_data", new JObject
				{
					{ "domain", "DeviceLegacy" },
					{ "device_type", Resources.DeviceType },
					{ "device_serial", RegistrationOptions?.DeviceSerialNumber },
					{ "app_name",  Resources.AppName },
					{ "app_version", Resources.AppVersion },
					{ "device_model",  Resources.DeviceModel },
					{ "os_version",  Resources.OsVersion },
					{ "software_version",  Resources.SoftwareVersion },
					{ "device_name",  $"%FIRST_NAME%%FIRST_NAME_POSSESSIVE_STRING%%DUPE_STRATEGY_1ST%{RegistrationOptions?.DeviceName}" },
				}
			},
			{ "device_metadata", new JObject
				{
					{ "device_os_family", Resources.OsFamily },
					{ "device_type", Resources.DeviceType },
					{ "device_serial", RegistrationOptions?.DeviceSerialNumber },
					{ "manufacturer",  Resources.Manufacturer },
					{ "model", Resources.DeviceModel },
					{ "os_version", Resources.OsVersionNumber },
					{ "product", Resources.DeviceProduct },
				}
			},
			{ "auth_data", new JObject
				{
					{ "use_global_authentication", "true" },
					{ "authorization_code", Code },
					{ "code_verifier", RegistrationOptions?.CodeVerifier },
					{ "code_algorithm", "SHA-256" },
					{ "client_domain", "DeviceLegacy" },
					{ "client_id", RegistrationOptions?.ClientID },
				}
			},
			{ "requested_extensions", new JArray
				{
					"device_info",
					"customer_info"
				}
			}
		};
	}
}
