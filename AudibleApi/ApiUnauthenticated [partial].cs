using Dinah.Core;
using Dinah.Core.Net.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi;

public partial class ApiUnauthenticated
{
	public virtual bool IsAuthenticated => false;
	public IHttpClientSharer Sharer { get; }

	/// <summary>
	/// The locale this identity is registered with: the marketplace whose Amazon/Audible auth domain issued
	/// the tokens, and the only one they can be refreshed or re-registered against.
	/// </summary>
	protected Locale Locale { get; }

	/// <summary>
	/// <para>
	/// The marketplace whose catalog is being read. Defaults to <see cref="Locale"/>, and for a single-marketplace
	/// account it always equals it.
	/// </para>
	/// <para>
	/// Audible honors one device registration across every marketplace, so a library held under a different
	/// storefront is reachable by pointing the store host elsewhere while the identity keeps authenticating
	/// against its own <see cref="Locale"/>. Anything that speaks to the store - the api host, the download CDN,
	/// catalog language - follows this one; anything that proves who you are follows <see cref="Locale"/>.
	/// </para>
	/// </summary>
	protected Locale StoreLocale { get; }

	protected IHttpClientActions Client
		=> Sharer.GetSharedHttpClient(StoreLocale.AudibleApiUri());

	public ApiUnauthenticated(Locale locale)
		: this(locale, storeLocale: null) { }

	public ApiUnauthenticated(Locale locale, Locale? storeLocale)
	{
		StackBlocker.ApiTestBlocker();
		Locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));
		StoreLocale = storeLocale ?? Locale;
		Sharer = new HttpClientSharer();
	}

	public ApiUnauthenticated(Locale locale, IHttpClientSharer sharer)
		: this(locale, null, sharer) { }

	public ApiUnauthenticated(Locale locale, Locale? storeLocale, IHttpClientSharer sharer)
	{
		Locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));
		StoreLocale = storeLocale ?? Locale;
		Sharer = ArgumentValidator.EnsureNotNull(sharer, nameof(sharer));
	}

	public Task<HttpResponseMessage> AdHocNonAuthenticatedGetAsync(string requestUri)
		=> AdHocNonAuthenticatedGetAsync(requestUri, Client);

	public async Task<HttpResponseMessage> AdHocNonAuthenticatedGetAsync(string requestUri, IHttpClientActions client)
	{
		ArgumentValidator.EnsureNotNullOrWhiteSpace(requestUri, nameof(requestUri));

		var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

		return await SendClientRequest(client, request);
	}

	protected async Task<HttpResponseMessage> SendClientRequest(IHttpClientActions client, HttpRequestMessage request)
	{
		//https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient.sendasync?view=net-6.0
		try
		{
			return await client.SendAsync(request);
		}
		catch (TaskCanceledException ex)
		{
			throw new ApiErrorException(request.RequestUri, ex.ToJson("The request failed due to timeout."));
		}
		catch (HttpRequestException ex)
		{
			throw new ApiErrorException(request.RequestUri, ex.ToJson("The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout."));
		}
	}
}
