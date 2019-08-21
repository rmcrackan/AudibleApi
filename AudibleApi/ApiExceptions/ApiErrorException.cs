using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    /// <summary>
    /// Error returned by API calls
    /// </summary>
    public class ApiErrorException : AudibleApiException
    {
        public ApiErrorException(Uri requestUri, JObject jObj)
            : this(requestUri, jObj, null, null) { }

        public ApiErrorException(Uri requestUri, JObject jObj, string message)
            : this(requestUri, jObj, message, null) { }

        public ApiErrorException(Uri requestUri, JObject jObj, string message, Exception innerException)
            : base(requestUri, jObj, message, innerException) { }
    }
}
