using System;
using AudibleApi;
using AudibleApi.Authorization;
using Newtonsoft.Json.Linq;
using static AuthorizationShared.Shared.AccessTokenTemporality;

namespace AuthorizationShared
{
    public static class Shared
	{
		// use "=>" instead of "{ get; } = " because static class can
		// have weird field/property init order

		public enum AccessTokenTemporality { Future, Expired }

		public static string GetAccessTokenExpires(AccessTokenTemporality time) => time switch
		{
			Expired => "1999-01-01T01:02:33.6204337-04:00",
			Future => "2200-01-01T01:02:33.6204337-04:00",
			_ => throw new NotImplementedException()
		};
		public static DateTime GetAccessTokenExpires_Parsed(AccessTokenTemporality time)
			=> DateTime.Parse(GetAccessTokenExpires(time));

		public static string JsonPathMatch =>
			"$.Accounts[?(@.AccountId == 'Uno' && @.IdentityTokens.LocaleName == 'us')].IdentityTokens";
		public static string JsonPathNonMatch =>
			"$.Accounts[?(@.AccountId == 'Juan' && @.IdentityTokens.LocaleName == 'us')].IdentityTokens";
	}
}
