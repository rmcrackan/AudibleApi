using System;

namespace AudibleApi
{
	public record ChoiceIn(string LoginUrl);

	public class ChoiceOut
	{
		public LoginMethod LoginMethod { get; }

		public string Username { get; }
		public string Password { get; }
		private ChoiceOut(string username, string password)
		{
			LoginMethod = LoginMethod.Api;
			Username = username;
			Password = password;
		}
		public static ChoiceOut WithApi(string username, string password) => new(username, password);

		public string ResponseUrl { get; }
		private ChoiceOut(string responseUrl)
		{
			LoginMethod = LoginMethod.External;
			ResponseUrl = responseUrl;
		}
		public static ChoiceOut External(string responseUrl) => new(responseUrl);
	}

	/// <summary>If not already logged in, user selects whether to log in with API or an external browser</summary>
	public interface ILoginChoiceEager
	{
		ChoiceOut Start(ChoiceIn choiceIn);

		ILoginCallback LoginCallback { get; }
	}
}
