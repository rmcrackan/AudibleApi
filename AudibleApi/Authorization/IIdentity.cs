﻿using System;
using System.Collections.Generic;
using Dinah.Core;

namespace AudibleApi.Authorization
{
    /// <summary>
    /// Manages Audible API's state of authorization/authentication keys, tokens, and cookies
    /// </summary>
    public interface IIdentity : IUpdatable
    {
        Locale Locale { get; }

        /// <summary>has all authorization tokens/keys</summary>
        bool IsValid { get; }

        AccessToken ExistingAccessToken { get; }

		PrivateKey PrivateKey { get; }

        AdpToken AdpToken { get; }

        RefreshToken RefreshToken { get; }

        IEnumerable<KVP<string, string>> Cookies { get; }

        string DeviceSerialNumber { get; }

        string DeviceType { get; }

        string AmazonAccountId { get; }

        void Update(AccessToken accessToken);

		void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken, string deviceSN = null, string deviceType = null, string amazonAccountId = null);

		void Invalidate();
	}
}
