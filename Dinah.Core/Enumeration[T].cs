using System;
using System.Collections.Generic;

namespace Dinah.Core
{
    public abstract class Enumeration<TEnumerationType> : Enumeration where TEnumerationType : Enumeration
    {
        public int Id => Value;

        protected Enumeration(int value, string displayName) : base(value, displayName) { }

        public static IEnumerable<TEnumerationType> GetAll() => GetAll<TEnumerationType>();
        public static TEnumerationType FromValue(int value) => FromValue<TEnumerationType>(value);
        public static TEnumerationType FromDisplayName(string displayName) => FromDisplayName<TEnumerationType>(displayName);
    }
}
