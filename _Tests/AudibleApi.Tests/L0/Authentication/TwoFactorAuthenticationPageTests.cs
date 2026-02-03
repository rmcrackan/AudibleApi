namespace Authentic.TwoFactorAuthenticationPageTests;

[TestClass]
public class SubmitAsync
{
	const string MOCK_MFA_BODY = "<a id=\"auth-get-new-otp-link\" href=\"ht" + "tp://mfa.code.com/\" />";
	[TestMethod]
	public async Task null_param_throws()
		=> await Assert.ThrowsAsync<ArgumentNullException>(() => new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(), MOCK_MFA_BODY).SubmitAsync(null!));

	[TestMethod]
	public async Task blank_param_throws()
	{
		await Assert.ThrowsAsync<ArgumentException>(() => new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(), MOCK_MFA_BODY).SubmitAsync(""));
		await Assert.ThrowsAsync<ArgumentException>(() => new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(), MOCK_MFA_BODY).SubmitAsync("   "));
	}

	[TestMethod]
	public async Task valid_param_calls_GetResultsPageAsync()
	{
		var responseToCaptureRequest = new HttpResponseMessage();

		var page = new TwoFactorAuthenticationPage(AuthenticateShared.GetAuthenticate(responseToCaptureRequest), MOCK_MFA_BODY);

		await Assert.ThrowsAsync<LoginFailedException>(() => page.SubmitAsync("2fa"));

		responseToCaptureRequest.RequestMessage.ShouldNotBeNull();
		responseToCaptureRequest.RequestMessage.Content.ShouldNotBeNull();

		var content = await responseToCaptureRequest.RequestMessage.Content.ReadAsStringAsync();
		var split = content.Split('&');
		var dic = split.Select(s => s.Split('=')).ToDictionary(key => key[0], value => value[1]);
		dic.Count.ShouldBe(3);
		dic["otpCode"].ShouldBe("2fa");
		dic["rememberDevice"].ShouldBe("false");
		dic["mfaSubmit"].ShouldBe("Submit");
	}
}
