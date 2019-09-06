using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public class CaptchaPage : LoginResult
    {
        public Uri CaptchaImage { get; }

        public CaptchaPage(IHttpClient client, ISystemDateTime systemDateTime, string responseBody, Uri img, string password) : base(client, systemDateTime, responseBody)
        {
            if (img is null)
                throw new ArgumentNullException(nameof(img));

            if (password is null)
                throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password may not be blank", nameof(password));

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

            return await GetResultsPageAsync(Inputs);
        }
    }
}
