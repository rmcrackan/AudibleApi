using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;

namespace Dinah.Core
{
	//
	// for my string extensions and itron string extensions:
	// test and move to StringExtensions.cs
	//
    public static class StringExt
    {
        public static string Pluralize(this string str, int qty) => new Pluralize.NET.Pluralizer().Format(str, qty);

		/// <summary>return qty and noun</summary>
		public static string PluralizeWithCount(this string str, int qty) => new Pluralize.NET.Pluralizer().Format(str, qty, true);

		public static string FirstCharToUpper(this string str)
        {
            if (str == null)
                return null;
            if (string.IsNullOrWhiteSpace(str))
                return str;

            return char.ToUpper(str[0]) + str.Substring(1);
        }

        public static string Truncate(this string str, int limit) => str.Length >= limit ? str.Substring(0, limit) : str;

        public static string SurroundWithQuotes(this string str) => "\"" + str + "\"";

        public static string ExtractString(this string haystack, string before, int needleLength)
        {
            var index = haystack.IndexOf(before);
            var needle = haystack.Substring(index + before.Length, needleLength);

            return needle;
        }
    }

    #region from Itron.Framework
    /// <summary>
    /// Holds extension methods for strings
    /// </summary>
    public static class ItronStringExtensions
    {
        private const string _validHexChars = "0123456789ABCDEFabcdef";

        /// <summary>
        /// A very forgiving interpretation of a string to a boolean.
        /// Should work for JSON (lower case) as well as C# (capitalized)
        /// replaces bool.Parse(string) and its use of System.Boolean.TrueString or System.Boolean.FalseString
        /// </summary>
        /// <param name="str">The string to convert</param>
        /// <returns>A boolean value for the string</returns>
        public static bool ToBoolean(this string str)
        {
            str = str.ToLower(CultureInfo.InvariantCulture);

            if (str == "n" || str == "0" || str == "f" || str == "false")
            {
                return false;
            }

            if (str == "y" || str == "1" || str == "t" || str == "true")
            {
                return true;
            }

            // Should I try to interpret numbers ?
            return false;   // Be forgiving ? Make anything else a false?
        }

