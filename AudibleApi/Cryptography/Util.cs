using System;
using System.Net.Http;
using System.Text;
using AudibleApi.Authorization;
using Dinah.Core;

namespace AudibleApi.Cryptography
{
    public static class Util
    {
        private static readonly uint[] AmazonKey = new uint[] { 4169969034, 4087877101, 1706678977, 3681020276 };

        public static string EncryptMetadata(string metadata)
        {
            ArgumentValidator.EnsureNotNull(metadata, nameof(metadata));

            var metadataBts = Encoding.ASCII.GetBytes(metadata);

            var crc = Crc32.ComputeChecksum(metadataBts).ToString("X8");

            var clearString = crc + "#" + metadata;

            var clearBytes = Encoding.ASCII.GetBytes(clearString);

            var cipherBytes = XXTEA.Encrypt(clearBytes, AmazonKey);

            return "ECdITeCs:" + Convert.ToBase64String(cipherBytes);
        }

        public static void SignRequest(this HttpRequestMessage request, DateTime dateTime, AdpToken adpToken, PrivateKey privateKey)
        {
            validate(request, adpToken, privateKey);

            var signature = request.CalculateSignature(dateTime, adpToken, privateKey);

            request.Headers.Add("x-adp-token", adpToken.Value);
            request.Headers.Add("x-adp-alg", "SHA256withRSA:1.0");
            request.Headers.Add("x-adp-signature", signature);

            // possible? requires client_id ?
            // request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token.Value);
        }

        private static void validate(HttpRequestMessage request, AdpToken adpToken, PrivateKey privateKey)
		{
			ArgumentValidator.EnsureNotNull(request, nameof(request));
			ArgumentValidator.EnsureNotNull(adpToken, nameof(adpToken));
			ArgumentValidator.EnsureNotNull(privateKey, nameof(privateKey));
        }

        public static string CalculateSignature(this HttpRequestMessage request, DateTime dateTime, AdpToken adpToken, PrivateKey privateKey)
        {
            var method = request.Method.ToString().ToUpper();
            var url = request.RequestUri.OriginalString;
            var date = dateTime.ToRfc3339String();
            var body = request.Content?.ReadAsStringAsync().Result;

            var dataString = $"{method}\n{url}\n{date}\n{body}\n{adpToken.Value}";

            var signature = $"{privateKey.SignMessage(dataString)}:{date}";

            return signature;
        }
    }
}
