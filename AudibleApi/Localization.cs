using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;

namespace AudibleApi
{
	public static class Localization
	{
		public static ReadOnlyCollection<Locale> Locales => _locales.AsReadOnly();

		public static Locale CurrentLocale { get; private set; }

		private static List<Locale> _locales { get; }

		static Localization()
		{
			const string filename = "locales.json";
			var contents = System.IO.File.ReadAllText(filename);
			_locales = JsonConvert.DeserializeObject<List<Locale>>(contents);

			// set default
			CurrentLocale = _locales.Single(l => l.Name == "us");
		}

		public static void SetLocale(string name)
		{
			name = name?.Trim().ToLower();

			if (name is null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException($"{name} may not be blank", nameof(name));

			var single = _locales.SingleOrDefault(l => l.Name == name);

			CurrentLocale = single ?? throw new InvalidOperationException($"Locale sequence contains no matching element: {name}");
		}
	}
}
