using System;
using System.IO;
using System.Threading.Tasks;
using AudibleApi;

namespace AudibleApiClientExample
{
	public class LoginCallback : ILoginCallback
	{
		public Task<string> Get2faCodeAsync()
		{
			Console.WriteLine("Two-Step Verification code:");
			var _2faCode = Console.ReadLine();
			return Task.FromResult(_2faCode);
		}

		public Task<string> GetCaptchaAnswerAsync(byte[] captchaImage)
		{
			var tempFileName = Path.Combine(Path.GetTempPath(), "audible_api_captcha_" + Guid.NewGuid() + ".jpg");

			try
			{
				File.WriteAllBytes(tempFileName, captchaImage);

				var processStartInfo = new System.Diagnostics.ProcessStartInfo
				{
					Verb = string.Empty,
					UseShellExecute = true,
					CreateNoWindow = true,
					FileName = tempFileName
				};
				System.Diagnostics.Process.Start(processStartInfo);

				Console.WriteLine("CAPTCHA answer: ");
				var guess = Console.ReadLine();
				return Task.FromResult(guess);
			}
			finally
			{
				if (File.Exists(tempFileName))
					File.Delete(tempFileName);
			}
		}

		public Task<(string email, string password)> GetLoginAsync()
		{
			var secrets = Program.GetSecrets();
			if (secrets is not null)
			{
				if (!string.IsNullOrWhiteSpace(secrets.email) && !string.IsNullOrWhiteSpace(secrets.password))
					return Task.FromResult((secrets.email, secrets.password));
			}

			Console.WriteLine("Email:");
			var e = Console.ReadLine().Trim();
			Console.WriteLine("Password:");
			var pw = Dinah.Core.ConsoleLib.ConsoleExt.ReadPassword();
			return Task.FromResult((e, pw));
		}

		//
		// not all parts are implemented for demo app
		//
		public Task<(string name, string value)> GetMfaChoiceAsync(MfaConfig mfaConfig) => throw new NotImplementedException();

		public Task ShowApprovalNeededAsync() => throw new NotImplementedException();
	}
}
