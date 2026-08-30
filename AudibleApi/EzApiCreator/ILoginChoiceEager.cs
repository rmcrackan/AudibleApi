using System.Threading.Tasks;

namespace AudibleApi;

/// <param name="LoginUrl">Initial sign-in page to begin login</param>
/// <param name="SignInCookies">Cookies to be sent with the initial sign-in request</param>
public record ChoiceIn(string LoginUrl, System.Net.CookieCollection SignInCookies);

/// <summary>If not already logged in, user can log in with API or an external browser. External browser url is provided. Response can be external browser login or continuing with native api callbacks.</summary>
public interface ILoginChoiceEager
{
	Task<string?> StartAsync(ChoiceIn choiceIn);
}
