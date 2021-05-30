using System.Linq;

namespace Model
{
    public static class Polinom
    {
        public static double Fn(double x, params double[] coefs) => coefs.Aggregate((a, b) => a * x + b);
        public static double RootFn(double x, params double[] roots) => roots.Aggregate(1.0, (v, root) => v * (x - root));
        public static double ScaledRootFn(double x, params double[] roots) => RootFn(x, roots) / roots.Length;
    }
}
