﻿using System;
using System.Net.Http;
using AudibleApi.Authorization;
using Dinah.Core;
using Dinah.Core.Net;
using Dinah.Core.Net.Http;

namespace AudibleApi.Authentication
{
    public class LoginComplete : LoginResult
    {
        public Identity Identity { get; }

        public LoginComplete(Authenticate authenticate, string responseBody, Identity identity) : base(authenticate, responseBody)
            => Identity = identity ?? throw new ArgumentNullException(nameof(identity));
    }
}
