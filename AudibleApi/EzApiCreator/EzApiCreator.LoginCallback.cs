using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;

namespace AudibleApi
{
	public static partial class EzApiCreator
	{
		/// <summary>Create a new Audible Api object. If not already logged in, log in with API</summary>
		/// <param name="locale">Audible region/locale to connect to</param>
		/// <param name="identityFilePath">Load from and save to the file at this path</param>
		/// <param name="loginCallback">Object with callback methods allowing for initial login</param>
		/// <param name="jsonPath">Optional JSONPath for location of identity tokens inside identity file</param>
		/// <returns>Object which enables calls to the Audible API</returns>
		public static async Task<Api> GetApiAsync(ILoginCallback loginCallback, Locale locale, string identityFilePath, string jsonPath = null)
		{
			StackBlocker.ApiTestBlocker();

			try
			{
				return await GetApiAsync(locale, identityFilePath, jsonPath);
			}
			catch (Exception debugEx) // TODO: exceptions should not be used for control flow. fix this
			{
				var inMemoryIdentity = await loginAsync(locale, loginCallback);
				return await createApiAsync(inMemoryIdentity, identityFilePath, jsonPath);
			}
		}

		// LOGIN PATTERN
		// - Start with Authenticate. Submit email + pw
		// - Each step in the login process will return a LoginResult
		// - Each result which has required user input has a SubmitAsync method
		// - The final LoginComplete result returns "Identity" -- in-memory authorization items
		private static async Task<Identity> loginAsync(Locale locale, ILoginCallback responder)
		{
			Dinah.Core.ArgumentValidator.EnsureNotNull(locale, nameof(locale));
			Dinah.Core.ArgumentValidator.EnsureNotNull(responder, nameof(responder));

			var (email, password) = await getUserLoginAsync(responder);

			return await loginEmailPasswordAsync(locale, responder, email, password);
		}

		private static async Task<Identity> loginEmailPasswordAsync(Locale locale, ILoginCallback responder, string email, string password)
		{
			var loginResult = await Authenticate.SubmitCredentialsAsync(locale, responder.DeviceName, email, password);

			while (true)
			{
				{
					//log the response body
					var resultType = loginResult.GetType();

					Serilog.Log.Logger.Information("Login result: {@DebugInfo}", resultType);
					Serilog.Log.Logger.Information("{@logfile}",
						new
						{
							filename = $"{resultType}.html",
							System.IO.Compression.CompressionLevel.Fastest,
							filedata = System.Text.Encoding.UTF8.GetBytes(loginResult.ResponseBody.Replace(email, email.ToMask()))
						});
				}

				switch (loginResult)
				{
					case CredentialsPage credentialsPage:
						var (emailInput, pwInput) = await getUserLoginAsync(responder);
						loginResult = await credentialsPage.SubmitAsync(emailInput, pwInput);
						break;

					case CaptchaPage captchaResult:
						var imageBytes = await downloadImageAsync(captchaResult.CaptchaImage);
						(pwInput, var guess) = await responder.GetCaptchaAnswerAsync(captchaResult.Password, imageBytes);
						loginResult = await captchaResult.SubmitAsync(pwInput, guess);
						break;

					case TwoFactorAuthenticationPage _2fa:
						var _2faCode = await responder.Get2faCodeAsync(_2fa.Prompt);
						//If 2fa guess is empty, get all MFA options
						loginResult = string.IsNullOrEmpty(_2faCode)? await _2fa.GetMfaPage() : await _2fa.SubmitAsync(_2faCode);
						break;

					case ApprovalNeededPage approvalNeeded:
						await responder.ShowApprovalNeededAsync();
						loginResult = await approvalNeeded.SubmitAsync();
						break;

					case MfaSelectionPage mfaSelection:
						(var name, var value) = await responder.GetMfaChoiceAsync(mfaSelection.MfaConfig);
						loginResult = await mfaSelection.SubmitAsync(name, value);
						break;

					case LoginComplete final:
						return final.Identity;

					default:
						throw new Exception("Unknown LoginResult");
				}
			}
		}

		private static async Task<(string email, string password)> getUserLoginAsync(ILoginCallback responder)
		{
			var (email, password) = await responder.GetLoginAsync();

			if (email is null && password is null)
				// TODO: exceptions should not be used for control flow. fix this
				throw new Exception("Login attempt cancelled by user");

			return (email, password);
		}

		private static async Task<byte[]> downloadImageAsync(Uri imageUri)
		{
			using var client = new HttpClient();
			using var contentStream = await client.GetStreamAsync(imageUri);
			using var localStream = new MemoryStream();
			await contentStream.CopyToAsync(localStream);
			return localStream.ToArray();
		}
	}
}
