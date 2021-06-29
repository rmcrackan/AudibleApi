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

namespace Authentic.ResultFactoryTests.CaptchaPageFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task captcha_returns_true()
        {
            var body
                = "<input name='email' />"
                + "<input name='password' />"
                + "<input name='use_image_captcha' />";
            var response = new HttpResponseMessage { Content = new StringContent(body) };
            var result = await ResultFactory.CaptchaPage.IsMatchAsync(response);
            result.Should().BeTrue();
        }

        [TestMethod]
        public async Task no_captcha_returns_false()
        {
            var body
                = "<input name='email' />"
                + "<input name='password' />";
            var response = new HttpResponseMessage { Content = new StringContent(body) };
            var result = await ResultFactory.CaptchaPage.IsMatchAsync(response);
            result.Should().BeFalse();
        }
    }

    [TestClass]
    public class CreateResultAsync
    {
        [TestMethod]
        public async Task no_password_throws()
        {
            var body
                = "<input name='email' />"
                + "<input name='password' />"
                + "<input name='use_image_captcha' />";
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => ResultFactory.CaptchaPage.CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent(body) }, new Dictionary<string, string>()));
        }
    }
}
