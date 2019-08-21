using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public class ValidationErrorException : AudibleApiException
    {
        public ValidationErrorException(Uri requestUri, JObject jObj)
            : this(requestUri, jObj, null, null) { }

        public ValidationErrorException(Uri requestUri, JObject jObj, string message)
            : this(requestUri, jObj, message, null) { }

        public ValidationErrorException(Uri requestUri, JObject jObj, string message, Exception innerException)
            : base(requestUri, jObj, message, innerException) { }
    }
}
