using System;
using Dinah.Core;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
    public static class RestMessageValidator
    {
        public static void ThrowStrongExceptionsIfInvalid(string message, Uri requestUri)
        {
            if (message is null)
                return;

            // Audible API returned content strings SHOULD always be json but there could be exceptions I'm not aware of.
            // Errors are always returned as json.
            // This method is not intended to be json validation. only finding out just enough to see if it's amazon's json error message. then throwing strong exceptions where needed
            if (!message.Trim().StartsWith("{") && !message.Trim().StartsWith("["))
                return;

            var jObject = JObject.Parse(message);

            if (jObject.TryGetValue("message", out JToken messageToken))
            {
                var jsonMessage = messageToken.ToString();

                if (jsonMessage.ContainsInsensitive("could not be authenticated"))
                    throw new NotAuthenticatedException(requestUri, jObject, jsonMessage);

                if (jsonMessage.ContainsInsensitive("Invalid response group"))
                    throw new InvalidResponseException(requestUri, jObject, jsonMessage);

                if (jsonMessage.ContainsInsensitive("validation error detected") ||
                    jsonMessage.ContainsInsensitive("validation errors detected"))
                    throw new ValidationErrorException(requestUri, jObject, jsonMessage);

                // yes, this is a real error message with a 500 internal server error
                if (jsonMessage.EqualsInsensitive("Whoops! Looks like something went wrong."))
                    throw new ApiErrorException(requestUri, jObject, jsonMessage);
            }

            if (jObject.TryGetValue("error", out JToken errorToken))
            {
                var error = errorToken.ToString();

                if (error.EqualsInsensitive("InvalidValue"))
                    throw new InvalidValueException(requestUri, jObject, error);

                // else, api error of unknown type
                throw new ApiErrorException(requestUri, jObject, error);
            }

            if (jObject.TryGetValue("error_code", out JToken errorCodeToken))
            {
                var error = errorCodeToken.ToString();
                throw new ApiErrorException(requestUri, jObject, error);
            }
        }
    }
}
