using System;
using System.Threading.Tasks;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    public class ApprovalNeededPage : LoginResult
    {
        public ApprovalNeededPage(Authenticate authenticate, string responseBody) : base(authenticate, responseBody) { }

        public Task<LoginResult> SubmitAsync()
		{
            // links[0] == "#"
            // links[1] is the correct redirect link
            var links = HtmlHelper.GetLinks(ResponseBody, "a-link-normal");
            return LoginResultRunner.GetResultsPageAsync(Authenticate, links[1]);
        }
    }
}
