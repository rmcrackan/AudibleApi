using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dinah.Core;
using Newtonsoft.Json;

namespace AudibleApi
{
	public static class Localization
	{
		public static Locale CurrentLocale { get; private set; }

		private static Dictionary<string, Locale> _locales { get; }

		static Localization()
		{
			const string filename = "locales.json";
			var contents = System.IO.File.ReadAllText(filename);
			_locales = JsonConvert.DeserializeObject<Dictionary<string, Locale>>(contents);

			// set default
			CurrentLocale = _locales["us"];
		}

		public static void SetLocale(string localeName)
		{
			localeName = localeName?.Trim().ToLower();
			if (localeName is null)
				throw new ArgumentNullException(nameof(localeName));
			if (string.IsNullOrWhiteSpace(localeName))
				throw new ArgumentException($"{localeName} may not be blank", nameof(localeName));
			if (!_locales.ContainsKey(localeName))
				throw new KeyNotFoundException($"Locale not found: {localeName}");
			CurrentLocale = _locales[localeName];
		}
	}
}
