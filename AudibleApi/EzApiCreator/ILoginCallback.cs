using System.Threading.Tasks;

namespace AudibleApi;

/// <summary>If not already logged in, log in with API</summary>
public interface ILoginCallback
{
	Task<(string email, string password)> GetLoginAsync();
	Task<(string password, string guess)> GetCaptchaAnswerAsync(string password, byte[] captchaImage);
	Task<(string name, string value)> GetMfaChoiceAsync(MfaConfig mfaConfig);
	Task<string> Get2faCodeAsync(string prompt);
	Task ShowApprovalNeededAsync();
	string DeviceName { get; }
}
