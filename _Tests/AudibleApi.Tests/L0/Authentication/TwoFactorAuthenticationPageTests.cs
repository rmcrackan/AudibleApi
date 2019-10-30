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
using Dinah.Core.Net;
using Dinah.Core.Net.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json.Linq;
using TestAudibleApiCommon;
using TestCommon;

namespace Authentic.TwoFactorAuthenticationPageTests
{
    [TestClass]
    public class SubmitAsync
    {
		[TestMethod]
        public async Task null_param_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new TwoFactorAuthenticationPage(ApiHttpClientMock.GetClient(), StaticSystemDateTime.Past, "x").SubmitAsync(null));

        [TestMethod]
        public async Task blank_param_throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => new TwoFactorAuthenticationPage(ApiHttpClientMock.GetClient(), StaticSystemDateTime.Past, "body").SubmitAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => new TwoFactorAuthenticationPage(ApiHttpClientMock.GetClient(), StaticSystemDateTime.Past, "body").SubmitAsync("   "));
        }

        [TestMethod]
        public async Task valid_param_calls_GetResultsPageAsync()
        {
            var responseToCaptureRequest = new HttpResponseMessage();

			var page = new TwoFactorAuthenticationPage(ApiHttpClientMock.GetClient(responseToCaptureRequest), StaticSystemDateTime.Past, "body");

			await Assert.ThrowsExceptionAsync<LoginFailedException>(() => page.SubmitAsync("2fa"));

            var content = await responseToCaptureRequest.RequestMessage.Content.ReadAsStringAsync();
            var split = content.Split('&');
            var dic = split.Select(s => s.Split('=')).ToDictionary(key => key[0], value => value[1]);
            dic.Count.Should().Be(3);
            dic["otpCode"].Should().Be("2fa");
			dic["rememberDevice"].Should().Be("false");
			dic["mfaSubmit"].Should().Be("Submit");
		}
    }
}
