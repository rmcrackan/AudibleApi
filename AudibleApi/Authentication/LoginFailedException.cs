using System;

namespace AudibleApi.Authentication
{
    // do not derive from ApiErrorException. those are erros returned by API calls. Login is pre-API calls
    public class LoginFailedException : Exception
    {
        public LoginFailedException() : base() { }
        public LoginFailedException(string message) : base(message) { }
        public LoginFailedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
