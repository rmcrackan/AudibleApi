using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public class ApprovalNeeded : LoginResult
    {
        public ApprovalNeeded(Authenticate authenticate, string responseBody) : base(authenticate, responseBody) { }

        public Task<LoginResult> SubmitAsync() => LoginResultRunner.GetResultsPageAsync(Authenticate, System.Web.HttpUtility.UrlDecode(Inputs["openid.return_to"]));
    }
}
