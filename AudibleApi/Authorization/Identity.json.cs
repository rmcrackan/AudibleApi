using System;
using System.Collections.Generic;
using System.Linq;
using AudibleApi.Cryptography;
using Dinah.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AudibleApi.Authorization
{
    public partial class Identity
	{
        [JsonConstructor]
        protected Identity(string localeName, AccessToken existingAccessToken, PrivateKey privateKey, AdpToken adpToken, RefreshToken refreshToken, List<KeyValuePair<string, string>> cookies)
		{
			IsValid = true;

			if (string.IsNullOrWhiteSpace(localeName))
				IsValid = false;
			else
				LocaleName = localeName.Trim();			

			ExistingAccessToken = ArgumentValidator.EnsureNotNull(existingAccessToken, nameof(existingAccessToken));

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
			=> Dinah.Core.JsonHelper.FromJson<Identity>(json, jsonPath, GetJsonSerializerSettings());

		public static JsonSerializerSettings GetJsonSerializerSettings()
		{
			var settings = new JsonSerializerSettings();
			settings.Converters.Add(new AccessTokenConverter());
			return settings;
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
