using Model.Extensions;
using System;
using System.Linq;
using System.Numerics;

namespace Model.Libraries
{
    public static class Fourier
    {
        public static Vector2 Exp(double t, params ((double, double) c, double k)[] args) => args.Select(v => ExpC(v.c, t * v.k)).Aggregate((a, b) => a + b).ToV2();

        private static Complex ExpC((double r, double i) c, double k) => new Complex(c.r, c.i) * Complex.Pow(new Complex(Math.E, 0), new Complex(0, k * 2 * Math.PI));


        public static Vector2 Squere(double t) => Exp(t, ((0.1, 0), 3), (RotateN(3), -1));
        public static Vector2 Star(double t) => Exp(t, ((0.25, 0), 4), (RotateN(8), -1));

        public static (double, double) RotateN(double r, double n) => (r * Math.Cos(Math.PI / n), r * Math.Sin(Math.PI / n));
        public static (double, double) RotateN(double n) => (Math.Cos(Math.PI / n), Math.Sin(Math.PI / n));
        public static (double, double) One = (1, 0);
    }
}
