using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace AudibleApi
{
	public static class Localization
	{
		public static Locale Get(string localeName) => _locales.SingleOrDefault(l => l.Name == localeName) ?? Locale.Empty;

		public static ReadOnlyCollection<Locale> Locales => _locales.AsReadOnly();

		private static List<Locale> _locales { get; }
			
			// nuget must do all this in code since it will not contain the json file.
			// keep this list AND locales.json up to date (just in case)
			= new List<Locale>
		{
			new Locale(name: "us", loginDomain: "amazon", countryCode: "us", topDomain: "com", marketPlaceId: "AF2M0KC94RCEA", language: "en-US"),
			new Locale(name: "uk", loginDomain: "amazon", countryCode: "uk", topDomain: "co.uk", marketPlaceId: "A2I9A3Q2GNFNGQ", language: "en-GB"),
			new Locale(name: "australia", loginDomain: "amazon", countryCode: "au", topDomain: "com.au", marketPlaceId: "AN7EY7DTAW63G", language: "en-AU"),
			new Locale(name: "canada", loginDomain: "amazon", countryCode: "ca", topDomain: "ca", marketPlaceId: "A2CQZ5RBY40XE", language: "en-CA"),
			new Locale(name: "france", loginDomain: "amazon", countryCode: "fr", topDomain: "fr", marketPlaceId: "A2728XDNODOQ8T", language: "fr-FR"),
			new Locale(name: "germany", loginDomain: "amazon", countryCode: "de", topDomain: "de", marketPlaceId: "AN7V1F1VY261K", language: "de-DE"),
			new Locale(name: "india", loginDomain: "amazon", countryCode: "in", topDomain: "in", marketPlaceId: "AJO3FBRUE6J4S", language: "en-IN"),
			new Locale(name: "italy", loginDomain: "amazon", countryCode: "it", topDomain: "it", marketPlaceId: "A2N7FU2W2BU2ZC", language: "it"),
			new Locale(name: "japan", loginDomain: "amazon", countryCode: "jp", topDomain: "co.jp", marketPlaceId: "A1QAP3MOU4173J", language: "ja"),
			new Locale(name: "spain", loginDomain: "amazon", countryCode: "es", topDomain: "es", marketPlaceId: "ALMIKO4SZCSAR", language: "es"),
			new Locale(name: "pre-amazon - germany", loginDomain: "audible", countryCode: "de", topDomain: "de", marketPlaceId: "AN7V1F1VY261K", language: "de-DE"),
			new Locale(name: "pre-amazon - us", loginDomain: "audible", countryCode: "us", topDomain: "com", marketPlaceId: "AF2M0KC94RCEA", language: "en-US"),
			new Locale(name: "pre-amazon - uk", loginDomain: "audible", countryCode: "uk", topDomain: "co.uk", marketPlaceId: "A2I9A3Q2GNFNGQ", language: "en-GB"),
		};

		static Localization()
		{
			//// nuget must do all this in code (above) since it will not contain the json file:

			// const string filename = "locales.json";
			// var contents = System.IO.File.ReadAllText(filename);
			// _locales = JsonConvert.DeserializeObject<List<Locale>>(contents);
		}
	}
}
