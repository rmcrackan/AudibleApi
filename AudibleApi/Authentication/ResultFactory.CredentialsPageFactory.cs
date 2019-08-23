using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    public abstract partial class ResultFactory
    {
        private class CredentialsPageFactory : ResultFactory
        {
            public CredentialsPageFactory() : base(nameof(CredentialsPageFactory)) { }

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
                    !newInputs.ContainsKey("use_image_captcha");
            }

			public override async Task<LoginResult> CreateResultAsync(IHttpClient client, ISystemDateTime systemDateTime, HttpResponseMessage response, Dictionary<string, string> oldInputs)
			{
                // shared validation
                await base.CreateResultAsync(client, systemDateTime, response, oldInputs);

                // do not extract email or pw from inputs. if we're here then a previous login failed
                var body = await response.Content.ReadAsStringAsync();
                return new CredentialsPage(client, systemDateTime, body);
            }
        }
    }
}
