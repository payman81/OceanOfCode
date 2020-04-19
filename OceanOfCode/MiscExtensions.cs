using System;
using System.Collections.Generic;
using System.Linq;

namespace OceanOfCode
{
    public static class MiscExtensions
    {
        public static IEnumerable<T> TakeLast<T>(this IList<T> source, int n)
        {
            var count = Math.Max(0, source.Count - n);
            return source.Skip(count);
        }
    }
}