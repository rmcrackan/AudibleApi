namespace LoginTests_L0
{
    [TestClass]
	public class ctor
	{
		[TestMethod]
		public void access_from_L0_throws()
			=> Assert.Throws<MethodAccessException>(() => new Authenticate(Locale.Empty, null));
	}

    [TestClass]
    public class ctor_client_systemDateTime
    {
        [TestMethod]
        public void null_locale_throws()
            => Assert.Throws<ArgumentNullException>(() => new Authenticate(null, null, ApiHttpClient.Create(HttpMock.GetHandler()), Substitute.For<ISystemDateTime>()));

        [TestMethod]
        public void null_httpClientHandler_throws()
            => Assert.Throws<ArgumentNullException>(() => new Authenticate(Locale.Empty, null, null, Substitute.For<ISystemDateTime>()));

		[TestMethod]
		public void has_cookies_throws()
		{
			var client = ApiHttpClient.Create(HttpMock.GetHandler());
			client.CookieJar.Add(new Cookie("foo", "bar", "/", "a.com"));
			Assert.Throws<ArgumentException>(() => new Authenticate(Locale.Empty, null, client, Substitute.For<ISystemDateTime>()));
		}

		[TestMethod]
		public void null_systemDateTime_throws()
			=> Assert.Throws<ArgumentNullException>(() => new Authenticate(Locale.Empty, null, ApiHttpClient.Create(HttpMock.GetHandler()), null));
    }
}
