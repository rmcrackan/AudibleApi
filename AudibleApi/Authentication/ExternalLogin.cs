using System;
using System.Collections.Generic;
using AudibleApi.Authorization;
using Dinah.Core;

namespace AudibleApi.Authentication
{
	public class ExternalLogin
	{
		private Locale _locale { get; }

		public ExternalLogin(Locale locale) => _locale = ArgumentValidator.EnsureNotNull(locale, nameof(locale));

		/// <summary>
		/// Gives the url to login with external browser and prompt for result.
		/// Builds the url to login to Amazon as an Audible device.
		/// </summary>
		public string GetLoginUrl() => _locale.OAuthUrl();

		/// <summary>Retrieve tokens from response URL. Return an in-memory Identity object</summary>
		public Identity Login(string responseUrl) => new Identity(_locale, AccessToken.Parse(responseUrl), new Dictionary<string, string>());
	}
}
