using Dinah.Core;
using Dinah.Core.Security;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AudibleApi.Authorization
{
	[DebuggerDisplay("{ToString(),nq}")]
	public class AccessToken : ValueObject
	{
		private const string REQUIRED_BEGINNING = "Atna|";

		public static AccessToken Empty => new AccessToken(REQUIRED_BEGINNING, DateTime.MinValue);
		public static AccessToken EmptyFuture => new AccessToken(REQUIRED_BEGINNING, DateTime.MaxValue);

		public SecretString TokenValue { get; }
		public DateTime Expires { get; private set; }

		public AccessToken(SecretString value, DateTime expires)
		{
			var raw = value.Reveal();
			ArgumentValidator.EnsureNotNullOrWhiteSpace(raw, nameof(value));
			if (!raw.StartsWith(REQUIRED_BEGINNING))
				throw new ArgumentException("Improperly formatted access token", nameof(value));

			TokenValue = value;

			// Login returns current time. Expiration is actually 1 hour later. By setting this as current time, we force initial registration
			Expires = expires;
		}

		public void Invalidate() => Expires = DateTime.MinValue;

		public static AccessToken? Parse(Uri uri)
			=> uri.IsAbsoluteUri
			? ParseQuery(uri?.Query)
			: Parse(uri?.OriginalString);

		public static AccessToken? Parse(string? url) => ParseQuery(url?.Split('?').Last());

		public static AccessToken? ParseQuery(string? urlQueryPortion)
		{
			if (string.IsNullOrWhiteSpace(urlQueryPortion))
				return null;

			// keys and values are already url-decoded
			var parameters = System.Web.HttpUtility.ParseQueryString(urlQueryPortion);

			const string tokenKey = "openid.oa2.access_token";
			if (!parameters.AllKeys.Contains(tokenKey))
				return null;

			const string timeKey = "openid.pape.auth_time";
			if (!parameters.AllKeys.Contains(timeKey))
				return null;

			var expires = parameters[timeKey] ?? throw new RegistrationException("Expiration time missing from query string");
			var token = parameters[tokenKey] ?? throw new RegistrationException("Access token missing from query string");
			return new AccessToken(token, DateTime.Parse(expires));
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return TokenValue;
			yield return Expires;
		}

		/// <summary>
		/// The token itself, non-null because the constructor validated it. A method rather than a property so
		/// that reflective logging cannot reach it.
		/// </summary>
		public string Reveal() => TokenValue.Reveal()!;

		public override string ToString()
			=> $"{SecretString.Redact(nameof(AccessToken), TokenValue.Reveal())}. Expires={Expires}";
	}
}
