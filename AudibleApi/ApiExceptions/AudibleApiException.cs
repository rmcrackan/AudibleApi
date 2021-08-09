using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public abstract class AudibleApiException : Exception
    {
        public Uri RequestUri { get; }
        public JObject JsonMessage { get; }

        public AudibleApiException(Uri requestUri, JObject jsonMessage) : this(requestUri, jsonMessage, null, null) { }

        public AudibleApiException(Uri requestUri, JObject jsonMessage, string message) : this(requestUri, jsonMessage, message, null) { }

        public AudibleApiException(Uri requestUri, JObject jsonMessage, string message, Exception innerException) : base(message, innerException)
        {
            RequestUri = requestUri;
            JsonMessage = jsonMessage;
        }
        public void LogException(Action<Exception, string, string> logMethod)
            => logMethod(this, $"{Message} {{@DebugInfo}}", JsonMessage.ToString(Newtonsoft.Json.Formatting.None));
    }
}
