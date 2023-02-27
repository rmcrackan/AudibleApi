using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;
using HtmlAgilityPack;

namespace AudibleApi.Authentication
{
    internal class TwoFactorAuthenticationPage : LoginResult
	{
		public string Prompt { get; }
		private string MfaPage { get; }
		public TwoFactorAuthenticationPage(Authenticate authenticate, string responseBody) : base(authenticate, responseBody)
		{
			Prompt
				= HtmlHelper.GetElements(responseBody, "form", "id", "auth-mfa-form")
				?.FirstOrDefault()
				?.ChildNodes
				?.FirstOrDefault(n=> n.Name == "p")
				?.InnerText
				?.Trim() ?? string.Empty;

			var mfaLink = HtmlHelper.GetElements(responseBody, "a", "id", "auth-get-new-otp-link").FirstOrDefault();

			if (mfaLink?.Attributes["href"]?.Value is string url)
				MfaPage = HtmlEntity.DeEntitize(url);
		}

		public async Task<LoginResult> SubmitAsync(string _2faCode)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(_2faCode, nameof(_2faCode));

			Inputs["otpCode"] = _2faCode.Trim();
			Inputs["mfaSubmit"] = "Submit";
			Inputs["rememberDevice"] = "false";

			return await LoginResultRunner.GetResultsPageAsync(Authenticate, Inputs, Method, Action);
		}
		public Task<LoginResult> GetMfaPage() => LoginResultRunner.GetResultsPageAsync(Authenticate, new(), HttpMethod.Get, MfaPage);
	}
}
