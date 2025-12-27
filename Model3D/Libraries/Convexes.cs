using Model.Extensions;
using System;
using System.Linq;

namespace Model3D.Libraries;

// Func<int, int, bool, bool, int[][]>
public delegate int[][] ConvexFunc(int m, int n, bool mClosed, bool nClosed);

public static class Convexes
{
    /// <summary>
    /// m, n - number of points by an axle
    /// shift = 0 or 1
    /// </summary>
    public static int[][] Hedgehog(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (i, j) => (i + j) % 2);
    public static int[][] ChessHedgehog(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, hedgehogTriangleMaps, (i, j) => 2 * (i % 2) + j % 2);

    public static int[][] ChessHedgehogBoth(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, hedgehogTriangleMaps, (i, j) => 2 * (i % 2) + j % 2, true);

    public static int[][] Hedgehog1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (i, j) => (i + j + 1) % 2);

    public static int[][] DiagonalSquares2D(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareDiagonalD2Maps);

    public static int[][] LineSquaresY(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, _) => i % 2 - 1);
    public static int[][] LineSquaresY1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, _) => (i + 1) % 2 - 1);

    public static int[][] LineSquaresX(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (_, j) => j % 2 - 1);
    public static int[][] LineSquaresX1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (_, j) => (j + 1) % 2 - 1);

    public static int[][] Squares(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps);
    public static int[][] ShiftedSquares(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => j < n - 2 ? 1 : -1);
    public static int[][] SquaresReverse(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, null, false, true);
    public static int[][] SquaresBoth(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, null, true);

    public static int[][] Triangles(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (_, _) => 0);
    public static int[][] CenteredTriangles(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (i, j) => ((i >= m / 2).Bit() + (j >= n / 2).Bit()) % 2);
    public static int[][] Triangles1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (_, _) => 1);

    public static int[][] ChessSquares(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => (i + j) % 2 - 1);
    public static int[][] ChessSquaresBoth(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => (i + j) % 2 - 1, true);
    public static int[][] ChessSquares1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => (i + j + 1) % 2 - 1);

    public static int[][] SpotSquares(int m, int n, bool mClosed = false, bool nClosed = false) => SpotSquaresInternal(m, n, mClosed, nClosed, 0);
    public static int[][] SpotSquares1(int m, int n, bool mClosed = false, bool nClosed = false) => SpotSquaresInternal(m, n, mClosed, nClosed, 1);
    public static int[][] SpotSquares2(int m, int n, bool mClosed = false, bool nClosed = false) => SpotSquaresInternal(m, n, mClosed, nClosed, 2);
    public static int[][] SpotSquares3(int m, int n, bool mClosed = false, bool nClosed = false) => SpotSquaresInternal(m, n, mClosed, nClosed, 3);

    private static int[][] SpotSquaresInternal(int m, int n, bool mClosed = false, bool nClosed = false, int shift = 0) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => ((j + 1 + shift / 2) % 2) * ((i + 1 + shift % 2) % 2) - 1);

    // both sides?
    private static (int i, int j)[][][] triangleMaps = new[] {
            new[] // slash triangle
            {
                new[] { (0, 1), (1, 1), (1, 0) },
                new[] { (1, 0), (0, 0), (0, 1) },
            },
            new[] // backslash triangle
            {
                new[] { (1, 1), (1, 0), (0, 0) },
                new[] { (0, 0), (0, 1), (1, 1) },
            },
        };

    private static (int i, int j)[][][] hedgehogTriangleMaps = new[] {
            new[] { new[] { (0, 1), (1, 1), (1, 0) } },
            new[] { new[] { (0, 0), (1, 1), (1, 0) } },
            new[] { new[] { (0, 0), (0, 1), (1, 1) } },
            new[] { new[] { (0, 0), (0, 1), (1, 0) } },
        };

    private static (int i, int j)[][][] squareDiagonalD2Maps = new[] {
        new[]
        {
            new[] { (0, 0), (0, 1) },
            new[] { (0, 1), (1, 1) },
            new[] { (1, 1), (1, 0) },
            new[] { (1, 0), (0, 0) },
            new[] { (0, 0), (1, 1) },
            new[] { (0, 1), (1, 0) },
        }
    };

    private static (int i, int j)[][][] squareMaps = new[] {
        new[] // square
        {
            new[] { (0, 0), (0, 1), (1, 1), (1, 0) }
        },
        new[] // shifted square
        {
            new[] { (0, 1), (0, 2), (1, 1), (1, 0) }
        },
    };

    private static int[][] GetConvexes(int m, int n, bool mClosed, bool nClosed, (int i, int j)[][][] maps, Func<int, int, int>? mapFn = null, bool both = false, bool reverse = false)
    {
        var fn = mapFn ?? ((_, _) => 0);
        int num(int i, int j) => n * (i % m) + j % n;
        var mm = mClosed ? m : m - 1;
        var nn = nClosed ? n : n - 1;

        var convexes = (mm, nn).SelectRange((i, j) => fn(i, j) >= 0
            ? maps[fn(i, j)].Select(line => line.Select(mp => num(i + mp.i, j + mp.j)).ToArray())
            : new int[0][]).ManyToArray();

        convexes = both
            ? convexes.Concat(convexes.ReverseConvexes()).ToArray()
            : convexes;

        return reverse ? convexes.ReverseConvexes().ToArray() : convexes;
    }
}
