using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// FROM: http://enterprisecraftsmanship.com/2017/08/28/value-object-a-better-implementation/
namespace Dinah.Core
{
    // NO REFLECTION. see below (ValueObject_Reflection) for a reflection solution
    public abstract class ValueObject_Static<T>
        where T : ValueObject_Static<T>
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
        {
			if (!(obj is ValueObject_Static<T> valueObject))
				return false;

			if (GetType() != obj.GetType())
                return false;

            return GetEqualityComponents().SequenceEqual(valueObject.GetEqualityComponents());
        }

        public override int GetHashCode()
        {
            return GetEqualityComponents()
                .Aggregate(1, (current, obj) =>
                {
                    unchecked
                    {
                        return current * 23 + (obj?.GetHashCode() ?? 0);
                    }
                });
        }

        public static bool operator ==(ValueObject_Static<T> a, ValueObject_Static<T> b)
        {
            if (a is null && b is null)
                return true;

            if (a is null || b is null)
                return false;

            return a.Equals(b);
        }

        public static bool operator !=(ValueObject_Static<T> a, ValueObject_Static<T> b) => !(a == b);
    }

    // USES REFLECTION
    // https://lostechies.com/jimmybogard/2007/06/25/generic-value-object-equality/
    public abstract class ValueObject_Reflection<T> : IEquatable<T>
        where T : ValueObject_Reflection<T>
    {
        public override bool Equals(object obj) => obj == null ? false : Equals(obj as T);
        public override int GetHashCode()
        {
            var fields = GetFields();
            int startValue = 17;
            int multiplier = 59;
            int hashCode = startValue;
            foreach (var field in fields)
            {
                object value = field.GetValue(this);
                if (value != null)
                    hashCode = hashCode * multiplier + value.GetHashCode();
            }
            return hashCode;
        }
        public virtual bool Equals(T other)
        {
            if (other == null)
                return false;
            Type t = GetType();
            Type otherType = other.GetType();
            if (t != otherType)
                return false;
            var fields = t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var field in fields)
            {
                object value1 = field.GetValue(other);
                object value2 = field.GetValue(this);
                if (value1 == null)
                {
                    if (value2 != null)
                        return false;
                }
                else if (!value1.Equals(value2))
                    return false;
            }
            return true;
        }
        private IEnumerable<FieldInfo> GetFields()
        {
            var t = GetType();
            var fields = new List<FieldInfo>();
            while (t != typeof(object))
            {
                fields.AddRange(t.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));
                t = t.BaseType;
            }
            return fields;
        }
        public static bool operator ==(ValueObject_Reflection<T> x, ValueObject_Reflection<T> y) => x.Equals(y);
        public static bool operator !=(ValueObject_Reflection<T> x, ValueObject_Reflection<T> y) => !(x == y);
    }
}
