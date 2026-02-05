namespace Authentic;

internal class AuthenticateShared
{
	public static Authenticate GetAuthenticate(ApiHttpClient client)
		=> new(Locales.Us, "", client, StaticSystemDateTime.Past)
		{
			MaxLoadSessionCookiesTrips = 3
		};
	public static Authenticate GetAuthenticate(string? handlerReturnString = null, HttpStatusCode statusCode = HttpStatusCode.OK)
		=> new(Locales.Us, "", ApiHttpClientMock.GetClient(handlerReturnString, statusCode), StaticSystemDateTime.Past)
		{
			MaxLoadSessionCookiesTrips = 3
		};
	public static Authenticate GetAuthenticate(HttpResponseMessage response)
		=> new(Locales.Us, "", ApiHttpClientMock.GetClient(response), StaticSystemDateTime.Past)
		{
			MaxLoadSessionCookiesTrips = 3
		};
}
