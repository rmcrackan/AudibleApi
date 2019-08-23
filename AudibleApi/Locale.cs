using System;
using System.Collections.Generic;
using Dinah.Core;

namespace AudibleApi
{
	public class Locale : ValueObject
	{
		public string CountryCode { get; }
		public string Domain { get; }
		public string MarketPlaceId { get; }
		public string Language { get; }

		public Locale(string countryCode, string domain, string marketPlaceId, string language)
		{
			if (countryCode is null)
				throw new ArgumentNullException(nameof(countryCode));
			if (domain is null)
				throw new ArgumentNullException(nameof(domain));
			if (marketPlaceId is null)
				throw new ArgumentNullException(nameof(marketPlaceId));
			if (language is null)
				throw new ArgumentNullException(nameof(language));

			if (string.IsNullOrWhiteSpace(countryCode))
				throw new ArgumentException($"{countryCode} may not be blank", nameof(countryCode));
			if (string.IsNullOrWhiteSpace(domain))
				throw new ArgumentException($"{domain} may not be blank", nameof(domain));
			if (string.IsNullOrWhiteSpace(marketPlaceId))
				throw new ArgumentException($"{marketPlaceId} may not be blank", nameof(marketPlaceId));
			if (string.IsNullOrWhiteSpace(language))
				throw new ArgumentException($"{language} may not be blank", nameof(language));

			CountryCode = countryCode.Trim();
			Domain = domain.Trim();
			MarketPlaceId = marketPlaceId.Trim();
			Language = language.Trim();
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return CountryCode;
			yield return Domain;
			yield return MarketPlaceId;
			yield return Language;
		}
	}
}
