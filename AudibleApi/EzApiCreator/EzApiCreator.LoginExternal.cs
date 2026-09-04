using AudibleApi.Authorization;
using System;
using System.Threading.Tasks;

namespace AudibleApi;

public static partial class EzApiCreator
{
	/// <summary>Create a new Audible Api object. If not already logged in, log in with an external browser</summary>
	/// <param name="locale">Audible region/locale to connect to</param>
	/// <param name="identityFilePath">Load from and save to the file at this path</param>
	/// <param name="loginExternal">Object with callback method for allowing external login</param>
	/// <param name="jsonPath">Optional JSONPath for location of identity tokens inside identity file</param>
	/// <param name="registrationProfile">Optional device-registration recipe. Null uses <see cref="DeviceRegistrationProfile.Default"/>.</param>
	/// <returns>Object which enables calls to the Audible API</returns>
	public static async Task<Api> GetApiAsync(ILoginExternal loginExternal, Locale locale, string identityFilePath, string? jsonPath = null, DeviceRegistrationProfile? registrationProfile = null)
	{
		StackBlocker.ApiTestBlocker();

		try
		{
			return await GetApiAsync(locale, identityFilePath, jsonPath);
		}
		catch (Exception debugEx) // TODO: exceptions should not be used for control flow. fix this
		{
			var inMemoryIdentity = externalLogin(locale, loginExternal, registrationProfile);
			return await createApiAsync(inMemoryIdentity, identityFilePath, jsonPath);
		}
	}

	private static Identity externalLogin(Locale locale, ILoginExternal loginExternal, DeviceRegistrationProfile? registrationProfile)
	{
		Dinah.Core.ArgumentValidator.EnsureNotNull(locale, nameof(locale));
		Dinah.Core.ArgumentValidator.EnsureNotNull(loginExternal, nameof(loginExternal));

		var externalLogin = new ExternalLogin(locale, registrationProfile);

		var loginUrl = externalLogin.GetLoginUrl();
		var signInCookies = externalLogin.GetSignInCookies();
		var responseUrl = loginExternal.GetResponseUrl(loginUrl, signInCookies);
		return externalLogin.Login(responseUrl);
	}
}
