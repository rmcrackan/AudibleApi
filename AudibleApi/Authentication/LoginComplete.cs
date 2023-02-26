using System;
using AudibleApi.Authorization;

namespace AudibleApi.Authentication
{
    internal class LoginComplete : LoginResult
    {
        public Identity Identity { get; }

        public LoginComplete(Authenticate authenticate, string responseBody, Identity identity) : base(authenticate, responseBody)
            => Identity = identity ?? throw new ArgumentNullException(nameof(identity));
    }
}
