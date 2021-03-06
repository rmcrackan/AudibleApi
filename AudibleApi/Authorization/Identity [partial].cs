﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace AudibleApi.Authorization
{
    /// <summary>
    /// In-memory handling of identity: Manages Audible API's state of authorization/authentication keys, tokens, and cookies. Maintains valid state
    /// </summary>
    public partial class Identity : IIdentity
	{
		public static Identity Empty => new Identity(Locale.Empty);

		public event EventHandler Updated;

		[JsonProperty]
		private string LocaleName { get; }
		[JsonIgnore]
		public Locale Locale => Localization.Get(LocaleName);

		[JsonIgnore]
		public bool IsValid { get; private set; }

		[JsonRequired]
        public AccessToken ExistingAccessToken { get; protected set; }

        public PrivateKey PrivateKey { get; private set; }

        public AdpToken AdpToken { get; private set; }

        public RefreshToken RefreshToken { get; private set; }
        
        // cookies are a list instead of Dictionary<string, string> b/c of duplicates
        protected List<KVP<string, string>> _cookies { private get; set; }
        public IEnumerable<KVP<string, string>> Cookies => _cookies.AsReadOnly();

		[JsonProperty]
		public string DeviceSerialNumber { get; private set; }

		[JsonProperty]
		public string DeviceType { get; private set; }

		[JsonProperty]
		public string AmazonAccountId { get; private set; }

		protected Identity() { }

		public Identity(Locale locale)
			: this(locale, AccessToken.Empty, new Dictionary<string, string>()) { }
		public Identity(Locale locale, AccessToken accessToken, IEnumerable<KeyValuePair<string, string>> cookies)
		{
			if (locale is null)
				throw new ArgumentNullException(nameof(locale));
			if (accessToken is null)
				throw new ArgumentNullException(nameof(accessToken));
			if (cookies is null)
				throw new ArgumentNullException(nameof(cookies));

			LocaleName = locale.Name;
			ExistingAccessToken = accessToken;
			_cookies = cookies.Select(kvp => new KVP<string, string> { Key = kvp.Key, Value = kvp.Value }).ToList();
		}

		public void Update(AccessToken accessToken)
		{
			if (accessToken is null)
				throw new ArgumentNullException(nameof(accessToken));

			ExistingAccessToken = accessToken;

			Updated?.Invoke(this, new EventArgs());
		}

		public void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken, string deviceSN = null, string deviceType = null, string amazonAccountId = null)
		{
			if (privateKey is null)
				throw new ArgumentNullException(nameof(privateKey));
			if (adpToken is null)
				throw new ArgumentNullException(nameof(adpToken));
			if (accessToken is null)
				throw new ArgumentNullException(nameof(accessToken));
			if (refreshToken is null)
				throw new ArgumentNullException(nameof(refreshToken));

			PrivateKey = new PrivateKey(privateKey);
			AdpToken = new AdpToken(adpToken);
			ExistingAccessToken = accessToken;
			RefreshToken = new RefreshToken(refreshToken);

			DeviceSerialNumber = deviceSN ?? string.Empty;
			DeviceType = deviceType ?? string.Empty;
			AmazonAccountId = amazonAccountId ?? string.Empty;

			Updated?.Invoke(this, new EventArgs());

			IsValid = true;
		}

		public void Invalidate()
		{
			AdpToken = null;
			RefreshToken = null;
			ExistingAccessToken.Invalidate();

			Updated?.Invoke(this, new EventArgs());

			IsValid = false;
		}
    }
}
