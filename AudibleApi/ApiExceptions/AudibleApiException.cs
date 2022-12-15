using System;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public abstract class AudibleApiException : Exception
    {
        public string RequestUri { get; }
        // strore as string, not dynamic JObject. Serilog sometimes prints dynamic JObject as "[[[]]]"
        public string JsonMessage { get; protected init; }

        public AudibleApiException(Uri requestUri, JObject jsonMessage) : this(requestUri, jsonMessage, null, null) { }

        public AudibleApiException(Uri requestUri, JObject jsonMessage, string message) : this(requestUri, jsonMessage, message, null) { }

        public AudibleApiException(Uri requestUri, JObject jsonMessage, string message, Exception innerException) : this(requestUri?.OriginalString, jsonMessage, message, innerException) { }

        public AudibleApiException(string requestUri, JObject jsonMessage, string message, Exception innerException) : base(message, innerException)
        {
            RequestUri = requestUri;
            JsonMessage = jsonMessage?.ToString(Newtonsoft.Json.Formatting.None);
        }
    }
}
