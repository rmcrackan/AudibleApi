using System;
using AudibleApi.Authorization;
using Newtonsoft.Json.Linq;
using static AuthorizationShared.Shared.AccessTokenTemporality;
using static TestAudibleApiCommon.ComputedTestValues;

namespace AuthorizationShared
{
    public static class Shared
	{
		// use "=>" instead of "{ get; } = " because static class can
		// have weird field/property init order

		public static string PrivateKeyValueNewLines
			=> PrivateKeyValue.Replace("\\n", "\n");

		public static string RefreshTokenResponse
			=> new JObject
			{
				{ "access_token", AccessTokenValue },
				{ "token_type", "bearer" },
				{ "expires_in", 3600 }
			}
			.ToString();

		public enum AccessTokenTemporality { Future, Expired }

		public static string GetAccessTokenExpires(AccessTokenTemporality time) => time switch
		{
			Expired => "1999-01-01T01:02:33.6204337-04:00",
			Future => "2200-01-01T01:02:33.6204337-04:00"
		};

		public static DateTime GetAccessTokenExpires_Parsed(AccessTokenTemporality time)
			=> DateTime.Parse(GetAccessTokenExpires(time));

		public static string GetIdentityJson(AccessTokenTemporality time)
			=> new JObject {
				{
					"ExistingAccessToken", new JObject {
						{ "TokenValue", AccessTokenValue },
						{ "Expires", GetAccessTokenExpires(time) }
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

		public static Identity GetIdentity(AccessTokenTemporality time)
			=> Identity.FromJson(GetIdentityJson(time));
    }
}
