using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinah.Core
{
    // adapted from http://enterprisecraftsmanship.com/2014/11/08/domain-object-base-class/
    public abstract class Entity<T> where T : class
    {
        public virtual T Id { get; protected set; }

        public override bool Equals(object obj) => eq(obj as Entity<T>);
        private bool eq(Entity<T> compareTo)
            => ReferenceEquals(compareTo, null) ? false
            : ReferenceEquals(this, compareTo) ? true
            : GetRealType() != compareTo.GetRealType() ? false
            : (!IsTransient() && !compareTo.IsTransient() && Id == compareTo.Id);

        public static bool operator ==(Entity<T> a, Entity<T> b)
            => ReferenceEquals(a, null) && ReferenceEquals(b, null) ? true
            : ReferenceEquals(a, null) || ReferenceEquals(b, null) ? false
            : a.Equals(b);

        public static bool operator !=(Entity<T> a, Entity<T> b) => !(a == b);

        public override int GetHashCode() => (GetRealType().ToString() + Id).GetHashCode();

        public virtual bool IsTransient() => Id == default(T);

        //original NHibernate way: return NHibernateUtil.GetClass(this);
        // EF way. has external dependencies. wouldn't want it in Core
        //// https://stackoverflow.com/a/16005340
        // return ObjectContext.GetObjectType(this.GetType());
        public abstract Type GetRealType();
    }
}
