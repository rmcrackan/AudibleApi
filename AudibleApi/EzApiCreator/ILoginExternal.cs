namespace AudibleApi;

/// <summary>If not already logged in, log in with an external browser</summary>
public interface ILoginExternal
{
	/// <param name="loginUrl">Initial sign-in page to begin login</param>
	/// <param name="signInCookies">Cookies to be sent with the initial sign-in request</param>
	/// <returns>URL or response page after login is successful</returns>
	string GetResponseUrl(string loginUrl, System.Net.CookieCollection signInCookies);

	string DeviceName { get; }
}
