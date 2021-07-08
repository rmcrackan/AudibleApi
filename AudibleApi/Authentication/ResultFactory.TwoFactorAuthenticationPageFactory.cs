﻿using System;
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
        private class TwoFactorAuthenticationPageFactory : ResultFactory
        {
            public TwoFactorAuthenticationPageFactory() : base(nameof(TwoFactorAuthenticationPageFactory)) { }

            protected override async Task<bool> _isMatchAsync(HttpResponseMessage response)
            {
                var body = await response.Content.ReadAsStringAsync();
                if (body is null)
                    return false;

                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(body);

                var otpCodeNodes = doc.DocumentNode.SelectNodes(".//input[@name='otpCode']");
                return otpCodeNodes != null && otpCodeNodes.Any();
            }

            public override async Task<LoginResult> CreateResultAsync(Authenticate authenticate, HttpResponseMessage response, Dictionary<string, string> oldInputs)
            {
                // shared validation
                await base.CreateResultAsync(authenticate, response, oldInputs);

                var body = await response.Content.ReadAsStringAsync();
                return new TwoFactorAuthenticationPage(authenticate, body);
            }
        }
    }
}
