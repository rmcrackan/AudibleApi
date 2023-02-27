using Dinah.Core;
using System;
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
			var baseUri = locale.LoginUri();

			// this helps dramatically with debugging
			var return_to = $"{baseUri.GetOrigin()}/ap/maplanding";
			var assoc_handle = locale.WithUsername ? $"amzn_audible_ios_lap_{locale.CountryCode}" : $"amzn_audible_ios_{locale.CountryCode}";
			var page_id = locale.WithUsername ? "amzn_audible_ios_privatepool" : "amzn_audible_ios";

			var oauth_params = new Dictionary<string, string>
			{
				// these are NOT dependent on locale and do NOT use https
				{"openid.oa2.response_type", "code"},
				{"openid.oa2.code_challenge_method", "S256"},
				{"openid.oa2.code_challenge", ChallengeCode },
				{"openid.return_to", return_to},
				{ "openid.assoc_handle", assoc_handle},
				{ "openid.identity", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "pageId", page_id},
				{ "accountStatusPolicy", "P1"},
				{ "openid.claimed_id", "http://specs.openid.net/auth/2.0/identifier_select" },
				{ "openid.mode", "checkid_setup"},
				{ "openid.ns.oa2", "http://www.amazon.com/ap/ext/oauth/2"},
				{ "openid.oa2.client_id", $"device:{ClientID}"},
				{ "openid.ns.pape", "http://specs.openid.net/extensions/pape/1.0"},
				{ "marketPlaceId", locale.MarketPlaceId},
				{ "openid.oa2.scope", "device_auth_access"},
				{ "forceMobileLayout", "true"},
				{ "openid.ns", "http://specs.openid.net/auth/2.0"},
				{ "openid.pape.max_auth_age", "0"}
			};

			return new Uri(baseUri, $"/ap/signin?{urlencode(oauth_params)}");
		}

		private static string urlencode(IEnumerable<KeyValuePair<string, string>> nameValuePairs)
			=> nameValuePairs
			.Select(kvp => $"{System.Web.HttpUtility.UrlEncode(kvp.Key)}={System.Web.HttpUtility.UrlEncode(kvp.Value)}")
			.Aggregate((a, b) => $"{a}&{b}");


		//https://github.com/mkb79/Audible/blob/master/src/audible/login.py
		private static string build_device_serial() => Guid.NewGuid().ToString("N").ToUpper();

		private static string create_code_verifier()
		{
			var code_verifier = new byte[32];
			new Random().NextBytes(code_verifier);
			return urlsafe_b64encode(code_verifier);
		}

		private static string build_client_id(string deviceSerialNumber)
		{
			var client_id_bytes = Encoding.UTF8.GetBytes($"{deviceSerialNumber}#{Resources.DeviceType}");
			return BitConverter.ToString(client_id_bytes).Replace("-", "").ToLower(); ;
		}

		private static string create_s256_code_challenge(string code_verifier)
		{
			using var sha256 = SHA256.Create();

			sha256.ComputeHash(Encoding.ASCII.GetBytes(code_verifier));
			return urlsafe_b64encode(sha256.Hash);
		}

		private static string urlsafe_b64encode(byte[] bytes)
			=> Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
	}
}
