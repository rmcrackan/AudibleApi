using System;

namespace AudibleApi
{
	/// <summary>If not already logged in, log in with API</summary>
	public interface ILoginCallback
	{
		(string email, string password) GetLogin();
		string GetCaptchaAnswer(byte[] captchaImage);
		(string name, string value) GetMfaChoice(MfaConfig mfaConfig);
		string Get2faCode();
		void ShowApprovalNeeded();
	}
}
