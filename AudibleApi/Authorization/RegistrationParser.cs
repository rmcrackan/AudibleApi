using System;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
	public static class RegistrationParser
	{
		public static void ParseRegistrationIntoIdentity(JObject authRegister, IIdentity identity, ISystemDateTime systemDateTime)
		{
			if (authRegister is null)
				throw new ArgumentNullException(nameof(authRegister));
			if (identity is null)
				throw new ArgumentNullException(nameof(identity));
			if (systemDateTime is null)
				throw new ArgumentNullException(nameof(systemDateTime));

			var tokens = authRegister["response"]["success"]["tokens"];

			var privateKey = tokens["mac_dms"]["device_private_key"].ToString();
			var adpToken = tokens["mac_dms"]["adp_token"].ToString();
			var accessToken = tokens["bearer"]["access_token"].ToString();
			var secondsUntilExpires = int.Parse(tokens["bearer"]["expires_in"].ToString());
			var refreshToken = tokens["bearer"]["refresh_token"].ToString();

			var expiresDateTime = systemDateTime.UtcNow.AddSeconds(secondsUntilExpires);

			identity.Update(
				new PrivateKey(privateKey),
				new AdpToken(adpToken),
				new AccessToken(accessToken, expiresDateTime),
				new RefreshToken(refreshToken)
				);
		}
	}
}
