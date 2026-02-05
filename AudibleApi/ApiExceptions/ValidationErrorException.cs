using Newtonsoft.Json.Linq;
using System;

namespace AudibleApi;

public class ValidationErrorException : AudibleApiException
{
	public ValidationErrorException(Uri? requestUri, JObject? jObj)
		: base(requestUri, jObj, null, null) { }

	public ValidationErrorException(Uri? requestUri, JObject? jObj, string? message)
		: base(requestUri, jObj, message, null) { }

	public ValidationErrorException(Uri? requestUri, JObject? jObj, string? message, Exception? innerException)
		: base(requestUri, jObj, message, innerException) { }
}
