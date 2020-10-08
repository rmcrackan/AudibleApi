using System;

namespace AudibleApi
{
	public interface ILoginCallback
	{
		(string email, string password) GetLogin();
		string GetCaptchaAnswer(byte[] captchaImage);
		string Get2faCode();
		void ShowApprovalNeeded();
	}
}
