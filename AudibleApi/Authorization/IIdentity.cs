using System;
using System.Collections.Generic;
using Dinah.Core;

namespace AudibleApi.Authorization
{
    /// <summary>
    /// Manages Audible API's state of authorization/authentication keys, tokens, and cookies
    /// </summary>
    public interface IIdentity
    {
        event EventHandler Updated;

        Locale Locale { get; }

        /// <summary>has all authorization tokens/keys</summary>
        bool IsValid { get; }

		PrivateKey PrivateKey { get; }

        AdpToken AdpToken { get; }

        RefreshToken RefreshToken { get; }

        IEnumerable<KVP<string, string>> Cookies { get; }

        AccessToken ExistingAccessToken { get; }

		void Update(AccessToken accessToken);

		void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken);

		void Invalidate();
	}
}
