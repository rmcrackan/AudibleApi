namespace ApiHttpClientTests;

[TestClass]
public class Create
{
	[TestMethod]
	public void access_from_L0_throws()
		=> Assert.Throws<MethodAccessException>(() => ApiHttpClient.Create());
}

[TestClass]
public class Create_innerHandler
{
	[TestMethod]
	public void null_param_throws()
		=> Assert.Throws<ArgumentNullException>(() => ApiHttpClient.Create(null!));

	[TestMethod]
	public void has_cookies_throws()
	{
		var httpClientHandler = new HttpClientHandler
		{
			CookieContainer = new CookieContainer()
		};
		httpClientHandler.CookieContainer.Add(new Cookie("foo", "bar", "/", "a.com"));
		Assert.Throws<ArgumentException>(() => ApiHttpClient.Create(httpClientHandler));
	}

	[TestMethod]
	public void sets_cookie_jar()
		=> ApiHttpClient
			.Create(HttpMock.GetHandler())
			.CookieJar
			.ShouldNotBeNull();

	[TestMethod]
	public async Task throw_api_error()
	{
		// ensure handler incl ApiMessageHandler

		var handler = HttpMock.GetHandler(new JObject
		{
			{ "message", "Invalid response group" }
		}.ToString());
		var client = ApiHttpClient.Create(handler);
		await Assert.ThrowsAsync<InvalidResponseException>(() => client.GetAsync(new Uri("http://a.com")));
	}
}
