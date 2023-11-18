using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Model.Tools;

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

    private readonly static double q5 = Math.Sqrt(5);
    private readonly static double alfa = (-1 + q5) / 2;
    private readonly static double betta = 1 - alfa;
    private readonly static double gamma = -alfa / (alfa * alfa - betta * betta);
    private readonly static double delta = -betta / (alfa * alfa - betta * betta);

    public static IEnumerable<(double x, double fx)> Gold(double x0, double dx0, double epsilon, Func<double, double> func, double? maxDx = null, bool debug = false)
    {
        if (dx0 <= 0)
            throw new ArgumentException("dx0 should be more 0");

        var fn = func;

        if (debug)
        {
            int count = 0;

            fn = x =>
            {
                var f = func(x);
                Debug.WriteLine($"Gold {++count}: ({x}, {f})");

                return f;
            };
        }

        double GetX1(double a, double b) => alfa * a + betta * b;
        double GetX2(double a, double b) => betta * a + alfa * b;
        double GetA(double x1, double x2) => gamma * x1 - delta * x2;
        double GetB(double x1, double x2) => delta * x1 - gamma * x2;

        var a = x0;
        var b = x0 + dx0;
        var x1 = GetX1(a, b);
        var x2 = GetX2(a, b);

        var fa = fn(a);
        yield return (a, fa);

        var fx1 = fn(x1);
        yield return (x1, fx1);

        var fx2 = fn(x2);
        yield return (x2, fx2);

        var fb = fn(b);
        yield return (b, fb);

        void LessThenA()
        {
            (a, x1, x2, b) = (GetA(a, x1), a, x1, b);
            (fa, fx1, fx2, fb) = (fn(a), fa, fx1, fb);
        }

        void MoreThenALessThenX2()
        {
            (a, x1, x2, b) = (a, GetX1(a, x2), x1, x2);
            (fa, fx1, fx2, fb) = (fa, fn(x1), fx1, fx2);
        }

        void MoreThenX1LessThenB()
        {
            (a, x1, x2, b) = (x1, x2, GetX2(x1, b), b);
            (fa, fx1, fx2, fb) = (fx1, fx2, fn(x2), fb);
        }

        void MoreThenB()
        {
            (a, x1, x2, b) = (a, x2, b, GetB(x2, b));
            (fa, fx1, fx2, fb) = (fa, fx2, fb, fn(b));
        }

        while((b-a) > epsilon)
        {
            if (fa > fb)
            {
                if (fx1 > fb)
                {
                    if (fx2 > fb)
                    {
                        MoreThenB();
                        yield return (b, fb);

                        if (maxDx.HasValue && (b-x2) > maxDx)
                        {
                            MoreThenX1LessThenB();
                            yield return (x2, fx2);
                        }
                    }
                    else
                    {
                        MoreThenX1LessThenB();
                        yield return (x2, fx2);
                    }
                }
                else
                {
                    if (fx1 > fx2)
                    {
                        MoreThenX1LessThenB();
                        yield return (x2, fx2);
                    }
                    else
                    {
                        MoreThenALessThenX2();
                        yield return (x1, fx1);
                    }
                }
            }
            else
            {
                if (fx2 > fa)
                {
                    if (fx1 > fa)
                    {
                        LessThenA();
                        yield return (a, fa);

                        if (maxDx.HasValue && (x1 - a) > maxDx)
                        {
                            MoreThenALessThenX2();
                            yield return (x1, fx1);
                        }
                    }
                    else
                    {
                        MoreThenALessThenX2();
                        yield return (x1, fx1);
                    }
                }
                else
                {
                    if (fx1 > fx2)
                    {
                        MoreThenX1LessThenB();
                        yield return (x2, fx2);
                    }
                    else
                    {
                        MoreThenALessThenX2();
                        yield return (x1, fx1);
                    }
                }
            }
        }
    }
    
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
