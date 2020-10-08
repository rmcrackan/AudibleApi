using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public abstract partial class ResultFactory
    {
        private class CaptchaPageFactory : ResultFactory
        {
            public CaptchaPageFactory() : base(nameof(CaptchaPageFactory)) { }

            public override async Task<bool> IsMatchAsync(HttpResponseMessage response)
            {
                // shared validation
                if (!await base.IsMatchAsync(response))
                    return false;

                var body = await response.Content.ReadAsStringAsync();
                var newInputs = HtmlHelper.GetInputs(body);
                return
                    newInputs.ContainsKey("email") &&
                    newInputs.ContainsKey("password") &&
                    newInputs.ContainsKey("use_image_captcha");
            }

			public override async Task<LoginResult> CreateResultAsync(Authenticate authenticate, HttpResponseMessage response, Dictionary<string, string> oldInputs)
			{
                // shared validation
                await base.CreateResultAsync(authenticate, response, oldInputs);

                if (!oldInputs.ContainsKey("password"))
                    throw new ArgumentException("Provided inputs do not contain a password", nameof(oldInputs));
                var password = oldInputs["password"];

                var body = await response.Content.ReadAsStringAsync();
                var captchaUri = getCaptchaUri(body);

                return new CaptchaPage(authenticate, body, captchaUri, password);
            }

            private static Uri getCaptchaUri(string body)
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(body);

                var node = doc
                    .DocumentNode
                    .SelectNodes("//img[@id='auth-captcha-image']")
                    ?.SingleOrDefault();

                var sourceUrl = node?.Attributes["src"]?.Value;

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
