namespace HttpClientSharerTests;

[TestClass]
public class ctor
{
	[TestMethod]
	public void access_from_L0_throws()
		=> Assert.Throws<MethodAccessException>(() => new HttpClientSharer());
}

[TestClass]
public class ctor_sharedMessageHandler
{
	[TestMethod]
	public void null_param_throws()
		=> Assert.Throws<ArgumentNullException>(() => new HttpClientSharer(null!));
}

[TestClass]
public class GetSharedHttpClient
{
	public static HttpMessageHandler newHandler(Action action)
		=> HttpMock.CreateMockHttpClientHandler(action);

	[TestMethod]
	public void null_param_throws()
		=> Assert.Throws<ArgumentNullException>(() => new HttpClientSharer(newHandler(() => { })).GetSharedHttpClient((Uri?)null!));

	[TestMethod]
	public async Task uses_SharedMessageHandler()
	{
		var log = new List<string>();
		var settings = new HttpClientSharer(newHandler(() => log.Add("send")));
		var locale = Localization.Get("us");
		var client = settings.GetSharedHttpClient(locale.AmazonApiUri());

		var httpClient = client as HttpClient;
		httpClient.ShouldNotBeNull();
		httpClient.BaseAddress.ShouldNotBeNull();
		httpClient.BaseAddress.AbsoluteUri.ShouldBe("https://api.amazon.com/");

		await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, new Uri("http://test.com")));

		log.ShouldBe(["send"]);
	}

	[TestMethod]
	public void returned_instances_are_equal()
	{
		var settings = new HttpClientSharer(newHandler(() => { }));
		var locale = Localization.Get("us");

		var client1 = settings.GetSharedHttpClient(locale.AmazonApiUri());
		var client2 = settings.GetSharedHttpClient(locale.AmazonApiUri());

		client1.ShouldBe(client2);
	}
}
