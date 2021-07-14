using System;
using System.IO;
using AudibleApi;

namespace AudibleApiClientExample
{
	public class LoginCallback : ILoginCallback
	{
		public string Get2faCode()
		{
			Console.WriteLine("Two-Step Verification code:");
			var _2faCode = Console.ReadLine();
			return _2faCode;
		}

		public string GetCaptchaAnswer(byte[] captchaImage)
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
				return guess;
			}
			finally
			{
				if (File.Exists(tempFileName))
					File.Delete(tempFileName);
			}
		}

		public (string email, string password) GetLogin()
		{
			if (File.Exists(_Main.loginFilePath))
			{
				var pwParts = File.ReadAllLines(_Main.loginFilePath);
				var email = pwParts[0];
				var password = pwParts[1];

				if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
					return (email, password);
			}

			Console.WriteLine("Email:");
			var e = Console.ReadLine().Trim();
			Console.WriteLine("Password:");
			var pw = Dinah.Core.ConsoleLib.ConsoleExt.ReadPassword();
			return (e, pw);
		}

		public (string name, string value) GetMfaChoice(MfaConfig mfaConfig) => throw new NotImplementedException();

		public void ShowApprovalNeeded() => throw new NotImplementedException();
	}
}
