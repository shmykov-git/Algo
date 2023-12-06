using System;
using System.Linq;
using Model.Extensions;

namespace Model3D.Libraries;

public static class Convexes
{
    /// <summary>
    /// m, n - number of points by an axle
    /// shift = 0 or 1
    /// </summary>
    public static int[][] Hedgehog(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (i, j) => (i + j) % 2);

    public static int[][] Hedgehog1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (i, j) => (i + j + 1) % 2);

    public static int[][] DiagonalSquares2D(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareDiagonalD2Maps);

    public static int[][] Squares(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps);

    public static int[][] SquaresBoth(int m, int n, bool mClosed = false, bool nClosed = false) => 
        GetConvexes(m, n, mClosed, nClosed, squareBothSideMaps);

    public static int[][] Triangles(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (_, _) => 0);
    public static int[][] Triangles1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, triangleMaps, (_, _) => 1);

    public static int[][] ChessSquares(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => (i + j) % 2 - 1);
    public static int[][] ChessSquares1(int m, int n, bool mClosed = false, bool nClosed = false) =>
        GetConvexes(m, n, mClosed, nClosed, squareMaps, (i, j) => (i + j + 1) % 2 - 1);

    public static int[][] SpotSquares0(int m, int n, bool mClosed = false, bool nClosed = false) => SpotSquares(m, n, mClosed, nClosed, 0);

    public static int[][] SpotSquares(int m, int n, bool mClosed = false, bool nClosed = false, int shift = 0) =>
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

    private static (int i, int j)[][][] squareDiagonalD2Maps = new[] {
        new[] // square
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
        }
    };

    private static (int i, int j)[][][] squareBothSideMaps = new[] {
        new[] // both sides square
        {
            new[] { (0, 0), (0, 1), (1, 1), (1, 0) },
            new[] { (1, 0), (1, 1), (0, 1), (0, 0) },
        }
    };

    private static int[][] GetConvexes(int m, int n, bool mClosed, bool nClosed, (int i, int j)[][][] maps, Func<int, int, int>? mapFn = null)
    {
        var fn = mapFn ?? ((_, _) => 0);
        int num(int i, int j) => n * (i % m) + j % n;
        var mm = mClosed ? m : m - 1;
        var nn = nClosed ? n : n - 1;

        return (mm, nn).SelectRange((i, j) => fn(i, j) >= 0 
            ? maps[fn(i, j)].Select(line => line.Select(v => num(i + v.i, j + v.j)).ToArray())
            : new int[0][]).ManyToArray();
    }
}
