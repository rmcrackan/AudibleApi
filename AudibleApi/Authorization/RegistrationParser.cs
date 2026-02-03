using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace AudibleApi.Authorization;

public static class RegistrationParser
{
	public static void ParseRegistrationIntoIdentity(JObject authRegister, IIdentity identity, ISystemDateTime systemDateTime)
	{
		ArgumentValidator.EnsureNotNull(authRegister, nameof(authRegister));
		ArgumentValidator.EnsureNotNull(identity, nameof(identity));
		ArgumentValidator.EnsureNotNull(systemDateTime, nameof(systemDateTime));

		var authRegisterDebug = authRegister.ToString(Newtonsoft.Json.Formatting.Indented);
		var success = (authRegister["response"]?["success"]) ?? throw new RegistrationException("Registration response does not contain success object.");
		var extensions = success?["extensions"] ?? throw new RegistrationException("Registration response does not contain extensions object.");
		var tokens = success?["tokens"] ?? throw new RegistrationException("Registration response does not contain tokens object.");
		var bearer = tokens["bearer"] ?? throw new RegistrationException("Registration response does not contain bearer token.");
		var mac_dms = tokens["mac_dms"] ?? throw new RegistrationException("Registration response does not contain mac_dms token.");
		var expires_in = bearer["expires_in"] ?? throw new RegistrationException("Registration response does not contain expires_in field in bearer token.");
		var access_token = bearer["access_token"] ?? throw new RegistrationException("Registration response does not contain access_token field in bearer token.");
		var refresh_token = bearer["refresh_token"] ?? throw new RegistrationException("Registration response does not contain refresh_token field in bearer token.");
		var device_private_key = mac_dms["device_private_key"] ?? throw new RegistrationException("Registration response does not contain device_private_key field in mac_dms token.");
		var adp_token = mac_dms["adp_token"] ?? throw new RegistrationException("Registration response does not contain adp_token field in mac_dms token.");
		var device_info = extensions["device_info"] ?? throw new RegistrationException("Registration response does not contain device_info field in extensions.");
		var customer_info = extensions["customer_info"] ?? throw new RegistrationException("Registration response does not contain customer_info field in extensions.");
		var user_id = customer_info["user_id"] ?? throw new RegistrationException("Registration response does not contain user_id field in customer_info.");
		var device_type = device_info["device_type"] ?? throw new RegistrationException("Registration response does not contain device_type field in device_info.");
		var device_name = device_info["device_name"] ?? throw new RegistrationException("Registration response does not contain device_name field in device_info.");
		var device_serial_number = device_info["device_serial_number"] ?? throw new RegistrationException("Registration response does not contain device_serial_number field in device_info.");
		var store_authentication_cookie = tokens["store_authentication_cookie"]?["cookie"] ?? throw new RegistrationException("Registration response does not contain store_authentication_cookie field in tokens.");

		var secondsUntilExpires = int.Parse(expires_in.ToString());
		var expiresDateTime = systemDateTime.UtcNow.AddSeconds(secondsUntilExpires);

		var cookies = tokens["website_cookies"]
			?.Select(cookie => new KeyValuePair<string?, string?>(
				cookie["Name"]?.ToString(),
				cookie["Value"]?.ToString().Replace("\"", "")))
			.OfType<KeyValuePair<string, string?>>()
			.ToList();

		identity.Update(
			new PrivateKey(device_private_key.ToString()),
			new AdpToken(adp_token.ToString()),
			new AccessToken(access_token.ToString(), expiresDateTime),
			new RefreshToken(refresh_token.ToString()),
			cookies,
			deviceSerialNumber: device_serial_number.ToString(),
			deviceType: device_type.ToString(),
			amazonAccountId: user_id.ToString(),
			deviceName: device_name.ToString(),
			storeAuthenticationCookie: store_authentication_cookie.ToString()
			);
	}
}
