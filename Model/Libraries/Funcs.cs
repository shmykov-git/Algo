using System;
using Model.Extensions;

namespace Model.Libraries
{
    public delegate double Func1(double x);

    public static class Funcs
    {
        public static Func1 NormalDistribution() => x => Math.Exp(-x * x) / Math.Sqrt(Math.PI);

        public static Func1 ParametricNormDistribution(double mu, double sigma) 
        {
            var fi = NormalDistribution();

            return x => fi((x - mu) / sigma) / sigma; 
        }

        public static Func1 BackParabola(double a) => x => x.Abs() < 0.00001 ? 0 : a / (x * x);

        public static Func1 Line(double a = 0, double b = 0) => x => a * x + b;
        public static Func1 Parabola(double k = 1) => x => (k * x).Pow2();
        public static Func1 Circle(double r = 1) => x => r - Math.Sqrt(r * r - x * x);
    }
}
