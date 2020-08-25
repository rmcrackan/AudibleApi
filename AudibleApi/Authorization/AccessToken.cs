using System;
using System.Collections.Generic;
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
			validate(value);

			TokenValue = value;
			Expires = expires;
		}

		private static void validate(string value)
		{
			if (value is null)
				throw new ArgumentNullException(nameof(value));
			if (string.IsNullOrWhiteSpace(value))
				throw new ArgumentException(nameof(value));

			if (!value.StartsWith(REQUIRED_BEGINNING))
				throw new ArgumentException("Improperly formatted access token", nameof(value));
		}

		public void Invalidate() => Expires = DateTime.MinValue;

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
