using Aspose.ThreeD;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;

namespace AI.Libraries;

public delegate double NFunc(double x);
public delegate double NDerFunc(double x, double f);

public static class NFuncs
{
    public static NFunc GetDampingFn(double c)
    {
        if (c > 0)
        {
            var a = (1 - c);

            return x => a * x;
        }

        return x => x;
    }

    public static NFunc GetSigmoidFn(double alfa)
    {
        var a = -2 * alfa;

        return x => 1 / (1 + Math.Exp(a * x));
    }

    public static NDerFunc GetDerSigmoidFn()
    {
        return (_, f) => f * (1 - f);
    }

    public static NFunc GetTanhFn(double a)
    {
        return x => Math.Tanh(x * a);
    }

    public static NDerFunc GetDerTanhFn(double a)
    {
        return (_, f) => a * (1 - f * f);
    }

    public static NFunc GetSinFn(double a)
    {
        return x => 0.01 + Math.Sin(x * a);
    }

    public static NFunc GetSinAFn(double aa, double a)
    {
        return x => a + Math.Sin(x * aa);
    }

    public static NDerFunc GetDerSinFn(double a)
    {
        return (x, _) => a * Math.Cos(x * a);
    }

    public static NFunc GetGaussianFn(double a)
    {
        return x => Math.Exp(-(x * a).Pow2());
    }

    public static NDerFunc GetDerGaussianFn(double a)
    {
        return (x, f) => -2 * x * a * f;
    }

    public static NFunc GetSincFn(double a)
    {
        return x => x.Abs() < Values.Epsilon6 ? 1 : Math.Sin(x * a) / (x * a);
    }

    public static NDerFunc GetDerSincFn(double a)
    {
        return (x, f) => x.Abs() < Values.Epsilon6 ? 0 : (Math.Cos(x * a) - f) / (x * a);
    }

    public static NFunc GetBaseWeight(double a, double b) => x => a + x * (b - a);
}
