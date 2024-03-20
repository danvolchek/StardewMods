using System;
using System.Collections.Generic;

namespace ModUpdateMenu.Extensions
{
    /// <summary>Extensions for the List class.</summary>
    internal static class ListExtensions
    {
        /// <summary>Create lists of tuples more easily.</summary>
        public static void Add<T1, T2>(this IList<Tuple<T1, T2>> list, T1 item1, T2 item2)
        {
            list.Add(new Tuple<T1, T2>(item1, item2));
        }
    }
}
