namespace AudibleApi;

/// <summary>If not already logged in, user selects whether to log in with API or an external browser. After choice, continue with only that login type.</summary>
public interface ILoginChoice
{
	LoginMethod GetLoginMethod();

	ILoginCallback LoginCallback { get; }
	ILoginExternal LoginExternal { get; }
}
