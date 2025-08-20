namespace Authentic.ResultFactoryTests.MfaSelectionPageFactoryTests
{
    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task match()
        {
            var match = @"<body><form id='auth-select-device-form' /></body>";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(match),
                StatusCode = HttpStatusCode.OK
            };
            (await ResultFactory.MfaSelectionPage.IsMatchAsync(response)).ShouldBeTrue();
        }

        [TestMethod]
        public async Task no_match()
        {
            var noMatch = @"<body><form id='zauth-select-device-form' /></body>";
            var response = new HttpResponseMessage
            {
                Content = new StringContent(noMatch),
                StatusCode = HttpStatusCode.OK
            };
            (await ResultFactory.MfaSelectionPage.IsMatchAsync(response)).ShouldBeFalse();
        }
    }
}
