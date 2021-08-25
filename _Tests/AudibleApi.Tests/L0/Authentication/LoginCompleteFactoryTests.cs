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
using TestAudibleApiCommon;

namespace Authentic.ResultFactoryTests.LoginCompleteFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task null_returns_false()
            => (await ResultFactory.LoginComplete.IsMatchAsync(null)).Should().BeFalse();

        [TestMethod]
        public async Task _200_returns_false()
        {
            var code = (HttpStatusCode)200;
            var response = new HttpResponseMessage { StatusCode = code };
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task _400_returns_false()
        {
            var code = (HttpStatusCode)400;
            var response = new HttpResponseMessage { StatusCode = code };
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task null_Location_returns_false()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.OK };
            response.Headers.Location.Should().BeNull();
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task no_access_token_returns_false()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved };
            response.Headers.Location = new Uri("http://t.co/?a=1");
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task no_auth_time_returns_false()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved };
            response.Headers.Location = new Uri("http://t.co/?openid.oa2.access_token=xyz");
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeFalse();
        }

        [TestMethod]
        public async Task valid_returns_true()
        {
            var response = new HttpResponseMessage { StatusCode = HttpStatusCode.Moved, Content = new StringContent("foo") };
            response.Headers.Location = new Uri("http://t.co/?openid.oa2.access_token=Atna|&openid.pape.auth_time=2000-01-01");
            (await ResultFactory.LoginComplete.IsMatchAsync(response)).Should().BeTrue();
        }
    }
}
