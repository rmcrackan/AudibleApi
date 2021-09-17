using System;

namespace AudibleApi
{
	public enum LoginMethod { Api, External }

	/// <summary>If not already logged in, user selects whether to log in with API or an external browser</summary>
	public interface ILoginChoice
	{
		LoginMethod GetLoginMethod();

		ILoginCallback loginCallback { get; }
		ILoginExternal loginExternal { get; }
	}
}
