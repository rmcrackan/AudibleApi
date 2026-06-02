using AudibleApi;

namespace ResponseBodyInspectorTests;

[TestClass]
public class IsHtmlResponse
{
	[TestMethod]
	public void false_for_null_or_empty() => Assert.IsFalse(ResponseBodyInspector.IsHtmlResponse(null));

	[TestMethod]
	public void false_for_json() => Assert.IsFalse(ResponseBodyInspector.IsHtmlResponse("{\"a\":1}"));

	[TestMethod]
	public void true_for_html_with_leading_whitespace()
		=> Assert.IsTrue(ResponseBodyInspector.IsHtmlResponse("  \r\n<html></html>"));

	[TestMethod]
	public void false_for_login_html_fragment()
		=> Assert.IsFalse(ResponseBodyInspector.IsHtmlResponse("<input name='email' value='e' /><input name='password' value='pw' />"));
}

[TestClass]
public class TryGetHtmlTitle
{
	[TestMethod]
	public void extracts_title_case_insensitive()
	{
		var html = "<HTML><HEAD><TITLE>502 Bad Gateway</TITLE></HEAD></HTML>";
		Assert.AreEqual("502 Bad Gateway", ResponseBodyInspector.TryGetHtmlTitle(html));
	}

	[TestMethod]
	public void null_when_missing() => Assert.IsNull(ResponseBodyInspector.TryGetHtmlTitle("<html><body></body></html>"));
}

[TestClass]
public class NonJsonResponseException_message
{
	[TestMethod]
	public void includes_title_and_snippet()
	{
		var html = "<html><head><title>Sign In</title></head><body>x</body></html>";
		var ex = new NonJsonResponseException("https://api.audible.com/foo", html);

		Assert.AreEqual("Sign In", ex.HtmlTitle);
		Assert.IsTrue(ex.Message.Contains("Sign In", StringComparison.Ordinal));
		Assert.IsTrue(ex.Message.Contains("HTML instead of JSON", StringComparison.Ordinal));
		Assert.IsFalse(string.IsNullOrWhiteSpace(ex.ResponseBodySnippet));
	}
}
