using System.Collections.Generic;
using System.Linq;
using Model.Extensions;

namespace Model.Libraries;

public static class Ranges
{
    private static double PyramidFn(int n, int i, int h) => i + (h / 2 + 0.5 * (h % 2)) - 0.5 * (n - 1);

    public static IEnumerable<(int i, int j, double x, double y)> Pyramid2(int n, double a = 1)
    {
        return (n, n).SelectRange((j, i) => (i, j))
            .Where(v => v.j < n - v.i)
            .Select(v => (v.i, v.j, PyramidFn(n, v.j, v.i), a * v.i));
    }

    public static IEnumerable<(int i, int j, int k, double x, double y, double z)> Pyramid3(int n)
    {
        return (n, n, n).SelectRange((k, i, j) => (i, j, k))
            .Where(v => v.j < n - v.k && v.i < n - v.k)
            .Select(v => (v.i, v.j, v.k, PyramidFn(n, v.j, v.k), PyramidFn(n, v.i, v.k), (double)v.k));
    }

    public static IEnumerable<(int i, int j)> Range(int m, int n) => Range(m).SelectMany(i => Range(n).Select(j => (i, j)));

    public static IEnumerable<int> Range(int n) => Enumerable.Range(0, n);

    public static IEnumerable<(int num, int i, int j, bool top)> Hedgehog(int m, int n, byte shift = 0)
    {
        return (m, n).SelectRange((i, j) => (i * n + j, i, j, (i + j) % 2 == shift));
    }
}
