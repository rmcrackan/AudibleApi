namespace Authentic.LoginResultRunnerTests
{
    [TestClass]
    public class GetResultsPageAsync
    {
		[TestMethod]
        public async Task null_authenticate_throws()
            => await Assert.ThrowsAsync<ArgumentNullException>(() => LoginResultRunner.GetResultsPageAsync(null, new Dictionary<string, string>(), HttpMethod.Get, ""));

        [TestMethod]
        public async Task null_inputs_throws()
            => await Assert.ThrowsAsync<ArgumentNullException>(() => LoginResultRunner.GetResultsPageAsync(AuthenticateShared.GetAuthenticate(), null, HttpMethod.Get, ""));

        [TestMethod]
        public async Task returns_CredentialsPage()
        {
            var response
                = "<input name='email' value='e' />"
                + "<input name='password' value='pw' />";
            var client = ApiHttpClientMock.GetClient(response);
            var result = await LoginResultRunner.GetResultsPageAsync(AuthenticateShared.GetAuthenticate(client), new Dictionary<string, string>(), HttpMethod.Get, "");
            var page = result as CredentialsPage;
            page.ShouldNotBeNull();
        }

        [TestMethod]
        public async Task returns_TwoFactorAuthenticationPage()
        {
            var response = "<input name='otpCode' value='2fa' />";
            var client = ApiHttpClientMock.GetClient(response);
            var result = await LoginResultRunner.GetResultsPageAsync(AuthenticateShared.GetAuthenticate(client), new Dictionary<string, string>(), HttpMethod.Get, "");
            var page = result as TwoFactorAuthenticationPage;
            page.ShouldNotBeNull();
        }
    }
}
