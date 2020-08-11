using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
    /// <summary>This is the same as KeyValuePair<K, V> except that newtonsoft doesn't play nicely with that native struct</summary>
    public class KVP<K, V>
    {
        public K Key { get; set; }
        public V Value { get; set; }

		public override string ToString() => $"[{Key}={Value}]";
	}

	public static class KVPExtensions
	{
		public static List<KeyValuePair<string, string>> ToKeyValuePair(this IEnumerable<KVP<string, string>> pairs)
		{
			if (pairs is null)
				throw new ArgumentNullException(nameof(pairs));
			return pairs
				.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value))
				.ToList();
		}
	}

    public partial class Identity
    {
        [JsonConstructor]
        protected Identity(AccessToken existingAccessToken, PrivateKey privateKey, AdpToken adpToken, RefreshToken refreshToken, List<KVP<string, string>> cookies)
		{
			IsValid = true;

			if (existingAccessToken is null)
				throw new ArgumentNullException(nameof(existingAccessToken));

			ExistingAccessToken = existingAccessToken;

			if (privateKey is null)
				IsValid = false;
			else
				PrivateKey = new PrivateKey(privateKey);

			if (adpToken is null)
				IsValid = false;
			else
				AdpToken = new AdpToken(adpToken);

			if (refreshToken is null)
				IsValid = false;
			else
				RefreshToken = new RefreshToken(refreshToken);

			_cookies = cookies;
		}

        public static Identity FromJson(string json, string jsonPath = null)
        {
			if (jsonPath != null)
			{
				var jToken = JObject.Parse(json).SelectToken(jsonPath);

				if (jToken is null)
					throw new JsonSerializationException($"No match found at JSONPath: {jsonPath}");

				json = jToken.ToString(Formatting.Indented);
			}

			var settings = new JsonSerializerSettings();
            settings.Converters.Add(new AccessTokenConverter());
            var idMgr = JsonConvert.DeserializeObject<Identity>(json, settings);

			if (idMgr is null)
				throw new FormatException("Could not deserialize json: " + json);

			return idMgr;
        }

        // https://stackoverflow.com/a/23017892
        internal class AccessTokenConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType)
                => objectType == typeof(AccessToken);

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var jo = JObject.Load(reader);

                var accessTokenValue = (string)jo["TokenValue"];
                var expires = (DateTime)jo["Expires"];

                var result = new AccessToken(accessTokenValue, expires);
                return result;
            }

            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                => throw new NotImplementedException();
        }
    }
}
