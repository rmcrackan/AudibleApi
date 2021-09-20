using System;
using System.Threading.Tasks;
using AudibleApi.Authentication;
using AudibleApi.Authorization;

namespace AudibleApi
{
	public static partial class EzApiCreator
	{
		/// <summary>Create a new Audible Api object. If not already logged in, user selects whether to log in with API or an external browser</summary>
		/// <param name="locale">Audible region/locale to connect to</param>
		/// <param name="identityFilePath">Load from and save to the file at this path</param>
		/// <param name="loginChoiceEager">Object with callback method for allowing user to choose external or API login</param>
		/// <param name="jsonPath">Optional JSONPath for location of identity tokens inside identity file</param>
		/// <returns>Object which enables calls to the Audible API</returns>
		public static async Task<Api> GetApiAsync(ILoginChoiceEager loginChoiceEager, Locale locale, string identityFilePath, string jsonPath = null)
		{
			StackBlocker.ApiTestBlocker();

			try
			{
				return await GetApiAsync(locale, identityFilePath, jsonPath);
			}
			catch (Exception debugEx) // TODO: exceptions should not be used for control flow. fix this
			{
				Dinah.Core.ArgumentValidator.EnsureNotNull(loginChoiceEager, nameof(loginChoiceEager));

				var inMemoryIdentity = await choiceLoginAsync(locale, loginChoiceEager);
				return await createApiAsync(inMemoryIdentity, identityFilePath, jsonPath);
			}
		}

		private static async Task<Identity> choiceLoginAsync(Locale locale, ILoginChoiceEager loginChoiceEager)
		{
			Dinah.Core.ArgumentValidator.EnsureNotNull(locale, nameof(locale));
			Dinah.Core.ArgumentValidator.EnsureNotNull(loginChoiceEager, nameof(loginChoiceEager));

			var externalLogin = new ExternalLogin(locale);
			var loginUrl = externalLogin.GetLoginUrl();

			var choiceIn = new ChoiceIn(loginUrl);
			var choiceOut = loginChoiceEager.Start(choiceIn);

			if (choiceOut is null)
			{
				// TODO: exceptions should not be used for control flow. fix this
				throw new Exception("Login attempt cancelled by user");
			}

			return choiceOut.LoginMethod switch
			{
				LoginMethod.Api => await loginEmailPasswordAsync(locale, loginChoiceEager.LoginCallback, choiceOut.Username, choiceOut.Password),
				LoginMethod.External => externalLogin.Login(choiceOut.ResponseUrl),
				_ => throw new Exception($"Unknown {nameof(LoginMethod)} value")
			};
		}
	}
}
