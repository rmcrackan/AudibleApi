using System;
using System.Net.Http;
using System.Threading.Tasks;
using Dinah.Core;

namespace AudibleApi.Authentication
{
    public class TwoFactorAuthenticationPage : LoginResult
    {
        public TwoFactorAuthenticationPage(IHttpClient client, ISystemDateTime systemDateTime, string responseBody) : base(client, systemDateTime, responseBody) { }

        public async Task<LoginResult> SubmitAsync(string _2faCode)
        {
            if (_2faCode is null)
                throw new ArgumentNullException(nameof(_2faCode));

            if (string.IsNullOrWhiteSpace(_2faCode))
                throw new ArgumentException("2FA code may not be blank", nameof(_2faCode));

            Inputs["otpCode"] = _2faCode.Trim();
			Inputs["mfaSubmit"] = "Submit";
			Inputs["rememberDevice"] = "false";

			return await GetResultsPageAsync(Inputs);
        }
    }
}
