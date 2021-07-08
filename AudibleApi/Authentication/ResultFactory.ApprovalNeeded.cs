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
        private class ApprovalNeededFactory : ResultFactory
        {
            public ApprovalNeededFactory() : base(nameof(ApprovalNeededFactory)) { }

            protected override async Task<bool> _isMatchAsync(HttpResponseMessage response)
            {
                var body = await response.Content.ReadAsStringAsync();
                var hasDiv = HtmlHelper.GetDivCount(body, "resend-approval-alert") > 0;
                return hasDiv;
            }

            public override async Task<LoginResult> CreateResultAsync(Authenticate authenticate, HttpResponseMessage response, Dictionary<string, string> oldInputs)
            {
                // shared validation
                await base.CreateResultAsync(authenticate, response, oldInputs);

                var body = await response.Content.ReadAsStringAsync();
                return new ApprovalNeeded(authenticate, body);
            }
        }
    }
}
