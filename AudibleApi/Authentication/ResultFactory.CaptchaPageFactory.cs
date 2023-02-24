using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    public abstract partial class ResultFactory
    {
        private class CaptchaPageFactory : ResultFactory
        {
            public CaptchaPageFactory() : base(nameof(CaptchaPageFactory)) { }

            protected override bool _isMatchAsync(HttpResponseMessage response, string body)
            {
                var newInputs = HtmlHelper.GetInputs(body);
                return
                    newInputs.ContainsKey("email") &&
                    newInputs.ContainsKey("password") &&
                    newInputs.ContainsKey("use_image_captcha");
            }

            protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
            {
                if (!oldInputs.ContainsKey("password"))
                    throw new ArgumentException("Provided inputs do not contain a password", nameof(oldInputs));
                var email = oldInputs["email"];
                var password = oldInputs["password"];

                var captchaUri = getCaptchaUri(body);

                return new CaptchaPage(authenticate, body, captchaUri, email, password);
            }

            private static Uri getCaptchaUri(string body)
            {
                var sourceUrl
                    = HtmlHelper.GetElements(body, "img", "id", "auth-captcha-image")
                    ?.SingleOrDefault()
                    ?.Attributes["src"]
                    ?.Value;
                if (sourceUrl is null)
                {
                    var errorMsg = "CAPTCHA image cannot be retrieved";
                    Serilog.Log.Logger.Error(errorMsg);
                    throw new Exception(errorMsg);
                }

                var captchaUrl = System.Web.HttpUtility.HtmlDecode(sourceUrl);
                return new Uri(captchaUrl);
            }
        }
    }
}
