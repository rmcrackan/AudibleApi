using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;

namespace AudibleApi;

/// <summary>
/// Throw strongly typed exceptions based on conventions of APIs by amazon/audible
/// </summary>
public class ApiMessageHandler : MessageProcessingHandler
{
	public ApiMessageHandler() : base() { }
	public ApiMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

	protected override HttpRequestMessage ProcessRequest(HttpRequestMessage request, CancellationToken cancellationToken)
		=> request;

	protected override HttpResponseMessage ProcessResponse(HttpResponseMessage response, CancellationToken cancellationToken)
	{
		var debugContentHeaders
			= response
			?.Content
			?.Headers
			?.ToString();

		#region blacklist
		// BLACKLIST: if conditions here are met, return without processing

		// this is almost certainly a mock call
		if (response == null ||
			response.Content == null ||
			response.Content.Headers == null ||
			string.IsNullOrWhiteSpace(debugContentHeaders) ||
			response.Content.Headers.ContentType == null)
			return response ?? new();

		var length = response.Content.Headers.ContentLength;
		if (!length.HasValue || length.Value == 0 || length.Value > 1_000_000)
			return response;
		#endregion

		#region whitelist
		// WHITELIST: if any condition here is met, allow processing. else, return without processing

		var shouldParse = false;

		// especially don't want to try to parse large files with
		// Content-Type: audio/vnd.audible.aax
		var whitelist = new List<string>
		{
			"application/json",
			"application/xml",
			"application/x-www-form-urlencoded",
			"multipart/form-data",
			 // eg: text/plain , text/html , text/html; charset=utf-8
			"text/"
		};
		var contentType = response.Content.Headers.ContentType?.ToString();
		if (whitelist.Any(x => contentType?.StartsWith(x) is true))
			shouldParse = true;

		if (!shouldParse)
			return response;
		#endregion

		// possible deadlock if called from a UI thread?
		var content = response
			.Content.ReadAsStringAsync(cancellationToken)
			.GetAwaiter().GetResult();

		RestMessageValidator.ThrowStrongExceptionsIfInvalid(content, response.RequestMessage?.RequestUri);

		return response;
	}
}
