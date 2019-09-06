using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Dinah.Core
{
    #region from namespace Itron.Framework
    /// <summary>
    /// Represents a utility class that can be used to validate method parameters and property values.
    /// </summary>
    public static class ArgumentValidator
    {
        /// <summary>
        /// Used to verify that the provided <i>argument</i> does not have a <see cref="DateTime.Kind"/> value
        /// of <see cref="DateTimeKind.Unspecified"/>.
        /// </summary>
        /// <param name="argument">The argument value that will be evaluated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentException">
        /// Thrown when <i>argument</i> has a <see cref="DateTime.Kind"/> value of <see cref="DateTimeKind.Unspecified"/>.
        /// </exception>
        public static void EnsureDateTimeKindIsSpecified(DateTime argument, string name)
        {
            if (argument.Kind == DateTimeKind.Unspecified)
            {
                throw new ArgumentException("The argument cannot have a Kind value of Unspecified.", name);
            }
        }

        /// <summary>
        /// Used to verify that a property array is not null or empty.
        /// </summary>
        /// <param name="value">The array that will be validated.</param>
        /// <param name="name">The name of the property that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <i>value</i> is null.
        /// - or -
        /// Thrown when <i>value</i> is an empty array.
        /// </exception>
        public static void EnsurePropertyArrayNotNullOrEmpty(Array value, string name)
        {
            EnsurePropertyNotNull(value, name);

            if (value.Length == 0)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The {0} property must contain one or more elements.", name));
            }
        }

        /// <summary>
        /// Used to verify that a property value is not null.
        /// </summary>
        /// <param name="value">The object that will be validated.</param>
        /// <param name="name">The name of the property that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="InvalidOperationException">Thrown when <i>value</i> is null.</exception>
        public static void EnsurePropertyNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "The {0} property cannot be null.", name));
            }
        }

        /// <summary>
        /// Used to verify that the provided argument is not null.
        /// </summary>
        /// <param name="argument">The object that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        public static void EnsureNotNull(object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument is not null.
        /// </summary>
        /// <param name="argument">The object that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        public static void EnsureNull(object argument, string name)
        {
            if (argument != null)
            {
                throw new ArgumentException(name);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument is not null.
        /// </summary>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <param name="arguments">The objects that will be validated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        public static void EnsureAtLeastOneNotNull(string name, params object[] arguments)
        {
            bool found = false;

            foreach (object obj in arguments)
            {
                if (obj != null)
                {
                    found = true;

                    break;
                }
            }

            if (!found)
            {
                throw new ArgumentNullException(name);
            }
        }

        /// <summary>
        /// Used to verify that the provided collection is not null and that all elements in the collection are not null.
        /// The collection may be empty.
        /// </summary>
        /// <param name="argument">The collection that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null or <i>argument</i> contains null elements.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty collection.</exception>
        public static void EnsureAllInCollectionNotNull(ICollection argument, string name)
        {
            EnsureNotNull(argument, name);

            foreach (object obj in argument)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException(name);
                }
            }
        }

        /// <summary>
        /// Used to verify that the provided collection is not null, empty, or contains any null elements.
        /// </summary>
        /// <param name="argument">The collection that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty collection.</exception>
        public static void EnsureCollectionNotNullOrEmpty(ICollection argument, string name)
        {
            EnsureNotNull(argument, name);

            if (argument.Count == 0)
            {
                throw new ArgumentException("The value must contain one or more items.", name);
            }

            EnsureAllInCollectionNotNull(argument, name);
        }

        /// <summary>
        /// Used to verify that the provided IEnumerable object returns an enumerator that contains values.
        /// </summary>
        /// <param name="argument">The IEnumerable to check</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is empty.</exception>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        public static void EnsureEnumerableNotNullOrEmpty<T>(IEnumerable<T> argument, string name)
        {
            EnsureNotNull(argument, name);
            if (!argument.Any())
            {
                throw new ArgumentException("The value must contain one or more items.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided collection is not null or empty.
        /// </summary>
        /// <param name="argument">The collection that will be validated.</param>
        /// <param name="minimum">The minimum inclusive number of items that the provided <i>argument</i> collection can contain.</param>
        /// <param name="maximum">The maximum inclusive number of items that the provided <i>argument</i> collection can contain.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <i>argument</i> contains fewer than the provided <i>minimum</i> or more than the provided <i>maximum</i> items.
        /// </exception>
        public static void EnsureCollectionInRange(ICollection argument, int minimum, int maximum, string name)
        {
            EnsureNotNull(argument, name);

            if ((argument.Count < minimum) || (argument.Count > maximum))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The value must contain between {0} and {1} items.", minimum, maximum), name);
            }
        }

        /// <summary>
        /// Used to verify that the provided collection is not null or empty.
        /// </summary>
        /// <param name="argument">The dictionary that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty collection.</exception>
        public static void EnsureDictionaryNotNullOrEmpty(IDictionary argument, string name)
        {
            EnsureNotNull(argument, name);

            if (argument.Count == 0)
            {
                throw new ArgumentException("The value must contain one or more items.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided collection contains no duplicate items.
        /// </summary>
        /// <param name="argument">The collection that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty collection.</exception>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        public static void EnsureCollectionContainsNoDuplicates<T>(IEnumerable<T> argument, string name)
        {
            if (argument.GroupBy(item => item).Where(grouping => grouping.Count() > 1).Count() > 0)
            {
                throw new ArgumentException("The collection contains duplicate items.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided generic list is not null or empty.
        /// </summary>
        /// <param name="argument">The list that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty collection.</exception>
        /// <typeparam name="T">The type of elements in the collection</typeparam>
        public static void EnsureListNotNullOrEmpty<T>(ICollection<T> argument, string name)
        {
            EnsureNotNull(argument, name);

            if (argument.Count == 0)
            {
                throw new ArgumentException("The value must contain one or more items.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided array is not null or empty.
        /// </summary>
        /// <param name="argument">The array that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty array.</exception>
        public static void EnsureArrayNotNullOrEmpty(Array argument, string name)
        {
            EnsureNotNull(argument, name);

            if (argument.Length == 0)
            {
                throw new ArgumentException("The value must contain one or more elements.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided array is not null and that it contains a fixed number of elements.
        /// </summary>
        /// <param name="argument">The array that will be validated.</param>
        /// <param name="size">The number of elements that the array must contain.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is does not contain <i>size</i> elements.</exception>
        public static void EnsureArrayIsFixedSize(Array argument, int size, string name)
        {
            EnsureNotNull(argument, name);

            if (argument.Length != size)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The value does not contain {0} elements.", size), name);
            }
        }

        /// <summary>
        /// Used to verify that the provided string is not null or zero length.
        /// </summary>
        /// <param name="argument">The string that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty string.</exception>
        public static void EnsureNotNullOrEmpty(string argument, string name)
        {
            EnsureNotNull(argument, name);
            if (string.IsNullOrEmpty(argument))
            {
                throw new ArgumentException("The value cannot be an empty string.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided string is not null and that it doesn't contain only white space.
        /// </summary>
        /// <param name="argument">The string that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is either an empty string or contains only white space.</exception>
        public static void EnsureNotNullOrWhiteSpace(string argument, string name)
        {
            EnsureNotNull(argument, name);
            if (string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentException("The value cannot be an empty string or contain only whitespace.", name);
            }
        }

        /// <summary>
        /// Deprecated name for above EnsureNotNullOrWhiteSpace.
        /// </summary>
        /// <param name="argument">The string that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is either an empty string or contains only white space.</exception>
        public static void EnsureNotNullOrContainsOnlyWhiteSpace(string argument, string name)
        {
            EnsureNotNullOrWhiteSpace(argument, name);
        }

        /// <summary>
        /// Make sure the given argument is at least 1.  If it isn't, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="argument">The argument that you want validated</param>
        /// <param name="name">The name of the argument.  It will be used in the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> value is not greater than or equal to 1.</exception>
        public static void EnsureOneOrGreater(int argument, string name)
        {
            if (argument < 1)
            {
                throw new ArgumentException("The value must be greater than or equal to one.", name);
            }
        }

        /// <summary>
        /// Make sure the given argument is at least 0.  If it isn't, an ArgumentException will be thrown.
        /// </summary>
        /// <param name="argument">The argument that you want validated</param>
        /// <param name="name">The name of the argument.  It will be used in the exception.</param>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> value is less than 0.</exception>
        public static void EnsureZeroOrGreater(int argument, string name)
        {
            if (argument < 0)
            {
                throw new ArgumentException("The value must be greater than or equal to zero.", name);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument has a length greater than or equal to the minimum and less than or equal to the maximum values.
        /// </summary>
        /// <param name="argument">The value that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <param name="minimum">The inclusive minimum valid length for the provided <i>argument</i> string.</param>
        /// <param name="maximum">The inclusive maximum valid length for the provided <i>argument</i> string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is not within the provided length range.</exception>
        public static void EnsureNotNullAndValidStringLength(string argument, string name, int minimum, int maximum)
        {
            EnsureNotNull(argument, name);

            if ((argument.Length < minimum) || (argument.Length > maximum))
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The provided value must have a length greater than or equal to {0} and less than or equal to {1}.", minimum, maximum), name);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument has a UTF8 encoded length greater than or equal to the minimum and less than or equal to the maximum values.
        /// </summary>
        /// <param name="argument">The value that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <param name="minimum">The inclusive minimum valid length for the provided <i>argument</i> string.</param>
        /// <param name="maximum">The inclusive maximum valid length for the provided <i>argument</i> string.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is not within the provided length range.</exception>
        public static void EnsureNotNullAndValidUTF8EncodedStringLength(string argument, string name, int minimum, int maximum)
        {
            EnsureNotNull(argument, name);

            int encodedArgumentLength = System.Text.Encoding.UTF8.GetByteCount(argument);
            if (encodedArgumentLength < minimum || encodedArgumentLength > maximum)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The provided value must have a UTF8 encoded length greater than or equal to {0} and less than or equal to {1}.", minimum, maximum), name);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument is greater than or equal to the minimum and less than or equal to the maximum values
        /// </summary>
        /// <typeparam name="T">Type of the argument</typeparam>
        /// <param name="argument">The value that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <param name="minimum">The inclusive minimum valid value for the provided <i>argument</i>.</param>
        /// <param name="maximum">The inclusive maximum valid value for the provided <i>argument</i>.</param>
        public static void EnsureValidRange<T>(T argument, string name, T minimum, T maximum) where T : IComparable<T>
        {
            if ((argument.CompareTo(minimum) < 0) || (argument.CompareTo(maximum) > 0))
            {
                // Maybe this should be ArgumentOutOfRangeException ?
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The provided value must be greater than or equal to {0} and less than or equal to {1}.", minimum, maximum), name);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument is greater than the minimum value
        /// </summary>
        /// <param name="argument">argument to check</param>
        /// <param name="name">Name of the argument</param>
        /// <param name="minimum">Value argument must be greater than</param>
        /// <typeparam name="T">Type of the argument. Must be IComparable</typeparam>
        public static void EnsureGreaterThan<T>(T argument, string name, T minimum) where T : IComparable<T>
        {
            if (argument.CompareTo(minimum) <= 0)
            {
                // ArgumentOutOfRangeException
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The provided value must be greater than {0}.", minimum), name);
            }
        }

        /// <summary>
        /// Verifies that a file with the specified path exists.
        /// </summary>
        /// <param name="filePath">Relative or absolute path to test.</param>
        /// <exception cref="FileNotFoundException">Thrown when a file does not exist at the specified location.</exception>
        public static void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The specified file could not be found.", filePath);
            }
        }

        /// <summary>
        /// Used to verify that the provided argument is less than the maximum value.
        /// </summary>
        /// <param name="argument">argument to check</param>
        /// <param name="name">Name of the argument</param>
        /// <param name="maximum">Value argument must be less than</param>
        /// <typeparam name="T">Type of the argument. Must be IComparable</typeparam>
        public static void EnsureLessThan<T>(T argument, string name, T maximum) where T : IComparable<T>
        {
            if (argument.CompareTo(maximum) >= 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The provided value must be less than {0}.", maximum), name);
            }
        }

        /// <summary>
        /// Used to verify that the provided stream is not null or empty.
        /// </summary>
        /// <param name="argument">The Stream that will be validated.</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentNullException">Thrown when <i>argument</i> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <i>argument</i> is an empty stream.</exception>
        public static void EnsureStreamNotNullOrEmpty(Stream argument, string name)
        {
            EnsureNotNull(argument, name);

            if (argument.Length < 1)
            {
                throw new ArgumentException("The value must not be empty.", name);
            }
        }

        /// <summary>
        /// Used to verify that a boolean constraint is true.
        /// </summary>
        /// <param name="constraintResult">Result of applying the constraint</param>
        /// <param name="message">Error message to display if the constraint is not met</param>
        /// <param name="name">The name of the <i>argument</i> that will be used to identify it should an exception be thrown.</param>
        /// <exception cref="ArgumentException">Thrown when the constraint is not met.</exception>
        public static void EnsureIsTrue(bool constraintResult, string message, string name)
        {
            if (!constraintResult)
            {
                throw new ArgumentException(message, name);
            }
        }
    }
    #endregion
}
