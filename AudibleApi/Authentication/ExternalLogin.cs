using AudibleApi.Authorization;
using Dinah.Core;
using System.Net;

namespace AudibleApi.Authentication;

public class ExternalLogin
{
	private Locale _locale { get; }
	private RegistrationOptions RegistrationOptions { get; }

	public ExternalLogin(Locale locale, string deviceName)
	{
		_locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));
		RegistrationOptions = new RegistrationOptions(deviceName);
	}

	/// <summary>
	/// Gives the url to login with external browser and prompt for result.
	/// Builds the url to login to Amazon as an Audible device.
	/// </summary>
	public string GetLoginUrl() => RegistrationOptions.OAuthUrl(_locale).ToString();

	/// <summary>
	/// Gives initial cookies to be used at /ap/Signin page.
	/// </summary>
	public CookieCollection GetSignInCookies() => RegistrationOptions.GetSignInCookies(_locale);

	/// <summary>Retrieve tokens from response URL. Return an in-memory Identity object</summary>
	public Identity Login(string responseUrl)
	{
		var oauth2 = OAuth2.Parse(responseUrl) ?? throw new ApiErrorException(responseUrl, null, "Invalid response URL");
		return new Identity(_locale, oauth2 with { RegistrationOptions = RegistrationOptions }, []);
	}
}
