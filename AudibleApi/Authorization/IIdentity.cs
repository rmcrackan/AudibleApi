using System;
using System.Collections.Generic;
using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json;

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

        IEnumerable<KeyValuePair<string, string>> Cookies { get; }

        string DeviceSerialNumber { get; }

		OAuth2 Authorization { get; }

		string DeviceType { get; }

        string AmazonAccountId { get; }

        string DeviceName { get; }

        string StoreAuthenticationCookie { get; }

        void Update(AccessToken accessToken);

		void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken, IEnumerable<KeyValuePair<string, string>> cookies, string deviceSerialNumber = null, string deviceType = null, string amazonAccountId = null, string deviceName = null, string storeAuthenticationCookie = null);

		void Invalidate();
	}
}
