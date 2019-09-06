using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dinah.Core.DataBinding
{
    public static class ListExt_SortableBindingList
    {
        public static SortableBindingList<T> ToSortableBindingList<T>(this IEnumerable<T> collection) => new SortableBindingList<T>(collection);
    }
}
