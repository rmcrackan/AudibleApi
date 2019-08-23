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

			public override async Task<LoginResult> CreateResultAsync(IHttpClient client, ISystemDateTime systemDateTime, HttpResponseMessage response, Dictionary<string, string> oldInputs)
			{
                // shared validation
                await base.CreateResultAsync(client, systemDateTime, response, oldInputs);

                if (!oldInputs.ContainsKey("password"))
                    throw new ArgumentException("Provided inputs do not contain a password", nameof(oldInputs));
                var password = oldInputs["password"];

                var body = await response.Content.ReadAsStringAsync();
                var captchaUri = getCaptchaUri(body);

                return new CaptchaPage(client, systemDateTime, body, captchaUri, password);
            }

            private static Uri getCaptchaUri(string body)
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(body);

                //// uncomment for testing
                //var sources = doc.DocumentNode.SelectNodes("//img").Select(i => i.GetAttributeValue("src", "")).ToList();
                var node = doc
                    .DocumentNode
                    .SelectNodes("//img[@alt='Visual CAPTCHA image, continue down for an audio option.']")
                    ?.SingleOrDefault();
                var captchaUrl = System.Web.HttpUtility.HtmlDecode(node?.Attributes["src"].Value);
                return new Uri(captchaUrl);
            }
        }
    }
}
