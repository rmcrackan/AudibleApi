using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    internal class ApprovalNeededPage : LoginResult
    {
        public ApprovalNeededPage(Authenticate authenticate, string responseBody) : base(authenticate, responseBody) { }

        public Task<LoginResult> SubmitAsync()
		{
            // links[0] == "#"
            // links[1] is the correct redirect link
            var links = HtmlHelper.GetLinks(ResponseBody, "a-link-normal");

			var debugInfo = new List<string> { $"Count: {links.Count}" };
			for (var i = 0; i < links.Count; i++)
                debugInfo.Add($"  links[{i}].Length: {links[i]?.Length}");
            var link = links.FirstOrDefault(l => l is not null && l.Trim().Length > 1);

            Serilog.Log.Logger.Debug("Page info {@DebugInfo}", new { link, debugInfo });
            return LoginResultRunner.GetResultsPageAsync(Authenticate, new(), HttpMethod.Get, link);
        }
    }
}
