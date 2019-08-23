using System.Collections.Generic;
using System.Linq;

namespace Dinah.Core
{
    // https://enterprisecraftsmanship.com/2017/08/28/value-object-a-better-implementation/
    public abstract class ValueObject
    {
        protected abstract IEnumerable<object> GetEqualityComponents();

        public override bool Equals(object obj)
            => (obj == null || GetType() != obj.GetType())
            ? false
            : GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());

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

        public static bool operator ==(ValueObject a, ValueObject b)
            => (a is null && b is null) ? true
            : (a is null || b is null) ? false
            : a.Equals(b);

        public static bool operator !=(ValueObject a, ValueObject b) => !(a == b);
    }

}
