namespace ExceptionTests;

/// <summary>
/// identical to other AudibleApiException exceptions (ApiErrorException, InvalidResponseException, InvalidValueException, NotAuthenticatedException) and acts as the unit tests for all of them
/// </summary>
public class FakeAbstractException : AudibleApiException
{
	public FakeAbstractException(Uri requestUri, JObject jObj) : this(requestUri, jObj, null, null) { }
	public FakeAbstractException(Uri requestUri, JObject jObj, string? message) : this(requestUri, jObj, message, null) { }
	public FakeAbstractException(Uri requestUri, JObject jObj, string? message, Exception? innerException) : base(requestUri, jObj, message, innerException) { }
}

[TestClass]
public class ctor2params
{
	[TestMethod]
	public void instantiate()
	{
		var uri = new Uri("http://test.com");
		var jObjString = "{\"a\":1}";
		var jObj = JObject.Parse(jObjString);
		var exception = new FakeAbstractException(uri, jObj);

		Assert.AreEqual(exception.RequestUri, uri.OriginalString);
		Assert.AreEqual(exception.JsonMessage, jObjString);
		Assert.AreEqual(exception.Message, $"Exception of type '{typeof(FakeAbstractException).FullName}' was thrown.");
		Assert.IsNull(exception.InnerException);
	}
}

[TestClass]
public class ctor3params
{
	[TestMethod]
	public void instantiate()
	{
		var uri = new Uri("http://test.com");
		var jObjString = "{\"a\":1}";
		var jObj = JObject.Parse(jObjString);
		var message = "my message";
		var exception = new FakeAbstractException(uri, jObj, message);

		Assert.AreEqual(exception.RequestUri, uri.OriginalString);
		Assert.AreEqual(exception.JsonMessage, jObjString);
		Assert.AreEqual(exception.Message, message);
		Assert.IsNull(exception.InnerException);
	}
}

[TestClass]
public class ctor4params
{
	[TestMethod]
	public void instantiate()
	{
		var uri = new Uri("http://test.com");
		var jObjString = "{\"a\":1}";
		var jObj = JObject.Parse(jObjString);
		var message = "my message";
		var innerException = new Exception("foo");
		var exception = new FakeAbstractException(uri, jObj, message, innerException);

		Assert.AreEqual(exception.RequestUri, uri.OriginalString);
		Assert.AreEqual(exception.JsonMessage, jObjString);
		Assert.AreEqual(exception.Message, message);
		Assert.AreEqual(exception.InnerException, innerException);
	}
}
