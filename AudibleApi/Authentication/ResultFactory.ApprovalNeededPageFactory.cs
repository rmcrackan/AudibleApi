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
        private class ApprovalNeededPageFactory : ResultFactory
        {
            public ApprovalNeededPageFactory() : base(nameof(ApprovalNeededPageFactory)) { }

            protected override bool _isMatchAsync(HttpResponseMessage response, string body)
                => HtmlHelper.GetDivCount(body, "resend-approval-alert") > 0;

            protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
                => new ApprovalNeededPage(authenticate, body);
        }
    }
}
