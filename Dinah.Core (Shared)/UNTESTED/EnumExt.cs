using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Dinah.Core
{
    #region class method/s
    /// <summary>Enum Extension Methods</summary>
    /// <typeparam name="T">type of Enum</typeparam>
    public static class Enum<T> where T : Enum
    {
        public static T Parse(string value) => (T)Enum.Parse(typeof(T), value);

        public static IEnumerable<T> GetValues() => (T[])Enum.GetValues(typeof(T));

        public static int ToInt(T obj) => Convert.ToInt32(Enum.Parse(typeof(T), obj.ToString()) as Enum);

        /// <summary>get count of enum options except "None"</summary>
        public static int Count => GetValues().Count(obj => ToInt(obj) > 0);
    }
    #endregion

    #region object/instance methods. set of single values (non-flags)
    // from: C:\Dev\ihes\main\Framework\Itron.Framework\StringExtensions.cs
    public static class EnumExt
    {
        public static string GetDescription(this Enum en)
        {
            // helper to get enum [Description] attribute metadata.
            // Get the [Description] attribute string for an enum (if it has one) else default to the enum's name.
            if (en == null)
                return "[null]";

            string name = en.ToString();
            MemberInfo[] memInfo = en.GetType().GetMember(name); // could use GetField() for FieldInfo ?
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
                if (attrs != null && attrs.Length > 0)
                    return ((DescriptionAttribute)attrs[0]).Description;
            }
            return name;
        }
    }
    #endregion

    #region object/instance methods. flags
    // most from: http://hugoware.net/blog/enumeration-extensions-2-0
    public static class EnumerationFlagsExtensions
    {
        [Flags]
        enum ExampleOptions
        {
            None = 0,
            One = 1,
            Two = 2,
            Four = 4
        }
        static void EXAMPLES()
        {
            //create the typical object
            var options = ExampleOptions.None;

            //Assign a value
            options = options.Include(ExampleOptions.One);
            //options: One

            //Or assign multiple values
            options = options.Include(ExampleOptions.Two | ExampleOptions.Four);
            //options: One, Two, Four

            //Remove values from the list
            options = options.Remove(ExampleOptions.One);
            //options: Two, Four

            //Check if a value even exists
            bool multiline = options.HasFlag(ExampleOptions.Two); //true
            bool ignoreCase = options.MissingFlag(ExampleOptions.One); //true
        }

        public static IEnumerable<TEnum> GetFlags<TEnum>(this TEnum input, bool checkZero = false, bool checkFlags = true)
            where TEnum : Enum
        {
            Type enumType = typeof(TEnum);
            if (!enumType.IsEnum)
                yield break;

            ulong setBits = Convert.ToUInt64(input);
            // if no flags are set, return empty
            if (!checkZero && (0 == setBits))
                yield break;

            // if it's not a flag enum, return empty
            if (checkFlags && !input.GetType().IsDefined(typeof(FlagsAttribute), false))
                yield break;

            // check each enum value mask if it is in input bits
            foreach (TEnum value in Enum<TEnum>.GetValues())
            {
                ulong valMask = Convert.ToUInt64(value);

                if ((setBits & valMask) == valMask)
                    yield return value;
            }
        }

        public static T IncludeOrRemove<T>(this Enum value, T flag, bool addRemoveFlag)
        {
            if (addRemoveFlag)
                return value.Include(flag);
            else
                return value.Remove(flag);
        }

        /// <summary>Includes an enumerated type and returns the new value</summary>
        public static T Include<T>(this Enum value, T append)
        {
            Type type = value.GetType();

            //determine the values
            _Value parsed = new _Value(append, type);
            object result = value;
            if (parsed.Signed is long)
                result = Convert.ToInt64(value) | (long)parsed.Signed;
            else if (parsed.Unsigned is ulong)
                result = Convert.ToUInt64(value) | (ulong)parsed.Unsigned;

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>Removes an enumerated type and returns the new value</summary>
        public static T Remove<T>(this Enum value, T remove)
        {
            Type type = value.GetType();

            //determine the values
            _Value parsed = new _Value(remove, type);
            object result = value;
            if (parsed.Signed is long)
                result = Convert.ToInt64(value) & ~(long)parsed.Signed;
            else if (parsed.Unsigned is ulong)
                result = Convert.ToUInt64(value) & ~(ulong)parsed.Unsigned;

            //return the final value
            return (T)Enum.Parse(type, result.ToString());
        }

        /// <summary>Checks if an enumerated type is missing a value</summary>
        public static bool MissingFlag<T>(this T obj, Enum value) where T : struct, IConvertible
            => !(Enum.Parse(typeof(T), obj.ToString()) as Enum).HasFlag(value);

        //class to simplfy narrowing values between a ulong and long since either value should cover any lesser value
        private class _Value
        {
            //cached comparisons for tye to use
            private static Type _UInt64 = typeof(ulong);
            private static Type _UInt32 = typeof(long);

            public long? Signed;
            public ulong? Unsigned;

            public _Value(object value, Type type)
            {
                //make sure it is even an enum to work with
                if (!type.IsEnum)
                    throw new ArgumentException("Value provided is not an enumerated type!");

                //then check for the enumerated value
                Type compare = Enum.GetUnderlyingType(type);

                //if this is an unsigned long then the only value that can hold it would be a ulong
                if (compare.Equals(_UInt32) || compare.Equals(_UInt64))
                    Unsigned = Convert.ToUInt64(value);
                //otherwise, a long should cover anything else
                else
                    Signed = Convert.ToInt64(value);
            }
        }
    }
    #endregion
}
