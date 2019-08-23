using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AudibleApi;
using AudibleApi.Authentication;
using AudibleApi.Authorization;
using Dinah.Core;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using static TestAudibleApiCommon.ComputedTestValues;

namespace Authentic.LoginResultRunnerTests
{
    [TestClass]
    public class GetResultsPageAsync
    {
		[TestMethod]
        public async Task null_client_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => LoginResultRunner.GetResultsPageAsync(null, StaticSystemDateTime.Past, new Dictionary<string, string>()));

		[TestMethod]
		public async Task null_systemDateTime_throws()
			=> await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => LoginResultRunner.GetResultsPageAsync(ApiClientMock.GetClient(), null, new Dictionary<string, string>()));

		[TestMethod]
        public async Task null_inputs_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => LoginResultRunner.GetResultsPageAsync(ApiClientMock.GetClient(), StaticSystemDateTime.Past, null));

        [TestMethod]
        public async Task returns_CredentialsPage()
        {
            var response
                = "<input name='email' value='e' />"
                + "<input name='password' value='pw' />";
            var client = ApiClientMock.GetClient(response);
            var result = await LoginResultRunner.GetResultsPageAsync(client, StaticSystemDateTime.Past, new Dictionary<string, string>());
            var page = result as CredentialsPage;
            page.Should().NotBeNull();
        }

        [TestMethod]
        public async Task returns_CaptchaPage()
        {
            var response
                = "<input name='email' value='e' />"
                + "<input name='password' value='pw' />"
                + "<input name='use_image_captcha' value='true' />"
                + "<img src='http://a.com/foo.png' alt='Visual CAPTCHA image, continue down for an audio option.' />";
            var client = ApiClientMock.GetClient(response);
            var result = await LoginResultRunner.GetResultsPageAsync(client, StaticSystemDateTime.Past, new Dictionary<string, string> { ["password"] = "pw" });
            var page = result as CaptchaPage;
            page.Should().NotBeNull();
        }

        [TestMethod]
        public async Task returns_TwoFactorAuthenticationPage()
        {
            var response = "<input name='otpCode' value='2fa' />";
            var client = ApiClientMock.GetClient(response);
            var result = await LoginResultRunner.GetResultsPageAsync(client, StaticSystemDateTime.Past, new Dictionary<string, string>());
            var page = result as TwoFactorAuthenticationPage;
            page.Should().NotBeNull();
        }

        [TestMethod]
        public async Task returns_LoginComplete()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved, Content = new StringContent(AuthenticateResponse) };
            var uri = new Uri("http://test.com?openid.oa2.access_token=Atna|&openid.pape.auth_time=2000-01-01%2000:00:01");
            response.Headers.Location = uri;
            response.Headers.Add("Set-Cookie", "session-id=123-456-789; Domain=.amazon.com; Expires=Thu, 30-Jun-2039 19:07:14 GMT; Path=/");
            response.Headers.Add("Set-Cookie", "session-id-time=987654321; Domain=.amazon.com; Expires=Thu, 30-Jun-2039 19:07:14 GMT; Path=/");

            var client = ApiClientMock.GetClient(response);
            var result = await LoginResultRunner.GetResultsPageAsync(client, StaticSystemDateTime.Past, new Dictionary<string, string>());
            var page = result as LoginComplete;
            page.Should().NotBeNull();
        }
    }
}
