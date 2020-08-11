using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
	/// <summary>Persist settings to json file.
	/// If not using the optional JSONPath: Create file if it does not exist. Overwrite existing with identity tokens
	/// If using the optional JSONPath: the object of the path must be valid and existing in the file</summary>
	public class IdentityPersistent : IIdentity, IDisposable
    {
		public string Path { get; }
		public string JsonPath { get; }
		private IIdentity _identity { get; }

		/// <summary>uses path. create file if doesn't yet exist</summary>
		public IdentityPersistent(IIdentity identity, string path, string jsonPath = null)
		{
			_identity = identity ?? throw new ArgumentNullException(nameof(identity));
			_identity.Updated += saveFile;

			validatePath(path);

			Path = path;

			if (!string.IsNullOrWhiteSpace(jsonPath))
				JsonPath = jsonPath.Trim();

			saveFile(this, null);
		}

		/// <summary>load from existing file</summary>
		public IdentityPersistent(string path, string jsonPath = null)
        {
			validatePath(path);

			Path = path;

			if (!string.IsNullOrWhiteSpace(jsonPath))
				JsonPath = jsonPath.Trim();

			_identity = loadFromFile();
            _identity.Updated += saveFile;
        }

        private void validatePath(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
			if (string.IsNullOrWhiteSpace(path))
				throw new ArgumentException("Path cannot be blank", nameof(path));
		}

        private IIdentity loadFromFile()
        {
            var json = File.ReadAllText(Path);
			var identity = Identity.FromJson(json, JsonPath);

			if (identity is null)
				throw new FormatException("File was not in a format able to be imported");

			if (identity.ExistingAccessToken is null ||
				string.IsNullOrWhiteSpace(identity.ExistingAccessToken.TokenValue))
				throw new FormatException("AccessToken is not present in file");

			return identity;
        }

        private object _locker { get; } = new object();
		private void saveFile(object sender, EventArgs e)
		{
			lock (_locker)
			{
				if (JsonPath is null)
				{
					File.WriteAllText(Path, JsonConvert.SerializeObject(_identity, Formatting.Indented, new Identity.AccessTokenConverter()));
					return;
				}

				// path must
				// - exist
				// - have valid jsonPath match

				var contents = File.ReadAllText(Path);
				var allToken = JObject.Parse(contents);
				var pathToken = allToken.SelectToken(JsonPath);

				if (pathToken is null)
					throw new JsonSerializationException($"No match found at JSONPath: {JsonPath}");

				// load existing identity into JObject
				var settings = new JsonSerializerSettings();
				settings.Converters.Add(new Identity.AccessTokenConverter());
				var serializer = JsonSerializer.Create(settings);
				var idJObj = JObject.FromObject(_identity, serializer);

				// replace. this propgates to 'allToken'
				pathToken.Replace(idJObj);

				var allSer = JsonConvert.SerializeObject(allToken, Formatting.Indented, new Identity.AccessTokenConverter());
				File.WriteAllText(Path, allSer);
			}
		}

		public void Dispose()
			=> _identity.Updated -= saveFile;

		#region delegate to _identity
		public event EventHandler Updated
        {
            add => _identity.Updated += value;
            remove => _identity.Updated -= value;
        }

		public bool IsValid => _identity.IsValid;

        public AccessToken ExistingAccessToken => _identity.ExistingAccessToken;

        public PrivateKey PrivateKey => _identity.PrivateKey;

        public AdpToken AdpToken => _identity.AdpToken;

        public RefreshToken RefreshToken => _identity.RefreshToken;

        public IEnumerable<KVP<string, string>> Cookies => _identity.Cookies;

		public void Update(AccessToken accessToken) => _identity.Update(accessToken);

		public void Update(PrivateKey privateKey, AdpToken adpToken, AccessToken accessToken, RefreshToken refreshToken) => _identity.Update(privateKey, adpToken, accessToken, refreshToken);

		public void Invalidate() => _identity.Invalidate();
		#endregion
	}
}
