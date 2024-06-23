using Model;
using Model.Extensions;

namespace AI.Libraries;

public static class NValues
{
    public readonly static Matrix SobelMatix = new Matrix([[1, 0, -1], [2, 0, -2], [1, 0, -1]]);
    public static (int min, int max) SobelMatrixBounds = (-4, 4);

    public readonly static Matrix SumMatrix3 = new Matrix([[1, 1, 1], [1, 1, 1], [1, 1, 1]]);
    public static (int min, int max) SumMatrix3Bounds = (0, 9);

    public readonly static Matrix SumMatrix5 = new Matrix([[1, 1, 1, 1, 1], [1, 1, 1, 1, 1], [1, 1, 1, 1, 1]]);
    public static (int min, int max) SumMatrix5Bounds = (0, 25);

    public static Matrix SumMatrix(int n) => new Matrix((n).Range().Select(_ => (n).Range(_ => 1.0).ToArray()).ToArray());
    public static (int min, int max) SumMatrixBounds(int n) => (0, n * n);

    public static double Boxed(double x, double maxX, double m) => Boxed(x, (0, maxX), m);
    public static double Boxed(double x, (double minX, double maxX) r, double m) => 0.5 * (1 - m) + (x - r.minX) * m / (r.maxX - r.minX);
    public static double Unboxed(double f, (double minX, double maxX) r, double m) => Math.Round((f - 0.5 * (1 - m)) * (r.maxX - r.minX) / m + r.minX);
    public static double Unboxed(double f, double maxX, double m) => Unboxed(f, (0, maxX), m);
    public static int Unboxed(double f, (int minX, int maxX) r, double m) => (int)Unboxed(f, ((double)r.minX, r.maxX), m);
    public static int Unboxed(double f, int maxX, double m) => (int)Unboxed(f, (0, (double)maxX), m);
}
