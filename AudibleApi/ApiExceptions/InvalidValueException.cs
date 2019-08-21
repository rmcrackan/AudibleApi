using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public class InvalidValueException : AudibleApiException
    {
        public InvalidValueException(Uri requestUri, JObject jObj)
            : this(requestUri, jObj, null, null) { }

        public InvalidValueException(Uri requestUri, JObject jObj, string message)
            : this(requestUri, jObj, message, null) { }

        public InvalidValueException(Uri requestUri, JObject jObj, string message, Exception innerException)
            : base(requestUri, jObj, message, innerException) { }
    }
}
