using Newtonsoft.Json.Linq;
using System;

namespace AudibleApi;

public class InvalidResponseException : AudibleApiException
{
	public InvalidResponseException(Uri? requestUri, JObject? jObj)
		: base(requestUri, jObj, null, null) { }

	public InvalidResponseException(Uri? requestUri, JObject? jObj, string? message)
		: base(requestUri, jObj, message, null) { }

	public InvalidResponseException(Uri? requestUri, JObject? jObj, string? message, Exception? innerException)
		: base(requestUri, jObj, message, innerException) { }
}
