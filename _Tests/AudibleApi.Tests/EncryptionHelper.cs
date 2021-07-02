using AuthorizationShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudibleApi.Tests
{
    static class EncryptionHelper
    {
        public static string EncryptVoucher(string asin, string voucherJstring)
        {
            var identity =  Shared.GetIdentity(Shared.AccessTokenTemporality.Future);

            byte[] keyComponents = System.Text.Encoding.ASCII.GetBytes(
                identity.DeviceType +
                identity.DeviceSerialNumber +
                identity.AmazonAccountId +
                asin
                );

            byte[] key = new byte[16];
            byte[] iv = new byte[16];

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                sha256.ComputeHash(keyComponents);
                Array.Copy(sha256.Hash, 0, key, 0, 16);
                Array.Copy(sha256.Hash, 16, iv, 0, 16);
            }

            byte[] cipherText;

            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                int needeSize = voucherJstring.Length + (aes.BlockSize / 8 - voucherJstring.Length % (aes.BlockSize / 8));
                byte[] plainText = new byte[needeSize];
                Encoding.ASCII.GetBytes(voucherJstring, 0, voucherJstring.Length, plainText, 0);
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;

                using (var encryptor = aes.CreateEncryptor(key, iv))

                using (var msEncrypt = new System.IO.MemoryStream())
                {

                    using (var csDecrypt = new System.Security.Cryptography.CryptoStream(msEncrypt, encryptor, System.Security.Cryptography.CryptoStreamMode.Write))

                        //No padding used, so plaintext same size as ciphertext
                        csDecrypt.Write(plainText, 0, plainText.Length);

                    cipherText = msEncrypt.ToArray();
                }
            }

            return Convert.ToBase64String(cipherText);
        }
        public static string DecryptVoucher(string asin, string license_response)
        {
            var identity = Shared.GetIdentity(Shared.AccessTokenTemporality.Future);

            byte[] keyComponents = System.Text.Encoding.ASCII.GetBytes(
                identity.DeviceType +
                identity.DeviceSerialNumber +
                identity.AmazonAccountId +
                asin
                );

            byte[] key = new byte[16];
            byte[] iv = new byte[16];

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                sha256.ComputeHash(keyComponents);
                Array.Copy(sha256.Hash, 0, key, 0, 16);
                Array.Copy(sha256.Hash, 16, iv, 0, 16);
            }

            var cipherText = Convert.FromBase64String(license_response);

            string plainText;

            using (var aes = System.Security.Cryptography.Aes.Create())
            {
                aes.Mode = System.Security.Cryptography.CipherMode.CBC;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;

                using (var decryptor = aes.CreateDecryptor(key, iv))

                using (var msDecrypt = new System.IO.MemoryStream(cipherText))

                using (var csDecrypt = new System.Security.Cryptography.CryptoStream(msDecrypt, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                {
                    //No padding used, so plaintext same size as ciphertext
                    byte[] ptBuff = new byte[cipherText.Length];
                    csDecrypt.Read(ptBuff, 0, ptBuff.Length);
                    //No padding, so only use non-null values
                    plainText = System.Text.Encoding.ASCII.GetString(ptBuff.TakeWhile(b => b != 0).ToArray());
                }
            }
            return plainText;
        }
    }
}
