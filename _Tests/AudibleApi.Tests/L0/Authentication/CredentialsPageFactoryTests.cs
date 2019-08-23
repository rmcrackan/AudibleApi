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
using TestCommon;

namespace Authentic.ResultFactoryTests.CredentialsPageFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task captcha_returns_false()
        {
            var body
                = "<input name='email' />"
                + "<input name='password' />"
                + "<input name='use_image_captcha' />";
            var response = new HttpResponseMessage { Content = new StringContent(body) };
            var result = await ResultFactory.CredentialsPage.IsMatchAsync(response);
            result.Should().BeFalse();
        }

        [TestMethod]
        public async Task no_captcha_returns_true()
        {
            var body
                = "<input name='email' />"
                + "<input name='password' />";
            var response = new HttpResponseMessage { Content = new StringContent(body) };
            var result = await ResultFactory.CredentialsPage.IsMatchAsync(response);
            result.Should().BeTrue();
        }
    }

    [TestClass]
    public class CreateResultAsync
    {
        [TestMethod]
        public async Task valid_returns_CredentialsPage()
        {
            var body
                = "<input name='email' value='zzz' />"
                + "<input name='password' />";
            var credentialsPage = await ResultFactory.CredentialsPage.CreateResultAsync(ApiClientMock.GetClient(), StaticSystemDateTime.Past, new HttpResponseMessage { Content = new StringContent(body) }, new Dictionary<string, string>()) as CredentialsPage;

            var inputs = credentialsPage.GetInputsReadOnly();
            inputs.Count.Should().Be(2);
            inputs["email"].Should().Be("zzz");
            inputs["password"].Should().BeNull();
        }
    }
}
