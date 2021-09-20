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

	/// <summary>If not already logged in, user can log in with API or an external browser. External browser url is provided. Response can be external browser login or continuing with native api callbacks.</summary>
	public interface ILoginChoiceEager
	{
		ChoiceOut Start(ChoiceIn choiceIn);

		ILoginCallback LoginCallback { get; }
	}
}
