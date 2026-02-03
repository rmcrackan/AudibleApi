using System.Threading.Tasks;

namespace AudibleApi;

/// <param name="LoginUrl">Initial sign-in page to begin login</param>
/// <param name="SignInCookies">Cookies to be sent with the initial sign-in request</param>
public record ChoiceIn(string LoginUrl, System.Net.CookieCollection SignInCookies);

public class ChoiceOut
{
	public LoginMethod LoginMethod { get; }

	public string? Username { get; }
	public string? Password { get; }
	private ChoiceOut(string username, string password)
	{
		LoginMethod = LoginMethod.Api;
		Username = username;
		Password = password;
	}
	public static ChoiceOut WithApi(string username, string password) => new(username, password);

	public string? ResponseUrl { get; }
	private ChoiceOut(string responseUrl)
	{
		LoginMethod = LoginMethod.External;
		ResponseUrl = responseUrl;
	}
	public static ChoiceOut External(string responseUrl) => new(responseUrl);
}

/// <summary>If not already logged in, user can log in with API or an external browser. External browser url is provided. Response can be external browser login or continuing with native api callbacks.</summary>
public interface ILoginChoiceEager
{
	Task<ChoiceOut> StartAsync(ChoiceIn choiceIn);

	ILoginCallback LoginCallback { get; }
}
