using System;
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
		/// <param name="locale">Audible region/locale to connect to</param>
		/// <param name="identityFilePath">Load from and save to the file at this path</param>
		/// <param name="loginCallback">Object with callback methods allowing for initial login</param>
		/// <param name="jsonPath">Optional JSONPath for location of identity tokens inside identity file</param>
		/// <returns>Object which enables calls to the Audible API</returns>
		public static async Task<Api> GetApiAsync(Locale locale, string identityFilePath, string jsonPath = null, ILoginCallback loginCallback = null)
		{
			StackBlocker.ApiTestBlocker();

			IdentityPersister identityPersister;
			try
			{
				identityPersister = new IdentityPersister(identityFilePath, jsonPath);
			}
			catch (Exception debugEx) // TODO: exceptions should not be used for control flow. fix this
			{
				var inMemoryIdentity = await loginAsync(locale, loginCallback);
				identityPersister = new IdentityPersister(inMemoryIdentity, identityFilePath, jsonPath);
			}

			var api = await createApiAsync(identityPersister.Identity);
			return api;
		}

		// LOGIN PATTERN
		// - Start with Authenticate. Submit email + pw
		// - Each step in the login process will return a LoginResult
		// - Each result which has required user input has a SubmitAsync method
		// - The final LoginComplete result returns "Identity" -- in-memory authorization items
		private static async Task<Identity> loginAsync(Locale locale, ILoginCallback responder)
		{
			Dinah.Core.ArgumentValidator.EnsureNotNull(responder, nameof(responder));

			var (email, password) = responder.GetLogin();

			var login = new Authenticate(locale);
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

		private static async Task<Api> createApiAsync(this IIdentity identity)
		{
			var identityMaintainer = await IdentityMaintainer.CreateAsync(identity);
			var api = new Api(identityMaintainer);
			return api;
		}
	}
}
