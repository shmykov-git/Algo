using AI.Exceptions;
using Model;
using Model.Extensions;

namespace AI.Libraries;

public static class NValues
{
    public readonly static int[][] SobelMatrix =
    [
        [1, 0, -1],
        [2, 0, -2],
        [1, 0, -1]
    ];

    public static int[][] ContrastMatrix(int n) =>
    [
        [1, n, 1],
        [n, n+n, n],
        [1, n, 1]
    ];

    public static double Boxed(double x, double maxX, double m) => Boxed(x, (0, maxX), m);

    public static double Boxed(double x, (double minX, double maxX) r, double m)
    {
        var y = 0.5 * (1 - m) + (x - r.minX) * m / (r.maxX - r.minX);

        if (y < 0 || y > 1)
            throw new AlgorithmException("incorrect boxed");

        return y;
    }

    public static double Unboxed(double f, (double minX, double maxX) r, double m)
    {
        return Math.Round((f - 0.5 * (1 - m)) * (r.maxX - r.minX) / m + r.minX);
    }

    public static double Unboxed(double f, double maxX, double m) => Unboxed(f, (0, maxX), m);
    public static int Unboxed(double f, (int minX, int maxX) r, double m) => (int)Unboxed(f, ((double)r.minX, r.maxX), m);
    public static int Unboxed(double f, int maxX, double m) => (int)Unboxed(f, (0, (double)maxX), m);
}
