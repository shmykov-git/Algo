using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Tools
{
    public static class Indexer
    {
        public static (TItem[] items, int[] bi, bool[] filter) DistinctBi<TItem>(TItem[] items)
        {
            var iDist = 0;
            var indices = new Dictionary<TItem, int>(items.Length);
            var res = new List<TItem>();
            var backIndices = new int[items.Length];
            var filter = new bool[items.Length];

            for (var i = 0; i < items.Length; i++)
            {
                if (indices.TryGetValue(items[i], out int index))
                {
                    backIndices[i] = index;
                    filter[i] = false;
                }
                else
                {
                    indices.Add(items[i], iDist);
                    res.Add(items[i]);
                    backIndices[i] = iDist;
                    filter[i] = true;
                    iDist++;
                }
            }

            return (res.ToArray(), backIndices, filter);
        }

        public static (int[] bi, List<TItem> items) RemoveBi<TItem>(IEnumerable<TItem> items, IEnumerable<TItem> removeItems)
        {
            var result = items.Except(removeItems).ToList();
            var bi = items.Select(v => result.IndexOf(v)).ToArray();

            return (bi, result);
        }

        public static (int[] bi, List<TItem> items) WhereBi<TItem>(this IEnumerable<TItem> items, Func<TItem, bool> whereFunc)
        {
            var result = items.Where(whereFunc).ToList();
            var bi = items.Select(v => result.IndexOf(v)).ToArray();

            return (bi, result);
        }

        public static (int[] bi, List<TItem> items) WhereBi<TItem>(this IEnumerable<TItem> items, Func<TItem, int, bool> whereFunc)
        {
            var result = items.Where(whereFunc).ToList();
            var bi = items.Select(v => result.IndexOf(v)).ToArray();

            return (bi, result);
        }
    }
}
