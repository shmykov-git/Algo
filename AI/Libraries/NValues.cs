namespace AI.Libraries;

public static class NValues
{
    public static double Boxed(double x, double maxX, double m) => 0.5 * (1 - m) + x * m / maxX;
    public static double Unboxed(double f, double maxX, double m) => Math.Round((f - 0.5 * (1 - m)) * maxX / m);
    public static int Unboxed(double f, int maxX, double m) => (int)Unboxed(f, (double)maxX, m);
}
