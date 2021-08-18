using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using L1.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TestAudibleApiCommon
{
	/// <summary>
	/// ComputedTestValues holds
	/// - big computed strings
	/// - benign personal identifying information
	/// - somewhat secret account info
	/// - may also later be used to contain actually secret info
	/// </summary>
	public static class ComputedTestValues
	{
		private static void ensureDir()
			=> Directory.CreateDirectory("ComputedTestValues");

		private static Dictionary<string, Func<string>> TestValuesDictionary { get; } = new Dictionary<string, Func<string>>
		{
			[nameof(AuthenticateResponse)] = getNewAuthenticateResponse,
			[nameof(AccessTokenValue)] = getNewAccessTokenValue,
			[nameof(RefreshTokenValue)] = getNewRefreshTokenValue,
			[nameof(AdpTokenValue)] = getNewAdpTokenValue,
			[nameof(PrivateKeyValue)] = getNewPrivateKeyValue,
			[nameof(LibraryFull)] = getNewLibraryFull,
			[nameof(LibraryWithOptions)] = getNewLibraryWithOptions,
			[nameof(LibraryBookWithResponseGroups)] = getNewLibraryBookWithResponseGroups,
			[nameof(CredentialsPageMetadataPlaintext)] = getNewCredentialsPageMetadataPlaintext,
			[nameof(CredentialsPageMetadataEncrypted)] = getNewCredentialsPageMetadataEncrypted,
			[nameof(Encryption_Get)] = getNewEncryption_Get,
			[nameof(Encryption_Post)] = getNewEncryption_Post,
			[nameof(Encryption_Delete)] = getNewEncryption_Delete
		};

		private static string getValue(string propertyName)
		{
			var filename = $@"ComputedTestValues\{propertyName}.json";

			if (!File.Exists(filename))
			{
				var fn = TestValuesDictionary[propertyName];
				var json = fn();

				Directory.CreateDirectory("ComputedTestValues");
				File.WriteAllText(filename, json);
			}
			var contents = File.ReadAllText(filename);
			return contents;
		}

		public static string AuthenticateResponse => getValue(nameof(AuthenticateResponse));
		private static string getNewAuthenticateResponse()
		{
			var str = new Authorize_L1
				.AuthorizeTests()
				.GetRegisterStringAsync()
				.GetAwaiter()
				.GetResult();
			var jObj = JObject.Parse(str);
			if (jObj.ContainsKey("response") && ((JObject)jObj["response"]).ContainsKey("error"))
				throw new Exception($"invalid {nameof(getNewAuthenticateResponse)}:\r\n" + str);
			return str;
		}

		public static string AccessTokenValue => getValue(nameof(AccessTokenValue));
		private static string getNewAccessTokenValue()
			=> getPopulatedIdentity().ExistingAccessToken.TokenValue;

		public static string RefreshTokenValue => getValue(nameof(RefreshTokenValue));
		private static string getNewRefreshTokenValue()
			=> getPopulatedIdentity().RefreshToken.Value;

		public static string AdpTokenValue => getValue(nameof(AdpTokenValue));
		private static string getNewAdpTokenValue()
			=> getPopulatedIdentity().AdpToken.Value;

		public static string PrivateKeyValue => getValue(nameof(PrivateKeyValue));
		private static string getNewPrivateKeyValue()
			=> getPopulatedIdentity().PrivateKey.Value;

		private static IIdentity getPopulatedIdentity()
		{
			// start by loading real identity to avoid infinite loop
			var identity = REAL.GetIdentity();

			var authResponse = AuthenticateResponse;
			var sysDateTime = StaticSystemDateTime.Future;
			RegistrationParser.ParseRegistrationIntoIdentity(JObject.Parse(authResponse), identity, sysDateTime);
			return identity;
		}

		public static string LibraryFull => getValue(nameof(LibraryFull));
		private static string getNewLibraryFull()
			=> new ApiTests_L1.Inherited.GetLibraryAsync()
			.GetResponseAsync()
			.GetAwaiter()
			.GetResult();

		/// <summary>
		/// NumberOfResultPerPage = 20
		/// PageNumber = 5
		/// PurchasedAfter = new DateTime(1970, 1, 1)
		/// ResponseGroups = Sku | Reviews
		/// SortBy = TitleDesc
		/// </summary>
		public static string LibraryWithOptions => getValue(nameof(LibraryWithOptions));
		private static string getNewLibraryWithOptions()
			=> new ApiTests_L1.Inherited.GetLibraryAsync_libraryOptions()
			.GetResponseAsync()
			.GetAwaiter()
			.GetResult();

		/// <summary>ResponseGroups = Relationships</summary>
		public static string LibraryBookWithResponseGroups => getValue(nameof(LibraryBookWithResponseGroups));
		private static string getNewLibraryBookWithResponseGroups()
			=> new ApiTests_L1.Inherited.GetLibraryBookAsync_responseGroups()
			.GetResponseAsync()
			.GetAwaiter()
			.GetResult();

		public static string UserProfileValue => getValue(nameof(UserProfileValue));

		public static string CredentialsPageMetadataPlaintext => getValue(nameof(CredentialsPageMetadataPlaintext));
		private static string getNewCredentialsPageMetadataPlaintext()
			=> CredentialsPage.GenerateMetadata(Locales.Us, 123456789L);

		public static string CredentialsPageMetadataEncrypted => getValue(nameof(CredentialsPageMetadataEncrypted));
		private static string getNewCredentialsPageMetadataEncrypted()
			=> Cryptography.EncryptMetadata(CredentialsPageMetadataPlaintext);

		public static string Encryption_Get => getValue(nameof(Encryption_Get));
		private static string getNewEncryption_Get()
			=> new CryptographyTests.SignRequest()
			.CreateGetSignature();

		public static string Encryption_Post => getValue(nameof(Encryption_Post));
		private static string getNewEncryption_Post()
			=> new CryptographyTests.SignRequest()
			.CreatePostSignature();

		public static string Encryption_Delete => getValue(nameof(Encryption_Delete));
		private static string getNewEncryption_Delete()
			=> new CryptographyTests.SignRequest()
			.CreateDeleteSignature();
	}
}
