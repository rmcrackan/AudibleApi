using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace AudibleApi
{
	public static class Localization
	{
		public static Locale Get(string localeName) => _locales.SingleOrDefault(l => l.Name == localeName) ?? Locale.Empty;

		//// doesn't seem to be needed. if I'm wrong: uncomment
		//public static ReadOnlyCollection<Locale> Locales => _locales.AsReadOnly();
		private static List<Locale> _locales { get; }

		static Localization()
		{
			const string filename = "locales.json";
			var contents = System.IO.File.ReadAllText(filename);
			_locales = JsonConvert.DeserializeObject<List<Locale>>(contents);
		}
	}
}
