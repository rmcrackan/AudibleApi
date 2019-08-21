using System;
using System.IO;
using Newtonsoft.Json;

namespace AudibleApiClientExample
{
	public class ClientSettings
	{
		private string _identityFilePath;

		/// <summary>
		/// path to use for tokens, keys, and cookies to communicate with audible api
		/// </summary>
		public string IdentityFilePath
		{
			get => _identityFilePath;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();
				_identityFilePath = value;
				Save();
			}
		}

		private string _localeCountryCode = "us";
		public string LocaleCountryCode
		{
			get => _localeCountryCode;
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					throw new ArgumentNullException();
				_localeCountryCode = value;
				Save();
			}
		}

		private string filepath;
		public static ClientSettings FromFile(string filename)
		{
			var contents = File.ReadAllText(filename);
			var clientSettings = JsonConvert.DeserializeObject<ClientSettings>(contents);
			clientSettings.filepath = filename;
			clientSettings.doSave = true;
			return clientSettings;
		}

		bool doSave;
		private void Save()
		{
			if (!doSave)
				return;

			var json = JsonConvert.SerializeObject(this, Formatting.Indented);
			File.WriteAllText(filepath, json);
		}
	}
}
