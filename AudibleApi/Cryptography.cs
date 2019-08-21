using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using AudibleApi.Authorization;
using BaseLib;

namespace AudibleApi
{
    public static class Cryptography
    {
		public static string Javascript { get; } = File.ReadAllText("Cryptography.js");

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

            using (var sha256 = new SHA256Managed())
            using (var rsaCSP = CreateRsaProviderFromPrivateKey(private_key))
            {
                var digestion = sha256.ComputeHash(dataBytes);
                // as string: digestion.Select(x => $"{x:x2}").Aggregate("", (a, b) => a + b);
                var oid = CryptoConfig.MapNameToOID("SHA256");
                return rsaCSP.SignHash(digestion, oid);
            }
        }

        // https://stackoverflow.com/a/32150537
        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            privateKey = privateKey
                .Replace("-----BEGIN RSA PRIVATE KEY-----", "")
                .Replace("-----END RSA PRIVATE KEY-----", "")
                .Replace("\\n", "")
                .Trim();

            var privateKeyBits = Convert.FromBase64String(privateKey);

            var RSA = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();

            using (BinaryReader binr = new BinaryReader(new MemoryStream(privateKeyBits)))
            {
                var twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                var bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                RSAparams.Modulus = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Exponent = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.D = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.P = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.Q = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DP = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.DQ = binr.ReadBytes(GetIntegerSize(binr));
                RSAparams.InverseQ = binr.ReadBytes(GetIntegerSize(binr));
            }

            RSA.ImportParameters(RSAparams);
            return RSA;
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            var bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;

            bt = binr.ReadByte();

            int count;
            if (bt == 0x81)
                count = binr.ReadByte();
            else if (bt == 0x82)
            {
                var highbyte = binr.ReadByte();
                var lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
    }
}
