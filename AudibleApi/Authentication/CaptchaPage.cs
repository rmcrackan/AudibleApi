using Dinah.Core;
using System;
using System.Threading.Tasks;

namespace AudibleApi.Authentication
{
    internal class CaptchaPage : LoginResult
    {
        public Uri CaptchaImage { get; }
        public string Password { get; }

        public CaptchaPage(Authenticate authenticate, string responseBody, Uri img, string email, string password) : base(authenticate, responseBody)
        {
			ArgumentValidator.EnsureNotNull(img, nameof(img));
			ArgumentValidator.EnsureNotNullOrWhiteSpace(email, nameof(email));

			Inputs["email"] = email;

			Password = password;
			CaptchaImage = img;
        }

        public async Task<LoginResult> SubmitAsync(string password, string guess)
        {
            ArgumentValidator.EnsureNotNullOrWhiteSpace(password, nameof(password));
            ArgumentValidator.EnsureNotNullOrWhiteSpace(guess, nameof(guess));

			Inputs["guess"] = guess.Trim().ToLower();
			Inputs["password"] = password;
			Inputs["rememberMe"] = "true";
            Inputs["use_image_captcha"] = "true";
            Inputs["use_audio_captcha"] = "false";
            Inputs["showPasswordChecked"] = "false";

			return await LoginResultRunner.GetResultsPageAsync(Authenticate, Inputs, Method, Action);
		}
    }
}
