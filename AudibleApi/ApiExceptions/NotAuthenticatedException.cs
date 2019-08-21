using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public class NotAuthenticatedException : AudibleApiException
    {
        public NotAuthenticatedException(Uri requestUri, JObject jObj)
            : this(requestUri, jObj, null, null) { }

        public NotAuthenticatedException(Uri requestUri, JObject jObj, string message)
            : this(requestUri, jObj, message, null) { }

        public NotAuthenticatedException(Uri requestUri, JObject jObj, string message, Exception innerException)
            : base(requestUri, jObj, message, innerException) { }
    }
}
