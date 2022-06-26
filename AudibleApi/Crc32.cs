namespace AudibleApi
{
    public static class Crc32
    {
        private const uint polynomial = 3988292384;
        private static readonly uint[] table = new uint[256];
        static Crc32()
        {
            uint value, temp;
            for (uint i = 0; i < table.Length; ++i)
            {
                value = 0;
                temp = i;
                for (byte j = 0; j < 8; ++j)
                {
                    if (((value ^ temp) & 0x1) != 0)
                    {
                        value = (value >> 1) ^ polynomial;
                    }
                    else
                    {
                        value >>= 1;
                    }
                    temp >>= 1;
                }
                table[i] = value;
            }
        }

        public static uint ComputeChecksum(byte[] bytes)
        {
            uint crc = 0;
            crc ^= uint.MaxValue;
            for (int i = 0; i < bytes.Length; ++i)
            {
                byte index = (byte)(crc ^ bytes[i]);
                crc = (crc >> 8) ^ table[index];
            }
            crc ^= uint.MaxValue;
            return crc;
        }
    }
}
