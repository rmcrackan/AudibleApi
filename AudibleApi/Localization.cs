using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace AudibleApi;

public static class Localization
{
	/// <summary>
	/// Resolve a locale by internal name (e.g. <c>germany</c>) or country code (e.g. <c>de</c>).
	/// When matching by country code and both a modern and pre-amazon locale exist, prefers the modern one.
	/// Returns <see cref="Locale.Empty"/> when nothing matches.
	/// </summary>
	public static Locale Get(string? localeName)
	{
		if (string.IsNullOrWhiteSpace(localeName))
			return Locale.Empty;

		var key = localeName.Trim();

		var byName = Locales.FirstOrDefault(l => l.Name.Equals(key, StringComparison.OrdinalIgnoreCase));
		if (byName is not null)
			return byName;

		// Prefer non-pre-amazon (WithUsername == false) when several locales share a country code.
		var byCountryCode = Locales
			.Where(l => l.CountryCode.Equals(key, StringComparison.OrdinalIgnoreCase))
			.OrderBy(l => l.WithUsername)
			.FirstOrDefault();

		return byCountryCode ?? Locale.Empty;
	}

	public static ReadOnlyCollection<Locale> Locales { get; }

	// official locales are here: https://www.audible.com/ep/country-selector
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
				{"name", "brazil"},
				{"countryCode", "br"},
				{"topDomain", "com.br"},
				{"marketPlaceId", "A10J1VAYUDTYRN"},
				{"language", "pt-BR"}
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
				{"language", "it-IT"}
			},
			new JObject
			{
				{"name", "japan"},
				{"countryCode", "jp"},
				{"topDomain", "co.jp"},
				{"marketPlaceId", "A1QAP3MOU4173J"},
				{"language", "ja-JP"}
			},
			new JObject
			{
				{"name", "spain"},
				{"countryCode", "es"},
				{"topDomain", "es"},
				{"marketPlaceId", "ALMIKO4SZCSAR"},
				{"language", "es-ES"}
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

		Locales = locales.ToObject<ReadOnlyCollection<Locale>>()!;
	}
}
