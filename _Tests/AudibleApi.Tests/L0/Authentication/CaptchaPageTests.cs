﻿namespace Authentic.CaptchaPageTests
{
    [TestClass]
    public class ctor
    {
        [TestMethod]
        public void null_img_throws()
            => Assert.ThrowsException<ArgumentNullException>(() => new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", null, "email", "pw"));


        [TestMethod]
        public void valid_create()
        {
			var uri = new Uri("http://a.com");
			var page = new CaptchaPage(AuthenticateShared.GetAuthenticate(), "<input name='a' value='z'>", uri, "email", "pw");

            page.CaptchaImage.Should().Be(uri);

            var inputs = page.GetInputsReadOnly();
            inputs.Count.Should().Be(2);
            inputs["a"].Should().Be("z");
            inputs["email"].Should().Be("email");
			page.Password.Should().Be("pw");
        }
    }

    [TestClass]
    public class SubmitAsync
    {
        private CaptchaPage getPage()
			=> new CaptchaPage(AuthenticateShared.GetAuthenticate(), "body", new Uri("http://a.com"), "email", "pw");

		[TestMethod]
        public async Task null_guess_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => getPage().SubmitAsync("pass", null));

		[TestMethod]
        public async Task null_pass_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => getPage().SubmitAsync(null, "guess"));

		[TestMethod]
        public async Task null_param_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => getPage().SubmitAsync(null, null));

        [TestMethod]
        public async Task blank_param_throws()
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("", ""));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("   ", "   "));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("", "   "));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("", "   "));
			await Assert.ThrowsExceptionAsync<ArgumentException>(() => getPage().SubmitAsync("   ", ""));
        }

        [TestMethod]
        public async Task valid_param_calls_GetResultsPageAsync()
        {
            var responseToCaptureRequest = new HttpResponseMessage();
			var page = new CaptchaPage(AuthenticateShared.GetAuthenticate(responseToCaptureRequest), "body", new Uri("http://a.com"), "email", "pw");

			await Assert.ThrowsExceptionAsync<LoginFailedException>(() => page.SubmitAsync("pw", "GUESS1"));

            var content = await responseToCaptureRequest.RequestMessage.Content.ReadAsStringAsync();
            var split = content.Split('&');
            var dic = split.Select(s => s.Split('=')).ToDictionary(key => key[0], value => value[1]);
            dic.Count.Should().Be(7);
            dic["email"].Should().Be("email");
            dic["password"].Should().Be("pw");
            dic["guess"].Should().Be("guess1");
            dic["rememberMe"].Should().Be("true");
            dic["use_image_captcha"].Should().Be("true");
            dic["use_audio_captcha"].Should().Be("false");
        }
    }
}
