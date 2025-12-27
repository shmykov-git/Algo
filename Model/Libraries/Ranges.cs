using Model.Extensions;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// 0 <= i < n, 
    /// 0 <= j < n,
    /// circle values from i to j by straight direction
    /// </summary>
    public static IEnumerable<int> Circle(int n, int i, int j)
    {
        var ii = (i + n) % n;
        var jj = (j + n) % n;

        var k = ii;

        while (k != jj)
        {
            yield return k;

            k = (k + 1) % n;
        }

        yield return jj;
    }

    /// <summary>
    /// 0 <= i < n, 
    /// 0 <= j < n,
    /// circle values from i to j by back direction
    /// </summary>
    public static IEnumerable<int> CircleBack(int n, int i, int j)
    {
        var ii = (i + n) % n;
        var jj = (j + n) % n;

        var k = ii;

        while (k != jj)
        {
            yield return k;

            k = (k - 1 + n) % n;
        }

        yield return jj;
    }

    public static IEnumerable<(int i, int k)> CircleBoth(int n, int i, int count, int countFrom = 0)
    {
        var ii = (i + n) % n;
        var j = countFrom + 1;

        while (j <= count)
        {
            yield return ((ii - j + n) % n, (ii + j) % n);
            j++;
        }
    }
}
