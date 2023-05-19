using System;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;
using Dinah.Core;

namespace AudibleApi.Cryptography
{
    public class PrivateKey : StrongType<string>
    {
        public const string REQUIRED_BEGINNING = "-----BEGIN RSA PRIVATE KEY-----";
        public const string REQUIRED_ENDING = "-----END RSA PRIVATE KEY-----";
        private readonly RSACryptoServiceProvider RSACryptoService;

        public PrivateKey(string value) : base(value)
        {
            RSACryptoService = CreateRsaProviderFromPrivateKey(value);
        }

        protected override void ValidateInput(string value)
        {
            ArgumentValidator.EnsureNotNull(value, nameof(value));

            if (Convert.TryFromBase64String(value.Trim(), new byte[value.Length], out _))
                return;

            if (!value.Trim().StartsWith(REQUIRED_BEGINNING))
                throw new ArgumentException("Improperly formatted RSA private key", nameof(value));

            if (!value.Trim().EndsWith(REQUIRED_ENDING))
                throw new ArgumentException("Improperly formatted RSA private key", nameof(value));
        }

        public string SignMessage(string message)
        {
			var dataBytes = Encoding.UTF8.GetBytes(message);

			using var sha256 = SHA256.Create();
			var digestion = sha256.ComputeHash(dataBytes);
			var signedBytes = RSACryptoService.SignHash(digestion, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            return Convert.ToBase64String(signedBytes);
		}


        // https://stackoverflow.com/a/32150537
        private static RSACryptoServiceProvider CreateRsaProviderFromPrivateKey(string privateKey)
        {
            privateKey = privateKey
                .Replace(REQUIRED_BEGINNING, "")
                .Replace(REQUIRED_ENDING, "")
                .Replace("\\n", "")
                .Trim();

            var asn1EncodedPrivateKey = Convert.FromBase64String(privateKey);
            var keySequence = Asn1Value.Parse(asn1EncodedPrivateKey);

            if (keySequence.Children.Count >= 3 && keySequence.Children[2].Type == Asn1Tag.PrimitiveOctetString)
                keySequence = Asn1Value.Parse(keySequence.Children[2].Value);

            var RSA = new RSACryptoServiceProvider();
            var RSAparams = new RSAParameters();

            //Guess the key size based on modulus size. Assume key size is multiple of 32 bits.
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
        /// Trim or add leading zeroes to ensure <see cref="RSAParameters"/> are valid.
        /// https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-wcce/5cf2e6b9-3195-4f85-bc18-05b50e6d4e11?redirectedfrom=MSDN
        /// </summary>
        private static byte[] ValidateInt(byte[] integer, int keySize, int quotient)
        {
            var expectedSize = keySize / quotient;

            if (integer.Length == expectedSize)
                return integer;
            else if (integer.Length == expectedSize + 1 && integer[0] == 0)
                return integer[1..];
            else if (integer.Length == expectedSize - 1)
            {
                var bts = new byte[expectedSize];
                Buffer.BlockCopy(integer, 0, bts, 1, integer.Length);
                return bts;
            }
            else
                //If this happens, it won't happen for all key parameters, so logging the real value will not reveal the complete key.
                throw new CryptographicException($"Unable to parse rsa parameter integer: {BitConverter.ToString(integer)}");
        }
    }
}
