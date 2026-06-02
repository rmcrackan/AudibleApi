using System;
using System.Text.RegularExpressions;

namespace AudibleApi;

/// <summary>
/// Detects non-JSON bodies (typically HTML error or login pages) returned where JSON was expected.
/// </summary>
public static partial class ResponseBodyInspector
{
	public const int DefaultSnippetMaxLength = 200;

	[GeneratedRegex(@"<title[^>]*>\s*([^<]+?)\s*</title>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
	private static partial Regex HtmlTitleRegex();

	/// <summary>
	/// True when the body looks like an HTML document (error page, login redirect, etc.), not a bare fragment
	/// such as the partial markup returned during Amazon's multi-step login flow.
	/// </summary>
	public static bool IsHtmlResponse(string? content)
	{
		if (string.IsNullOrWhiteSpace(content))
			return false;

		var trimmed = content.TrimStart();
		if (!trimmed.StartsWith('<'))
			return false;

		return LooksLikeHtmlDocument(trimmed);
	}

	private static bool LooksLikeHtmlDocument(string trimmed)
		=> trimmed.Contains("<html", StringComparison.OrdinalIgnoreCase)
		|| trimmed.Contains("<!DOCTYPE", StringComparison.OrdinalIgnoreCase)
		|| trimmed.Contains("<title", StringComparison.OrdinalIgnoreCase)
		|| trimmed.Contains("<head", StringComparison.OrdinalIgnoreCase)
		|| trimmed.Contains("<body", StringComparison.OrdinalIgnoreCase);

	public static string? TryGetHtmlTitle(string content)
	{
		if (string.IsNullOrWhiteSpace(content))
			return null;

		var match = HtmlTitleRegex().Match(content);
		if (!match.Success)
			return null;

		var title = match.Groups[1].Value.Trim();
		return string.IsNullOrWhiteSpace(title) ? null : title;
	}

	public static string GetSnippet(string content, int maxLength = DefaultSnippetMaxLength)
	{
		if (string.IsNullOrEmpty(content))
			return string.Empty;

		var normalized = content.Replace("\r\n", " ").Replace('\n', ' ').Replace('\r', ' ').Trim();
		while (normalized.Contains("  ", StringComparison.Ordinal))
			normalized = normalized.Replace("  ", " ", StringComparison.Ordinal);

		if (normalized.Length <= maxLength)
			return normalized;

		return normalized[..maxLength] + "...";
	}
}
