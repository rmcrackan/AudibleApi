using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace AudibleApi
{
	public static class Localization
	{
		public static Locale Get(string localeName) => Locales.SingleOrDefault(l => l.Name == localeName) ?? Locale.Empty;

		public static IReadOnlyList<Locale> Locales { get; }

		static Localization()
		{
			var locales = new JArray
			{
				new JObject
				{
					{"name", "us"},
					{"countryCode", "us"},
					{"topDomain", "com"},
					{"marketPlaceId", "AF2M0KC94RCEA"},
					{"language", "en-US"}
				},
				new JObject
				{
					{"name", "uk"},
					{"countryCode", "uk"},
					{"topDomain", "co.uk"},
					{"marketPlaceId", "A2I9A3Q2GNFNGQ"},
					{"language", "en-GB"}
				},
				new JObject
				{
					{"name", "australia"},
					{"countryCode", "au"},
					{"topDomain", "com.au"},
					{"marketPlaceId", "AN7EY7DTAW63G"},
					{"language", "en-AU"}
				},
				new JObject
				{
					{"name", "canada"},
					{"countryCode", "ca"},
					{"topDomain", "ca"},
					{"marketPlaceId", "A2CQZ5RBY40XE"},
					{"language", "en-CA"}
				},
				new JObject
				{
					{"name", "france"},
					{"countryCode", "fr"},
					{"topDomain", "fr"},
					{"marketPlaceId", "A2728XDNODOQ8T"},
					{"language", "fr-FR"}
				},
				new JObject
				{
					{"name", "germany"},
					{"countryCode", "de"},
					{"topDomain", "de"},
					{"marketPlaceId", "AN7V1F1VY261K"},
					{"language", "de-DE"}
				},
				new JObject
				{
					{"name", "india"},
					{"countryCode", "in"},
					{"topDomain", "in"},
					{"marketPlaceId", "AJO3FBRUE6J4S"},
					{"language", "en-IN"}
				},
				new JObject
				{
					{"name", "italy"},
					{"countryCode", "it"},
					{"topDomain", "it"},
					{"marketPlaceId", "A2N7FU2W2BU2ZC"},
					{"language", "it"}
				},
				new JObject
				{
					{"name", "japan"},
					{"countryCode", "jp"},
					{"topDomain", "co.jp"},
					{"marketPlaceId", "A1QAP3MOU4173J"},
					{"language", "ja"}
				},
				new JObject
				{
					{"name", "spain"},
					{"countryCode", "es"},
					{"topDomain", "es"},
					{"marketPlaceId", "ALMIKO4SZCSAR"},
					{"language", "es"}
				},
				new JObject
				{
					{"name", "pre-amazon - germany"},
					{"countryCode", "de"},
					{"topDomain", "de"},
					{"marketPlaceId", "AN7V1F1VY261K"},
					{"language", "de-DE"},
					{"withUsername", true}
				},
				new JObject
				{
					{"name", "pre-amazon - us"},
					{"countryCode", "us"},
					{"topDomain", "com"},
					{"marketPlaceId", "AF2M0KC94RCEA"},
					{"language", "en-US"},
					{"withUsername", true}
				},
				new JObject
				{
					{"name", "pre-amazon - uk"},
					{"countryCode", "uk"},
					{"topDomain", "co.uk"},
					{"marketPlaceId", "A2I9A3Q2GNFNGQ"},
					{"language", "en-GB"},
					{"withUsername", true}
				}
			};

			Locales = locales.ToObject<IReadOnlyList<Locale>>();
		}
	}
}
