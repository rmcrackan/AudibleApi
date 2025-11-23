using Dinah.Core;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace AudibleApi.Authorization
{
	public record RegistrationOptions
	{
		public string DeviceName { get; }
		public string CodeVerifier { get; }
		public string ChallengeCode { get; }
		public string DeviceSerialNumber { get; }
		public string ClientID { get; }

		public RegistrationOptions(string deviceName)
		{
			DeviceName = deviceName ?? "Libation for iPhone";
			DeviceSerialNumber = build_device_serial();
			CodeVerifier = create_code_verifier();
			ClientID = build_client_id(DeviceSerialNumber);
			ChallengeCode = create_s256_code_challenge(CodeVerifier);
		}

		public Uri OAuthUrl(Locale locale)
		{
			// this helps dramatically with debugging
			//According to static analysis of the Audible v25.38.26 apk,
			//the return_to domain is always www.audible.TLD, even for private pool accounts.
			var return_to = $"{locale.AudibleLoginUri().GetOrigin()}/ap/maplanding";
			var assoc_handle = locale.WithUsername ? $"amzn_audible_android_aui_lap_{locale.CountryCode}" : $"amzn_audible_android_aui_{locale.CountryCode}";
			var page_id = locale.WithUsername ? $"amzn_audible_android_privatepool_aui_v2_dark_{locale.CountryCode}" : $"amzn_audible_android_aui_v2_dark_us{locale.CountryCode}";

			var oauth_params = new Dictionary<string, string>
			{
				// these are NOT dependent on locale and do NOT use https
				
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
				{ "disableLoginPrepopulate", "1"},
				{ "openid.ns", "http://specs.openid.net/auth/2.0"},
			};

			return new Uri(locale.LoginUri(), $"/ap/signin?{urlencode(oauth_params)}");
		}

		private static string urlencode(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
			=> nameValuePairs
			.Select(kvp => $"{System.Web.HttpUtility.UrlEncode(kvp.Key)}={System.Web.HttpUtility.UrlEncode(kvp.Value)}")
			.Aggregate((a, b) => $"{a}&{b}");


		//https://github.com/mkb79/Audible/blob/master/src/audible/login.py
		private static string build_device_serial()
		{
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

		private static string build_client_id(string deviceSerialNumber)
		{
			var client_id_bytes = Encoding.UTF8.GetBytes($"{deviceSerialNumber}#{Resources.DeviceType}");
			return Convert.ToHexStringLower(client_id_bytes);
		}

		private static string create_s256_code_challenge(string code_verifier)
		{
			var hash = SHA256.HashData(Encoding.ASCII.GetBytes(code_verifier));
			return Base64Url.EncodeToString(hash);
		}
	}
}
