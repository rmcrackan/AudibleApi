using System;

namespace AudibleApi.Authorization
{
    // do not derive from ApiErrorException. those are erros returned by API calls. Login is pre-API calls
    public class RegistrationException : Exception
    {
        public RegistrationException() : base() { }
        public RegistrationException(string message) : base(message) { }
        public RegistrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
