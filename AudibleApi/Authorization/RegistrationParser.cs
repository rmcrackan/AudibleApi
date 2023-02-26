using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
	public static class RegistrationParser
	{
		public static void ParseRegistrationIntoIdentity(JObject authRegister, IIdentity identity, ISystemDateTime systemDateTime)
		{
			ArgumentValidator.EnsureNotNull(authRegister, nameof(authRegister));
			ArgumentValidator.EnsureNotNull(identity, nameof(identity));
			ArgumentValidator.EnsureNotNull(systemDateTime, nameof(systemDateTime));

			var authRegisterDebug = authRegister.ToString(Newtonsoft.Json.Formatting.Indented);

			var extensions = authRegister["response"]["success"]["extensions"];
			var tokens = authRegister["response"]["success"]["tokens"];

			var secondsUntilExpires = int.Parse(tokens["bearer"]["expires_in"].ToString());
			var expiresDateTime = systemDateTime.UtcNow.AddSeconds(secondsUntilExpires);

			var cookies = tokens["website_cookies"]
				?.Select(cookie => new KeyValuePair<string, string>(
					cookie["Name"].ToString(),
					cookie["Value"].ToString().Replace("\"", "")))
				.ToList();

			identity.Update(
				new PrivateKey(tokens["mac_dms"]["device_private_key"].ToString()),
				new AdpToken(tokens["mac_dms"]["adp_token"].ToString()),
				new AccessToken(tokens["bearer"]["access_token"].ToString(), expiresDateTime),
				new RefreshToken(tokens["bearer"]["refresh_token"].ToString()),
				cookies,
				deviceSerialNumber: extensions["device_info"]["device_serial_number"].ToString(),
				deviceType: extensions["device_info"]["device_type"].ToString(),
				amazonAccountId: extensions["customer_info"]["user_id"].ToString(),
				deviceName: extensions["device_info"]["device_name"].ToString(),
				storeAuthenticationCookie: tokens["store_authentication_cookie"]["cookie"].ToString()
				);
		}
	}
}
