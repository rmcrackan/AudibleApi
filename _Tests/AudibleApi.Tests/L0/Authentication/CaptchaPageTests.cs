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

namespace Authentic.CaptchaPageTests
{
    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void null_img_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", null, "pw"));

        [TestMethod]
        public void null_password_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", new Uri("http://a.com"), null));

        [TestMethod]
        public void blank_password_throws()
        {
            Assert.ThrowsException<ArgumentException>(() => new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", new Uri("http://a.com"), ""));

            Assert.ThrowsException<ArgumentException>(() => new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", new Uri("http://a.com"), "   "));
        }

        [TestMethod]
        public void valid_create()
        {
			var uri = new Uri("http://a.com");
			var page = new CaptchaPage(AuthenticateShared.GetAuthenticate(), "<input name='a' value='z'>", uri, "pw");

            page.CaptchaImage.Should().Be(uri);

            var inputs = page.GetInputsReadOnly();
            inputs.Count.Should().Be(2);
            inputs["a"].Should().Be("z");
            inputs["password"].Should().Be("pw");
        }
    }

    [TestClass]
    public class SubmitAsync
    {
        private CaptchaPage getPage()
			=> new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", new Uri("http://a.com"), "pw");

		[TestMethod]
        public async Task null_param_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => getPage().SubmitAsync(null));

        [TestMethod]
        public async Task blank_param_throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync(""));
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("   "));
        }

        [TestMethod]
        public async Task valid_param_calls_GetResultsPageAsync()
        {
            var responseToCaptureRequest = new HttpResponseMessage();
			var page = new CaptchaPage(AuthenticateShared.GetAuthenticate(responseToCaptureRequest), "body", new Uri("http://a.com"), "pw");

			await Assert.ThrowsExceptionAsync<LoginFailedException>(() => page.SubmitAsync("GUESS1"));

            var content = await responseToCaptureRequest.RequestMessage.Content.ReadAsStringAsync();
            var split = content.Split('&');
            var dic = split.Select(s => s.Split('=')).ToDictionary(key => key[0], value => value[1]);
            dic.Count.Should().Be(5);
            dic["password"].Should().Be("pw");
            dic["guess"].Should().Be("guess1");
            dic["rememberMe"].Should().Be("true");
            dic["use_image_captcha"].Should().Be("true");
            dic["use_audio_captcha"].Should().Be("false");
        }
    }
}
