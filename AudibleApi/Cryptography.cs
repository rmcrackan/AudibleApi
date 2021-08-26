using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using AudibleApi.Authorization;
using Dinah.Core;

namespace AudibleApi
{
    public static partial class Cryptography
    {
        public static string EncryptMetadata(string metadata)
        {
            if (metadata is null)
                throw new ArgumentNullException(nameof(metadata));

            var engine = new Jint.Engine()
                .SetValue("log", new Action<object>(Console.WriteLine))
                .Execute(Javascript);

            var val = engine
                .Execute($@"
                    var metadata = '{metadata}';
                    var update_val = update(metadata);
                    var hex_hash = format(update_val);
                    var str_to_parse = hex_hash + '#' + metadata;
                    var parsed = parse(str_to_parse);
                    var evaled = evaluate(parsed);

                    // return
                    'ECdITeCs:' + evaled
                ")
                .GetCompletionValue()
                .ToObject()
                .ToString();

            return val;
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
            if (request is null)
                throw new ArgumentNullException(nameof(request));
            if (adpToken is null)
                throw new ArgumentNullException(nameof(adpToken));
            if (privateKey is null)
                throw new ArgumentNullException(nameof(privateKey));
        }

        public static string CalculateSignature(this HttpRequestMessage request, DateTime dateTime, AdpToken adpToken, PrivateKey privateKey)
        {
            var method = request.Method.ToString().ToUpper();
            var url = request.RequestUri.OriginalString;
            var date = dateTime.ToRfc3339String();
            var body = request.Content?.ReadAsStringAsync().Result;

            var dataString = $"{method}\n{url}\n{date}\n{body}\n{adpToken.Value}";

            var signedBytes = signSha256(privateKey.Value, dataString);
            // as string: var signedString = Encoding.UTF8.GetString(signedBytes);

            var encoded = Convert.ToBase64String(signedBytes);
            var signature = $"{encoded}:{date}";

            return signature;
        }

        private static byte[] signSha256(string private_key, string dataString)
        {
            var dataBytes = Encoding.UTF8.GetBytes(dataString);

            using var sha256 = new SHA256Managed();
            using var rsaCSP = CreateRsaProviderFromPrivateKey(private_key);
            var digestion = sha256.ComputeHash(dataBytes);
            // as string: digestion.Select(x => $"{x:x2}").Aggregate("", (a, b) => a + b);
            var oid = CryptoConfig.MapNameToOID("SHA256");
            return rsaCSP.SignHash(digestion, oid);
        }

        // https://stackoverflow.com/a/32150537
        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            privateKey = privateKey
                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace("\\n", "")
                .Trim();

            var asn1EncodedPrivateKey = Convert.FromBase64String(privateKey);
            var keySequence = Asn1Value.Parse(asn1EncodedPrivateKey);

            var RSA = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();

            //Guess the key size based on modulus size. Assume key size is multiple of 128 bits.
            int keySizeGuess = (int)Math.Round(keySequence.Children[1].Value.Length / 4d, 0) * 32;

            RSAparams.Modulus = ValidateInt(keySequence.Children[1].Value, keySizeGuess, 8);
            RSAparams.Exponent = keySequence.Children[2].Value;
            RSAparams.D = ValidateInt(keySequence.Children[3].Value, keySizeGuess, 8);
            RSAparams.P = ValidateInt(keySequence.Children[4].Value, keySizeGuess, 16);
            RSAparams.Q = ValidateInt(keySequence.Children[5].Value, keySizeGuess, 16);
            RSAparams.DP = ValidateInt(keySequence.Children[6].Value, keySizeGuess, 16);
            RSAparams.DQ = ValidateInt(keySequence.Children[7].Value, keySizeGuess, 16);
            RSAparams.InverseQ = ValidateInt(keySequence.Children[8].Value, keySizeGuess, 16);

            RSA.ImportParameters(RSAparams);
            return RSA;
        }

        /// <summary>
        /// Trim any leading zeroes to ensure <see cref="RSAParameters"/> are valid.
        /// https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-wcce/5cf2e6b9-3195-4f85-bc18-05b50e6d4e11?redirectedfrom=MSDN
        /// </summary>
        private static byte[] ValidateInt(byte[] integer, int keySize, int quotient)
        {
            if (integer.Length != Math.Ceiling(keySize / (double)quotient) &&
                    integer[0] == 0)
            {
                var bts = new byte[integer.Length - 1];
                Buffer.BlockCopy(integer, 1, bts, 0, bts.Length);
                return bts;
            }
            return integer;
        }
    }
}
