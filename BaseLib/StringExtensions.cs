using System;

namespace BaseLib
{
    public static class StringExtensions
    {
        public static bool EqualsInsensitive(this string str1, string str2) => string.Equals(str1, str2, StringComparison.OrdinalIgnoreCase);

        public static bool StartsWithInsensitive(this string str1, string str2) => str1.StartsWith(str2, StringComparison.OrdinalIgnoreCase);

        public static bool EndsWithInsensitive(this string str1, string str2) => str1.EndsWith(str2, StringComparison.OrdinalIgnoreCase);

        public static bool ContainsInsensitive(this string str1, string str2) => str1?.IndexOf(str2, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