        /// <summary>
        /// Concatenates a collection of values into a string delimited by the supplied separator. 
        /// The specified format string is applied to each member in the collection.
        /// </summary>
        /// <param name="format">The format string used when creating the final string</param>
        /// <param name="separator">The separator to use when concatenating the values</param>
        /// <param name="values">The values to be joined into the final string</param>
        /// <returns>A string that is a list of the values separated by the separator string</returns>
        public static string JoinFormat(this string format, string separator, System.Collections.IEnumerable values)
        {
            ArgumentValidator.EnsureNotNull(format, nameof(format));

            if (values == null)
            {
                return string.Empty;
            }

            System.Collections.IEnumerator enumerator = values.GetEnumerator();

            if (!enumerator.MoveNext())
            {
                return string.Empty;
            }

            if (separator == null)
            {
                separator = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(format))
            {
                format = "{0}";
            }

            StringBuilder builder = new StringBuilder();

            foreach (object value in values)
            {
                if (value != null)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append(separator);
                    }

                    builder.AppendFormat(CultureInfo.InvariantCulture, format, value);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Formats a string using the current culture
        /// The original format string is NOT modified so the result must be assigned to a variable or passed as a method argument.
        /// </summary>
        /// <param name="format">The string to format</param>
        /// <param name="parameters">The substitution parameters to use during format</param>
        /// <returns>A formatted string</returns>
        public static string FormatCurrent(this string format, params object[] parameters)
        {
            ArgumentValidator.EnsureNotNull(format, nameof(format));

            return string.Format(CultureInfo.CurrentCulture, format, parameters);
        }

        /// <summary>
        /// Formats a string using the invariant culture
        /// The original format string is NOT modified so the result must be assigned to a variable or passed as a method argument.
        /// </summary>
        /// <param name="format">The string to format</param>
        /// <param name="parameters">The substitution parameters to use during format</param>
        /// <returns>A formatted string</returns>
        public static string FormatInvariant(this string format, params object[] parameters)
        {
            ArgumentValidator.EnsureNotNull(format, nameof(format));

            return string.Format(CultureInfo.InvariantCulture, format, parameters);
        }

        /// <summary>
        /// Extension for string to interpret as hexadecimal string and convert to a byte array
        /// </summary>
        /// <param name="hexValue">The hexadecimal string (such as "0001AF3cab") to convert into a byte array.</param>
        /// <returns>A byte array containing the bytes represented by the supplied string.</returns>
        /// <exception cref="ArgumentException">
        ///   If the string contains non-hexadecimal characters (except for leading or trailing white space, which is ignored), or if it 
        ///   does not contain an even number of characters (since 2 characters are needed to represent each byte).
        /// </exception>
        public static byte[] HexStringToByteArray(this string hexValue)
        {
            ArgumentValidator.EnsureNotNull(hexValue, nameof(hexValue));

            if (string.IsNullOrEmpty(hexValue))
            {
                throw new ArgumentException("hexValue cannot be an empty string", "hexValue");
            }

            string trimmed = hexValue.Trim();

            bool hasInvalidChars = trimmed.Except(_validHexChars).Any();

            if (hasInvalidChars)
            {
                throw new ArgumentException("string contains invalid hexadecimal characters.", "hexValue");
            }

            if ((trimmed.Length % 2) != 0)
            {
                throw new ArgumentException("string does not contain an even number of hexadecimal characters.", "hexValue");
            }

            byte[] result = new byte[trimmed.Length / 2];

            for (int index = 0; index < trimmed.Length; index += 2)
            {
                result[index / 2] = byte.Parse(trimmed.Substring(index, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return result;
        }

        /// <summary>
        /// Uppercases the first letter of a string
        /// </summary>
        /// <param name="value">The string to format</param>
        /// <returns>The new string with an uppercase first letter</returns>
        public static string UppercaseFirstLetter(this string value)
        {
            string result;

            if (!string.IsNullOrEmpty(value))
            {
                char[] characters = value.ToCharArray();

                characters[0] = char.ToUpper(characters[0], CultureInfo.InvariantCulture);

                result = new string(characters);
            }
            else
            {
                result = string.Empty;
            }

            return result;
        }

        /// <summary>
        /// Gets the description associated with an enum value
        /// </summary>
        /// <param name="en">The enum value to get a description for</param>
        /// <returns>The description associated with an enum value</returns>
        public static string GetDescription(Enum en)
        {
            // helper to get enum [Description] attribute metadata.
            // Get the [Description] attribute string for an enum (if it has one) else default to the enums name.
            // Should we use LocalizedDescriptionAttribute ? [LocalizedDescription] ?
            if (en == null)
            {
                return "null";
            }
            Type type = en.GetType();
            string name = en.ToString();
            MemberInfo[] memInfo = type.GetMember(name);   // could use GetField() for FieldInfo ?
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    return ((DescriptionAttribute)attrs[0]).Description;
                }
            }
            return name;   // default to using the enums name. like: Enum.GetName(type, en);
        }

        /// <summary>
        /// Gets the description associated with an enum value
        /// </summary>
        /// <param name="en">The enum value to get a description for</param>
        /// <returns>The description associated with an enum value</returns>
        public static string ToDescription(this Enum en)
        {
            // If this enum has a meta data description then return it else just return the string version of the enum
            return GetDescription(en);
        }

        /// <summary>
        /// Creates an MD5 fingerprint of the string.
        /// </summary>
        /// <param name="s">This string</param>
        /// <returns>MD5 encrypted string</returns>
        public static string ToMd5Fingerprint(this string s)
        {
            var bytes = Encoding.Unicode.GetBytes(s.ToCharArray());
            var hash = new MD5CryptoServiceProvider().ComputeHash(bytes);

            // concat the hash bytes into one long string
            return hash.Aggregate(new StringBuilder(32),
                (sb, b) => sb.Append(b.ToString("X2", CultureInfo.InvariantCulture)))
                .ToString();
        }

        /// <summary>
        /// Formats a string with a limited number of items from a list and a count of any remaining items.
        /// Example:
        ///     Input:
        ///         message: "NodeKeys: {0}"
        ///         list: new List() { 1, 3, 5, 7, 9, 11 }
        ///         limit: 3
        ///     Output:
        ///         "NodeKeys: 1, 3, 5 (+ 3 more)"
        /// </summary>
        /// <typeparam name="T">List can be of any type of items</typeparam>
        /// <param name="format">The format string.  This string is expected to have a single substitution parameter.</param>
        /// <param name="list">The list of items to format into the string</param>
        /// <param name="limit">The maximum number of items to explicitly include in the formatting</param>
        /// <returns>The formatted string</returns>
        public static string FormatWithLimitedList<T>(this string format, List<T> list, int limit)
        {
            string listString = string.Join(",", list.Take(limit));
            if (list.Count > limit)
            {
                listString += " (+ " + (list.Count - limit) + " more)";
            }

            return format.FormatInvariant(listString);
        }

        /// <summary>
        /// Counts the number of occurrences of a character in a string
        /// </summary>
        /// <param name="value">The string to evaluate</param>
        /// <param name="c">The character to count</param>
        /// <returns>The number of occurrences of a character in a string</returns>
        public static int CountOf(this string value, char c)
        {
            int count = 0;

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == c)
                {
                    count++;
                }
            }

            return count;
        }

        public static Stream ToMemoryStream(this string text)
        {
            return new MemoryStream(Encoding.Default.GetBytes(text));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static SecureString ToSecureString(this string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var secureText = new SecureString();
            foreach (var c in text.ToCharArray())
            {
                secureText.AppendChar(c);
            }
            secureText.MakeReadOnly();
            return secureText;
        }

        public static string ToUnsecureString(this SecureString secureText)
        {
            if (secureText == null)
            {
                throw new ArgumentNullException(nameof(secureText));
            }

            IntPtr ptrToPlainText = IntPtr.Zero;
            try
            {
                ptrToPlainText = Marshal.SecureStringToGlobalAllocUnicode(secureText);

                // Make sure we don't truncate any trailing '\0' characters (needed for some optical password handling)
                return Marshal.PtrToStringUni(ptrToPlainText).PadRight(secureText.Length, '\0');
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ptrToPlainText);
            }
        }

        public static byte[] ToSecureStringBytes(this SecureString secureText)
        {
            if (secureText == null)
            {
                throw new ArgumentNullException(nameof(secureText));
            }

            //get bytes from SecureString
            byte[] bytesPlain = new byte[secureText.Length];

            IntPtr ssAsIntPtr = Marshal.SecureStringToGlobalAllocUnicode(secureText);
            try
            {
                for (int i = 0; i < secureText.Length; i++)
                {
                    // multiply 2 because Unicode chars are 2 bytes wide
                    bytesPlain[i] = Marshal.ReadByte(ssAsIntPtr, i * 2);
                }
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(ssAsIntPtr);
            }
            return bytesPlain;
        }

    }
    #endregion
}

