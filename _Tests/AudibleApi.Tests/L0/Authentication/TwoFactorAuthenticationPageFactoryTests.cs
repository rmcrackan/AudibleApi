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

namespace Authentic.ResultFactoryTests.TwoFactorAuthenticationPageFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task null_response_returns_false()
            => (await ResultFactory.TwoFactorAuthenticationPage.IsMatchAsync(null)).Should().BeFalse();

        [TestMethod]
        public async Task null_content_returns_false()
        {
            var response = new HttpResponseMessage();
            response.Content.Should().BeNull();
            (await ResultFactory.TwoFactorAuthenticationPage.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task not_a_match()
        {
            var response = new HttpResponseMessage { Content = new StringContent("<input name='fail' />") };
            (await ResultFactory.TwoFactorAuthenticationPage.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task is_a_match()
        {
            var response = new HttpResponseMessage { Content = new StringContent("<input name='otpCode' />") };
            (await ResultFactory.TwoFactorAuthenticationPage.IsMatchAsync(response)).Should().BeTrue();
        }
    }

    [TestClass]
    public class CreateResultAsync
    {
        [TestMethod]
        public async Task valid_returns_TwoFactorAuthenticationPage()
        {
            var body
                = "<input name='foo' value='abc' />"
                + "<input name='bar' value='xyz' />"
                + "<input name='otpCode' value='2fa' />";

            var response = new HttpResponseMessage { Content = new StringContent(body) };

            var _2faPage = await ResultFactory.TwoFactorAuthenticationPage.CreateResultAsync(AuthenticateShared.GetAuthenticate(AuthenticateResponse), response, new Dictionary<string, string>()) as TwoFactorAuthenticationPage;

            var inputs = _2faPage.GetInputsReadOnly();
            inputs.Count.Should().Be(3);
            inputs["foo"].Should().Be("abc");
            inputs["bar"].Should().Be("xyz");
            inputs["otpCode"].Should().Be("2fa");
        }
    }
}
