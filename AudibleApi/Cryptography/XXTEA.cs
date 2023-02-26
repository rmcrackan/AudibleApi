using System;

namespace AudibleApi.Cryptography
{
    internal static class XXTEA
    {
        //Corrected Block TEA
        //https://en.wikipedia.org/wiki/XXTEA

        private const uint DELTA = 0x9e3779b9;

        public static byte[] Encrypt(byte[] clearBytes, uint[] key)
        {
            int n = (int)Math.Ceiling(clearBytes.Length / 4d);

            uint[] clearText = new uint[n];
            Buffer.BlockCopy(clearBytes, 0, clearText, 0, clearBytes.Length);

            uint z = clearText[^1], y, sum = 0, e;
            int p;

            uint MX() => (z >> 5 ^ y << 2) + (y >> 3 ^ z << 4) ^ (sum ^ y) + (key[p & 3 ^ e] ^ z);

            for (int rounds = 6 + 52 / n; rounds > 0; rounds--)
            {
                sum += DELTA;
                e = sum >> 2 & 3;

                for (p = 0; p < n - 1; p++)
                {
                    y = clearText[p + 1];
                    z = clearText[p] += MX();
                }

                y = clearText[0];
                z = clearText[^1] += MX();
            }

            byte[] cipherBytes = new byte[n * sizeof(uint)];
            Buffer.BlockCopy(clearText, 0, cipherBytes, 0, cipherBytes.Length);
            return cipherBytes;
        }
    }
}
