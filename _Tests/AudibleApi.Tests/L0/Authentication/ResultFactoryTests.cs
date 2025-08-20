namespace Authentic.ResultFactoryTests
{
    internal class FakeLoginResult : LoginResult { public FakeLoginResult() : base(AuthenticateShared.GetAuthenticate(), "body") { } }
    internal class ConcreteResultFactory : ResultFactory
    {
        public ConcreteResultFactory() : base(nameof(ConcreteResultFactory)) { }

		protected override LoginResult _createResultAsync(Authenticate authenticate, HttpResponseMessage response, string body, Dictionary<string, string> oldInputs)
            => new FakeLoginResult();

        protected override bool _isMatchAsync(HttpResponseMessage response, string body)
            => response.Content.ReadAsStringAsync().GetAwaiter().GetResult() == "IsMatch";
	}

    [TestClass]
    public class IsMatchAsync
    {
        [TestMethod]
        public async Task null_response_returns_false()
            => (await new ConcreteResultFactory().IsMatchAsync(null)).ShouldBeFalse();

        [TestMethod]
        public async Task null_content_returns_false()
            => (await new ConcreteResultFactory().IsMatchAsync(new HttpResponseMessage())).ShouldBeFalse();

        [TestMethod]
        public async Task invalid_content_returns_false()
            => (await new ConcreteResultFactory().IsMatchAsync(new HttpResponseMessage { Content = new StringContent("x") })).ShouldBeFalse();

        [TestMethod]
        public async Task valid_content_returns_true()
            => (await new ConcreteResultFactory().IsMatchAsync(new HttpResponseMessage { Content = new StringContent("IsMatch") })).ShouldBeTrue();
    }

    [TestClass]
    public class CreateResultAsync
    {
        [TestMethod]
        public async Task null_authenticate_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new ConcreteResultFactory().CreateResultAsync(null, new HttpResponseMessage { Content = new StringContent("x") }, new Dictionary<string, string>()));

        [TestMethod]
		public async Task null_response_throws()
			=> await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), null, new Dictionary<string, string>()));

        [TestMethod]
        public async Task null_inputs_throws()
            => await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent("x") }, null));

        [TestMethod]
        public async Task false_IsMatch_throws()
        {
            var badMatch = "x";
            await Assert.ThrowsExceptionAsync<LoginFailedException>(() => new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent(badMatch) }, new Dictionary<string, string>())
            );
        }

        [TestMethod]
        public async Task valid_returns_createResultAsync()
        {
            var result = await new ConcreteResultFactory().CreateResultAsync(AuthenticateShared.GetAuthenticate(), new HttpResponseMessage { Content = new StringContent("IsMatch") }, new Dictionary<string, string>());
            Assert.IsTrue(result is FakeLoginResult);
        }
    }
}
