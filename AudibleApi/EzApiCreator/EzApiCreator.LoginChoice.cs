using System;
using System.Threading.Tasks;

namespace AudibleApi
{
	public static partial class EzApiCreator
	{
		/// <summary>Create a new Audible Api object. If not already logged in, user selects whether to log in with API or an external browser</summary>
		/// <param name="locale">Audible region/locale to connect to</param>
		/// <param name="identityFilePath">Load from and save to the file at this path</param>
		/// <param name="loginChoice">Object with callback method for allowing user to choose external or API login</param>
		/// <param name="jsonPath">Optional JSONPath for location of identity tokens inside identity file</param>
		/// <returns>Object which enables calls to the Audible API</returns>
		public static async Task<Api> GetApiAsync(ILoginChoice loginChoice, Locale locale, string identityFilePath, string jsonPath = null)
		{
			StackBlocker.ApiTestBlocker();

			try
			{
				return await GetApiAsync(locale, identityFilePath, jsonPath);
			}
			catch (Exception debugEx) // TODO: exceptions should not be used for control flow. fix this
			{
				Dinah.Core.ArgumentValidator.EnsureNotNull(loginChoice, nameof(loginChoice));

				var choice = loginChoice.GetLoginMethod();
				return choice switch
				{
					LoginMethod.Api => await GetApiAsync(loginChoice.LoginCallback, locale, identityFilePath, jsonPath),
					LoginMethod.External => await GetApiAsync(loginChoice.LoginExternal, locale, identityFilePath, jsonPath),
					_ => throw new Exception($"Unknown {nameof(LoginMethod)} value")
				};
			}
		}
	}
}
