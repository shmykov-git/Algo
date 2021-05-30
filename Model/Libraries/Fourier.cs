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
    }
}
