using System.Collections.Generic;
using System.Linq;
using Model.Extensions;

namespace Model.Libraries;

public static class Ranges
{
    public static IEnumerable<(int i, int j, double x, double y)> Pyramid2(int n)
    {
        return (n, n).SelectRange((j, i) => (i, j))
            .Where(v => v.j < n - v.i)
            .Select(v => (v.i, v.j, v.j + v.i / 2 - 0.5 * (n - 1) + 0.5 * (v.i % 2), (double)v.i));
    }

    public static IEnumerable<(int i, int j)> Range(int m, int n) => Range(m).SelectMany(i => Range(n).Select(j => (i, j)));

    public static IEnumerable<int> Range(int n) => Enumerable.Range(0, n);
}
