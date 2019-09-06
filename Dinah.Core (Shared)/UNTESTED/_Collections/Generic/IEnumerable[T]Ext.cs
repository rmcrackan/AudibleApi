using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinah.Core.Collections.Generic
{
    public static class IEnumerable_T_Ext
    {
        public static bool In<T>(this T source, params T[] parameters) => _in(source, parameters);
        public static bool In<T>(this T source, IEnumerable<T> parameters) => _in(source, parameters);
        private static bool _in<T>(T source, IEnumerable<T> parameters) => parameters.Contains(source);

        /// <summary>Determines whether a string collection contains a specified string. Case-INsensative.</summary>
        public static bool ContainsInsensative(this IEnumerable<string> collection, string str) => collection.Any(item => item.EqualsInsensitive(str));

        // from MoreLinq. add ref to entire lib if i need more than 1 or 2
        // https://github.com/morelinq/MoreLINQ
        #region DistintBy()
        #region // examples
        // var query = people.DistinctBy(p => p.Id);
        // var query = people.DistinctBy(p => new { p.Id, p.Name });
        #endregion

        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the default equality comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            return source.DistinctBy(keySelector, null);
        }

        /// <summary>
        /// Returns all distinct elements of the given source, where "distinctness"
        /// is determined via a projection and the specified comparer for the projected type.
        /// </summary>
        /// <remarks>
        /// This operator uses deferred execution and streams the results, although
        /// a set of already-seen keys is retained. If a key is seen multiple times,
        /// only the first element with that key is returned.
        /// </remarks>
        /// <typeparam name="TSource">Type of the source sequence</typeparam>
        /// <typeparam name="TKey">Type of the projected element</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="keySelector">Projection for determining "distinctness"</param>
        /// <param name="comparer">The equality comparer to use to determine whether or not keys are equal.
        /// If null, the default equality comparer for <c>TSource</c> is used.</param>
        /// <returns>A sequence consisting of distinct elements from the source sequence,
        /// comparing them by the specified key projection.</returns>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (keySelector == null) throw new ArgumentNullException(nameof(keySelector));

            return _(); IEnumerable<TSource> _()
            {
                var knownKeys = new HashSet<TKey>(comparer);
                foreach (var element in source)
                {
                    if (knownKeys.Add(keySelector(element)))
                        yield return element;
                }
            }
        }
        #endregion

        /// <summary>Are there any common values between a and b?</summary>
        public static bool SharesAnyValueWith<T>(this IEnumerable<T> a, IEnumerable<T> b)
            => a == null || b == null
            ? false
            : a.Intersect(b).Any();
    }
}
