using Dinah.Core;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace AudibleApi.Authentication;

internal abstract partial class ResultFactory
{
	private class ApprovalNeededPageFactory : ResultFactory
	{
		public ApprovalNeededPageFactory() : base(nameof(ApprovalNeededPageFactory)) { }

		protected override bool _isMatchAsync(HttpResponseMessage response, string body)
			=> HtmlHelper.GetDivCount(body, "resend-approval-alert") > 0
			|| HtmlHelper.GetElements(body, "form", "id", "resend-approval-form").Any()
			|| HtmlHelper.GetElements(body, "a", "id", "resend-approval-link").Any();

		protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string?> oldInputs)
			=> new ApprovalNeededPage(authenticate, body);
	}
}
