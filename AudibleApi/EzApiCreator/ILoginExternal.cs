using System;

namespace AudibleApi
{
	/// <summary>If not already logged in, log in with an external browser</summary>
	public interface ILoginExternal
	{
		/// <param name="loginUrl">Page to begin login</param>
		/// <returns>URL or response page after login is successful</returns>
		string GetResponseUrl(string loginUrl);

		string DeviceName { get; }
	}
}
