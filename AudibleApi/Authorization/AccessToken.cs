using System;
using System.Collections.Generic;
using System.Linq;
using Dinah.Core;

namespace AudibleApi.Authorization
{
    public class AccessToken : ValueObject
	{
		private const string REQUIRED_BEGINNING = "Atna|";

		public static AccessToken Empty => new AccessToken(REQUIRED_BEGINNING, DateTime.MinValue);
		public static AccessToken EmptyFuture => new AccessToken(REQUIRED_BEGINNING, DateTime.MaxValue);

		public string TokenValue { get; }
        public DateTime Expires { get; private set; }

        public AccessToken(string value, DateTime expires)
		{
			ArgumentValidator.EnsureNotNullOrWhiteSpace(value, nameof(value));
			if (!value.StartsWith(REQUIRED_BEGINNING))
				throw new ArgumentException("Improperly formatted access token", nameof(value));

			TokenValue = value;

			// Login returns current time. Expiration is actually 1 hour later. By setting this as current time, we force initial registration
			Expires = expires;
		}

		public void Invalidate() => Expires = DateTime.MinValue;

		public static AccessToken Parse(Uri uri)
			=> uri.IsAbsoluteUri
			? ParseQuery(uri?.Query)
			: Parse(uri?.OriginalString);

		public static AccessToken Parse(string url) => ParseQuery(url?.Split('?').Last());

		public static AccessToken ParseQuery(string urlQueryPortion)
        {
			if (string.IsNullOrWhiteSpace(urlQueryPortion))
				return null;

            // keys and values are already url-decoded
            var parameters = System.Web.HttpUtility.ParseQueryString(urlQueryPortion);

            var tokenKey = "openid.oa2.access_token";
            if (!parameters.AllKeys.Contains(tokenKey))
                return null;

            var timeKey = "openid.pape.auth_time";
            if (!parameters.AllKeys.Contains(timeKey))
                return null;

            var expires = parameters[timeKey];
			return new AccessToken(parameters[tokenKey], DateTime.Parse(expires));
        }

		protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return TokenValue;
            yield return Expires;
        }

		public override string ToString()
			=> "AccessToken. "
			+ $"Value={TokenValue}. "
			+ $"Expires={Expires}";
	}
}
