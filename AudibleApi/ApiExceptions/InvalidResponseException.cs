using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public class InvalidResponseException : AudibleApiException
    {
        public InvalidResponseException(Uri requestUri, JObject jObj)
            : this(requestUri, jObj, null, null) { }

        public InvalidResponseException(Uri requestUri, JObject jObj, string message)
            : this(requestUri, jObj, message, null) { }

        public InvalidResponseException(Uri requestUri, JObject jObj, string message, Exception innerException)
            : base(requestUri, jObj, message, innerException) { }
    }
}
