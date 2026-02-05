using Dinah.Core;
using Dinah.Core.Net.Http;
using System.Net.Http;
using System.Threading.Tasks;

namespace AudibleApi;

public partial class ApiUnauthenticated
{
	public virtual bool IsAuthenticated => false;
	public IHttpClientSharer Sharer { get; }
	protected Locale Locale { get; }
	protected IHttpClientActions Client
		=> Sharer.GetSharedHttpClient(Locale.AudibleApiUri());

	public ApiUnauthenticated(Locale locale)
	{
		StackBlocker.ApiTestBlocker();
		Locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));
		Sharer = new HttpClientSharer();
	}

	public ApiUnauthenticated(Locale locale, IHttpClientSharer sharer)
	{
		Locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));
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
