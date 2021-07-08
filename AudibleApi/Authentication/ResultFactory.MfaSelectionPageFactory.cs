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
        private class MfaSelectionPageFactory : ResultFactory
        {
            public MfaSelectionPageFactory() : base(nameof(CaptchaPageFactory)) { }

            protected override bool _isMatchAsync(HttpResponseMessage response, string body)
                => HtmlHelper.GetElements(body, "form", "id", "auth-select-device-form").Any();

            protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
                => new MfaSelectionPage(authenticate, body);
        }
    }
}
