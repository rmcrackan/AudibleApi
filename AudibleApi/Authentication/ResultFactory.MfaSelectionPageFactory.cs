using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Dinah.Core;

namespace AudibleApi.Authentication
{
	internal abstract partial class ResultFactory
    {
        private class MfaSelectionPageFactory : ResultFactory
        {
            public MfaSelectionPageFactory() : base(nameof(CaptchaPageFactory)) { }

            protected override bool _isMatchAsync(HttpResponseMessage response, string body)
                => HtmlHelper.GetElements(body, "form", "id", "auth-select-device-form").Any();

            protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
                => new MfaSelectionPage(authenticate, response.RequestMessage.RequestUri, body);
        }
    }
}
