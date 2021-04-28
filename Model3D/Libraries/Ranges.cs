using System.Collections.Generic;
using System.Linq;

namespace Model.Libraries
{
    public static class Ranges
    {
        public static IEnumerable<(int, int)> Range(int m, int n) => Enumerable.Range(0, m).SelectMany(i => Enumerable.Range(0, n).Select(j => (i, j)));
    }
}
