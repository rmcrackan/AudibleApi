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
	internal abstract partial class ResultFactory
    {
        private class TwoFactorAuthenticationPageFactory : ResultFactory
        {
            public TwoFactorAuthenticationPageFactory() : base(nameof(TwoFactorAuthenticationPageFactory)) { }

            protected override bool _isMatchAsync(HttpResponseMessage response, string body)
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(body);

                var otpCodeNodes = doc.DocumentNode.SelectNodes(".//input[@name='otpCode']");
                return otpCodeNodes != null && otpCodeNodes.Any();
            }

			protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
                => new TwoFactorAuthenticationPage(authenticate, body);
        }
    }
}
