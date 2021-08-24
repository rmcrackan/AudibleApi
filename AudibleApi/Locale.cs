using System;
using System.Collections.Generic;
using Dinah.Core;

namespace AudibleApi
{
	public class Locale : ValueObject
	{
		public static Locale Empty => new Locale();
		private Locale() => Name = "[empty]";

		public string Name { get; }

		public string LoginDomain { get; }
		public string CountryCode { get; }
		public string TopDomain { get; }
		public string MarketPlaceId { get; }
		public string Language { get; }

		public Locale(string name, string loginDomain, string countryCode, string topDomain, string marketPlaceId, string language)
		{
			if (name is null)
				throw new ArgumentNullException(nameof(name));

			if (loginDomain is null)
				throw new ArgumentNullException(nameof(loginDomain));
			if (countryCode is null)
				throw new ArgumentNullException(nameof(countryCode));
			if (topDomain is null)
				throw new ArgumentNullException(nameof(topDomain));
			if (marketPlaceId is null)
				throw new ArgumentNullException(nameof(marketPlaceId));
			if (language is null)
				throw new ArgumentNullException(nameof(language));

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException($"{name} may not be blank", nameof(name));

			if (string.IsNullOrWhiteSpace(loginDomain))
				throw new ArgumentException($"{loginDomain} may not be blank", nameof(loginDomain));
			if (string.IsNullOrWhiteSpace(countryCode))
				throw new ArgumentException($"{countryCode} may not be blank", nameof(countryCode));
			if (string.IsNullOrWhiteSpace(topDomain))
				throw new ArgumentException($"{topDomain} may not be blank", nameof(topDomain));
			if (string.IsNullOrWhiteSpace(marketPlaceId))
				throw new ArgumentException($"{marketPlaceId} may not be blank", nameof(marketPlaceId));
			if (string.IsNullOrWhiteSpace(language))
				throw new ArgumentException($"{language} may not be blank", nameof(language));

			Name = name.Trim();

			LoginDomain = loginDomain.Trim();
			CountryCode = countryCode.Trim();
			TopDomain = topDomain.Trim();
			MarketPlaceId = marketPlaceId.Trim();
			Language = language.Trim();
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Name;

			yield return LoginDomain;
			yield return CountryCode;
			yield return TopDomain;
			yield return MarketPlaceId;
			yield return Language;
		}

		public override string ToString() => Name;
	}
}
