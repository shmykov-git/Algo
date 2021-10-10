using System.Collections.Generic;
using System.Linq;

namespace Model3D.Extensions
{
    public static class ConvexExtensions
    {
        public static int[][] ApplyBi(this IEnumerable<int[]> convexes, int[] bi)
        {
            return convexes.Select(c => c.Select(i => bi[i]).ToArray()).ToArray();
        }
    }
}
