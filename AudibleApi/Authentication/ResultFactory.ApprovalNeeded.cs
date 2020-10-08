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
        private class ApprovalNeededFactory : ResultFactory
        {
            public ApprovalNeededFactory() : base(nameof(ApprovalNeededFactory)) { }

            public override async Task<bool> IsMatchAsync(HttpResponseMessage response)
            {
                // shared validation
                if (!await base.IsMatchAsync(response))
                    return false;

                var body = await response.Content.ReadAsStringAsync();
                var newInputs = HtmlHelper.GetInputs(body);
                return newInputs.ContainsKey("openid.return_to");
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
