using System;
using System.Threading.Tasks;

namespace AudibleApi.Authentication
{
    public class CaptchaPage : LoginResult
    {
        public Uri CaptchaImage { get; }

        public CaptchaPage(Authenticate authenticate, string responseBody, Uri img, string email, string password) : base(authenticate, responseBody)
        {
            if (img is null)
                throw new ArgumentNullException(nameof(img));

            if (password is null)
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password may not be blank", nameof(password));

			Inputs["email"] = email;
			Inputs["password"] = password;

            CaptchaImage = img;
        }

        public async Task<LoginResult> SubmitAsync(string guess)
        {
            if (guess is null)
                throw new ArgumentNullException(nameof(guess));
            if (string.IsNullOrWhiteSpace(guess))
                throw new ArgumentException("Guess may not be blank", nameof(guess));

            Inputs["guess"] = guess.Trim().ToLower();
            Inputs["rememberMe"] = "true";
            Inputs["use_image_captcha"] = "true";
            Inputs["use_audio_captcha"] = "false";
            Inputs["showPasswordChecked"] = "false";

            return await LoginResultRunner.GetResultsPageAsync(Authenticate, Inputs);
        }
    }
}
