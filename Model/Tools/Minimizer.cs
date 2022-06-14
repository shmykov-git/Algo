using MathNet.Numerics;
using System;
using System.Diagnostics;

namespace Model.Tools
{
    public static class Minimizer
    {
        public static double Minimize(Func<double, double> fn, double x0, double tolerance = 1E-8)
        {
            return FindMinimum.OfScalarFunction(fn, x0, tolerance);
        }

        public static (double x, double y) Minimize(Func<double, double, double> fn, (double x, double y) v0, double tolerance = 1E-8)
        {
            return FindMinimum.OfFunction(fn, v0.x, v0.y, tolerance);
        }

        //var count = 0;
        //double fn2(double x)
        //{
        //    count++;
        //    Debug.WriteLine(x);
        //    return fn(x);
        //};

        public static double MinimizeSimple(double x0, double dx0, double epsilon, Func<double, double> fn)
        {
            //todo: optimization

            var x = x0;
            var dx = dx0;
            while (dx > epsilon)
            {
                var y1 = fn(x);
                var y2 = fn(x + dx);
                var y3 = fn(x - dx);

                if (y1 < y2)
                    if (y1 < y3)
                        dx = dx / 2;
                    else
                        x = x - dx;
                else
                    x = x + dx;
            }

            return x;
        }
    }
}
