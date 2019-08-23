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
        private class TwoFactorAuthenticationPageFactory : ResultFactory
        {
            public TwoFactorAuthenticationPageFactory() : base(nameof(TwoFactorAuthenticationPageFactory)) { }

            public override async Task<bool> IsMatchAsync(HttpResponseMessage response)
            {
                // shared validation
                if (!await base.IsMatchAsync(response))
                    return false;

                var body = await response.Content.ReadAsStringAsync();
                if (body is null)
                    return false;

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(body);

                var otpCodeNodes = doc.DocumentNode.SelectNodes(".//input[@name='otpCode']");
                return otpCodeNodes != null && otpCodeNodes.Any();
            }

            public override async Task<LoginResult> CreateResultAsync(IHttpClient client, ISystemDateTime systemDateTime, HttpResponseMessage response, Dictionary<string, string> oldInputs)
            {
                // shared validation
                await base.CreateResultAsync(client, systemDateTime, response, oldInputs);

                var body = await response.Content.ReadAsStringAsync();
                return new TwoFactorAuthenticationPage(client, systemDateTime, body);
            }
        }
    }
}
