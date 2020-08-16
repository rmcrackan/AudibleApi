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

		public string CountryCode { get; }
		public string Domain { get; }
		public string MarketPlaceId { get; }
		public string Language { get; }

		public Locale(string name, string countryCode, string domain, string marketPlaceId, string language)
		{
			if (name is null)
				throw new ArgumentNullException(nameof(name));

			if (countryCode is null)
				throw new ArgumentNullException(nameof(countryCode));
			if (domain is null)
				throw new ArgumentNullException(nameof(domain));
			if (marketPlaceId is null)
				throw new ArgumentNullException(nameof(marketPlaceId));
			if (language is null)
				throw new ArgumentNullException(nameof(language));

			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException($"{name} may not be blank", nameof(name));

			if (string.IsNullOrWhiteSpace(countryCode))
				throw new ArgumentException($"{countryCode} may not be blank", nameof(countryCode));
			if (string.IsNullOrWhiteSpace(domain))
				throw new ArgumentException($"{domain} may not be blank", nameof(domain));
			if (string.IsNullOrWhiteSpace(marketPlaceId))
				throw new ArgumentException($"{marketPlaceId} may not be blank", nameof(marketPlaceId));
			if (string.IsNullOrWhiteSpace(language))
				throw new ArgumentException($"{language} may not be blank", nameof(language));

			Name = name.Trim();

			CountryCode = countryCode.Trim();
			Domain = domain.Trim();
			MarketPlaceId = marketPlaceId.Trim();
			Language = language.Trim();
		}

		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Name;

			yield return CountryCode;
			yield return Domain;
			yield return MarketPlaceId;
			yield return Language;
		}

		public override string ToString() => Name;
	}
}
