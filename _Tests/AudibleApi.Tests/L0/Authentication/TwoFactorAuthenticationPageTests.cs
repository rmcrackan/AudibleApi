namespace Authentic.TwoFactorAuthenticationPageTests
{
    [TestClass]
    public class SubmitAsync
    {
		[TestMethod]
        public async Task null_param_throws()
            => await Assert.ThrowsAsync<ArgumentNullException>(() => new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(), "x").SubmitAsync(null));

        [TestMethod]
        public async Task blank_param_throws()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(), "body").SubmitAsync(""));
            await Assert.ThrowsAsync<ArgumentException>(() => new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(), "body").SubmitAsync("   "));
        }

        [TestMethod]
        public async Task valid_param_calls_GetResultsPageAsync()
        {
            var responseToCaptureRequest = new HttpResponseMessage();

			var page = new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(responseToCaptureRequest), "body");

			await Assert.ThrowsAsync<LoginFailedException>(() => page.SubmitAsync("2fa"));

            var content = await responseToCaptureRequest.RequestMessage.Content.ReadAsStringAsync();
            var split = content.Split('&');
            var dic = split.Select(s => s.Split('=')).ToDictionary(key => key[0], value => value[1]);
            dic.Count.ShouldBe(3);
            dic["otpCode"].ShouldBe("2fa");
			dic["rememberDevice"].ShouldBe("false");
			dic["mfaSubmit"].ShouldBe("Submit");
		}
    }
}
