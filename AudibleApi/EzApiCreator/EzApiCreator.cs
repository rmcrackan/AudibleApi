﻿using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authentication;
using AudibleApi.Authorization;

namespace AudibleApi
{
	/// <summary>
	/// The Api class is backed by a complex set of interconnected tiny classes, each with a specific purpose. To avoid all of the complexity, use EzApiCreator. It will accept the minimum requirements and provide access to the api and will maintain auth info and keep it up to date in the file specified.
	/// </summary>
	public static class EzApiCreator
	{
		/// <summary>
		/// Create a new Audible Api object
		/// </summary>
		/// <param name="identityFilePath">Load from and save to the file at this path</param>
		/// <param name="loginCallback">Object with callback methods allowing for initial login</param>
		/// <param name="localeCountryCode">Country code for desired Audible site</param>
		/// <returns>Object which enables calls to the Audible API</returns>
		public static async Task<Api> GetApiAsync(string identityFilePath, ILoginCallback loginCallback = null, string localeCountryCode = "us")
		{
			StackBlocker.ApiTestBlocker();

			if (localeCountryCode != null)
				Localization.SetLocale(localeCountryCode);

			IdentityPersistent identityPersistent;
			try
			{
				identityPersistent = new IdentityPersistent(identityFilePath);
			}
			catch // TODO: exceptions should not be used for control flow. fix this
			{
				var inMemoryIdentity = await loginAsync(loginCallback);
				identityPersistent = new IdentityPersistent(identityFilePath, inMemoryIdentity);
			}

			var api = await createApiAsync(identityPersistent);
			return api;
		}

		// LOGIN PATTERN
		// - Start with Authenticate. Submit email + pw
		// - Each step in the login process will return a LoginResult
		// - Each result which has required user input has a SubmitAsync method
		// - The final LoginComplete result returns "Identity" -- in-memory authorization items
		private static async Task<IIdentity> loginAsync(ILoginCallback responder)
		{
			Dinah.Core.ArgumentValidator.EnsureNotNull(responder, nameof(responder));

			var (email, password) = responder.GetLogin();

			var login = new Authenticate();
			var loginResult = await login.SubmitCredentialsAsync(email, password);

			while (true)
			{
				switch (loginResult)
				{
					case CredentialsPage credentialsPage:
						var (emailInput, pwInput) = responder.GetLogin();
						loginResult = await credentialsPage.SubmitAsync(emailInput, pwInput);
						break;

					case CaptchaPage captchaResult:
						var imageBytes = await downloadImageAsync(captchaResult.CaptchaImage);
						var guess = responder.GetCaptchaAnswer(imageBytes);
						loginResult = await captchaResult.SubmitAsync(guess);
						break;

					case TwoFactorAuthenticationPage _2fa:
						var _2faCode = responder.Get2faCode();
						loginResult = await _2fa.SubmitAsync(_2faCode);
						break;

					case LoginComplete final:
						return final.Identity;

					default:
						throw new Exception("Unknown LoginResult");
				}
			}
		}

		private static async Task<byte[]> downloadImageAsync(Uri imageUri)
		{
			using var client = new HttpClient();
			using var contentStream = await client.GetStreamAsync(imageUri);
			using var localStream = new MemoryStream();
			await contentStream.CopyToAsync(localStream);
			return localStream.ToArray();
		}

		private static async Task<Api> createApiAsync(this IdentityPersistent identityPersistent)
		{
			var identityMaintainer = await IdentityMaintainer.CreateAsync(identityPersistent);
			var api = new Api(identityMaintainer);
			return api;
		}
	}
}