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
}
