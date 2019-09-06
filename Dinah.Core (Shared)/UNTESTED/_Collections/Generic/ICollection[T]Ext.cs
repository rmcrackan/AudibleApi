using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinah.Core.Collections.Generic
{
    public static class ICollection_T_Ext
    {
        public static void AddOrRemove<T>(this ICollection<T> hashSet, T item, bool addRemoveFlag)
        {
            if (addRemoveFlag)
                hashSet.Add(item);
            else
                hashSet.Remove(item);
        }

        public static void AddIfNotContains<T>(this ICollection<T> list, T item)
        {
            if (!list.Contains(item))
                list.Add(item);
        }
    }
}
