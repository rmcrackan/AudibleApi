using System;

namespace AudibleApi;

/// <summary>
/// Audible (or an intermediary) returned HTML or other non-JSON content where JSON was expected.
/// Common causes include expired login, VPN/proxy interference, and transient gateway errors.
/// </summary>
public class NonJsonResponseException : Exception
{
	public string? RequestUri { get; }
	public string? HtmlTitle { get; }
	public string ResponseBodySnippet { get; }

	public NonJsonResponseException(Uri? requestUri, string responseBody)
		: this(requestUri?.OriginalString, responseBody, null) { }

	public NonJsonResponseException(string? requestUri, string responseBody)
		: this(requestUri, responseBody, null) { }

	public NonJsonResponseException(string? requestUri, string responseBody, Exception? innerException)
		: base(BuildMessage(requestUri, responseBody), innerException)
	{
		RequestUri = requestUri;
		HtmlTitle = ResponseBodyInspector.TryGetHtmlTitle(responseBody);
		ResponseBodySnippet = ResponseBodyInspector.GetSnippet(responseBody);
	}

	internal static string BuildMessage(string? requestUri, string responseBody)
	{
		var title = ResponseBodyInspector.TryGetHtmlTitle(responseBody);
		var titlePart = title is null ? "" : $" ({title})";
		var snippet = ResponseBodyInspector.GetSnippet(responseBody);
		var uriPart = string.IsNullOrWhiteSpace(requestUri) ? "" : $" for {requestUri}";
		return $"Audible returned HTML instead of JSON{titlePart}{uriPart}. Response snippet: {snippet}";
	}
}
