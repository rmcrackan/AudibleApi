using System;
using AudibleApi;
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

		private static JObject getIdentityJObject(AccessTokenTemporality time, Locale locale = null)
			=> new JObject {
				{
					"LocaleName", locale?.Name ?? "us"
				},
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
			};

		public static Identity GetIdentity(AccessTokenTemporality time, Locale locale = null)
			=> Identity.FromJson(GetIdentityJson(time, locale));

		public static string GetIdentityJson(AccessTokenTemporality time, Locale locale = null)
			=> getIdentityJObject(time, locale).ToString().Replace("\\n", "\n");

		public static string GetNestedIdentityJson(AccessTokenTemporality time)
			=> new JObject {
				{
					"AccountsSettings", new JArray {
						new JObject {
							{ "Username", "Uno" },
							{ "DecryptKey", "11111111" },
							// TODO: give Locale: us
							{ "IdentityTokens", getIdentityJObject(time) }
						},
						//// TODO: new object. duplicate username. unique username+locale
						//new JObject {
						//	{ "Username", "Uno" },
						//	{ "DecryptKey", "11111111" },
						//	// TODO: give Locale: uk
						//	{ "IdentityTokens", GetIdentityJson(time) }
						//},
						new JObject {
							{ "Username", "Dos" },
							{ "DecryptKey", "22222222" },
							// TODO: give Locale: us
							{ "IdentityTokens", getIdentityJObject(time) }
						}
					}
				}
			}.ToString().Replace("\\n", "\n");
		public static string JsonPathMatch =>
			//TODO: WITH LOCALE// "$.AccountsSettings[?(@.Username == 'Uno' && @.IdentityTokens.Locale == 'uk')].IdentityTokens"
			"$.AccountsSettings[?(@.Username == 'Uno')].IdentityTokens"
			;
		public static string JsonPathNonMatch =>
			//TODO: WITH LOCALE// "$.AccountsSettings[?(@.Username == 'Juan' && @.IdentityTokens.Locale == 'uk')].IdentityTokens"
			"$.AccountsSettings[?(@.Username == 'Juan')].IdentityTokens"
			;
	}
}
