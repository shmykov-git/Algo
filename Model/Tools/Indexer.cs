using System.Collections.Generic;

namespace Model.Tools
{
    public static class Indexer
    {
        public static (int[] indices, bool[] filter) DistinctIndices<TItem>(TItem[] items)
        {
            var iDist = 0;
            var indices = new Dictionary<TItem, int>(items.Length);
            var backIndices = new int[items.Length];
            var filter = new bool[items.Length];

            for (var i=0; i<items.Length; i++)
            {
                if (indices.TryGetValue(items[i], out int index))
                {
                    backIndices[i] = index;
                    filter[i] = false;
                }
                else
                {
                    indices.Add(items[i], iDist);
                    backIndices[i] = iDist;
                    filter[i] = true;
                    iDist++;
                }
            }

            return (backIndices, filter);
        }
    }
}
