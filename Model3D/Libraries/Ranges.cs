using System.Collections.Generic;
using System.Linq;

namespace Model.Libraries
{
    public static class Ranges
    {
        public static IEnumerable<(int, int)> Range(int m, int n) => Range(m).SelectMany(i => Range(n).Select(j => (i, j)));
        public static IEnumerable<int> Range(int n) => Enumerable.Range(0, n);
    }
}
