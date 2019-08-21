using System;
using AudibleApi.Authorization;
using Newtonsoft.Json.Linq;
using static TestAudibleApiCommon.ComputedTestValues;

namespace AuthorizationShared
{
    public static class Shared
	{
		// use "=>" instead of "{ get; } = " because static class can
		// have weird field/property init order

		public static string PrivateKeyValueNewLines
			=> PrivateKeyValue.Replace("\\n", "\n");

		public static string AccessTokenExpires_Expired
			=> "1999-01-01T01:02:33.6204337-04:00";
		public static DateTime AccessTokenExpires_Expired_Parsed
			=> DateTime.Parse(AccessTokenExpires_Expired);

		public static string AccessTokenExpires_Future
			=> "2200-01-01T01:02:33.6204337-04:00";
		public static DateTime AccessTokenExpires_Future_Parsed
			=> DateTime.Parse(AccessTokenExpires_Future);

		public static string IdentityJson_Expired
			=> new JObject {
				{
					"ExistingAccessToken", new JObject {
						{ "TokenValue", AccessTokenValue },
						{ "Expires", AccessTokenExpires_Expired }
					}
				},
				{
					"PrivateKey", new JObject {
						{ "Value", PrivateKeyValueNewLines }
					}
				},
				{
					"AdpToken", new JObject {
						{ "Value", AdpTokenValue }
					}
				},
				{
					"RefreshToken", new JObject{
						{ "Value", RefreshTokenValue }
					}
				},
				{
					"Cookies", new JArray {
						new JObject {
							{ "Key", "key1" },
							{ "Value", "value 1" }
						},
						new JObject {
							{ "Key", "key1" },
							{ "Value", "value 2" }
						}
					}
				}
			}.ToString().Replace("\\n", "\n");

		public static string IdentityJson_Future
			=> new JObject {
				{
					"ExistingAccessToken", new JObject {
						{ "TokenValue", AccessTokenValue },
						{ "Expires", AccessTokenExpires_Future }
					}
				},
				{
					"PrivateKey", new JObject {
						{ "Value", PrivateKeyValueNewLines }
					}
				},
				{
					"AdpToken", new JObject {
						{ "Value", AdpTokenValue }
					}
				},
				{
					"RefreshToken", new JObject{
						{ "Value", RefreshTokenValue }
					}
				},
				{
					"Cookies", new JArray {
						new JObject {
							{ "Key", "key1" },
							{ "Value", "value 1" }
						},
						new JObject {
							{ "Key", "key1" },
							{ "Value", "value 2" }
						}
					}
				}
			}.ToString().Replace("\\n", "\n");

		public static Identity GetIdentity_Expired()
			=> Identity.FromJson(IdentityJson_Expired);

		public static Identity GetIdentity_Future()
			=> Identity.FromJson(IdentityJson_Future);

		public static string RefreshTokenResponse
            => new JObject
            {
                { "access_token", AccessTokenValue },
                { "token_type", "bearer" },
                { "expires_in", 3600 }
            }
            .ToString();
    }
}
