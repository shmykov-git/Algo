using Aspose.ThreeD;
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

    public static NFunc GetTanhFn(double alfa)
    {
        return x => Math.Tanh(x / alfa);
    }

    public static NDerFunc GetDerTanhFn()
    {
        return (_, f) => (1 - f * f);
    }

    public static NFunc GetSinFn(double alfa, double power)
    {
        var a = alfa * power;
        return x => 0.01 + Math.Sin(x * a);
    }

    public static NFunc GetSinCFn(double alfa, double power, double c)
    {
        var a = alfa * power;
        return x => c + Math.Sin(x * a);
    }

    public static NDerFunc GetDerSinFn(double alfa, double power)
    {
        var a = alfa * power;
        return (x, _) => a * Math.Cos(x * a);
    }

    public static NFunc GetBaseWeight(double a, double b) => x => a * x + b;
}
