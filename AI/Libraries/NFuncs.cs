using AI.Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;

namespace AI.Libraries;

public delegate double NFunc(N n);

public static class NFuncs
{
    public static NFunc GetSigmoidFn(double a)
    {
        var aa = -2 * a;

        return n => 1 / (1 + Math.Exp(aa * n.xx));
    }

    public static NFunc GetDerSigmoidFn(double a)
    {
        return n => a * n.f * (1 - n.f);
    }

    public static NFunc GetTanhFn(double a)
    {
        return n => Math.Tanh(n.xx * a);
    }

    public static NFunc GetDerTanhFn(double a)
    {
        return n => a * (1 - n.f * n.f);
    }

    public static NFunc GetSinFn(double a)
    {
        return n => Math.Sin(n.xx * a);
    }

    public static NFunc GetSinBFn(double a, double bias)
    {
        return n => bias + Math.Sin(n.xx * a);
    }

    public static NFunc GetDerSinFn(double a)
    {
        return n => a * Math.Cos(n.xx * a);
    }

    public static NFunc GetGaussianFn(double a)
    {
        return n => Math.Exp(-(n.xx * a).Pow2());
    }

    public static NFunc GetDerGaussianFn(double a)
    {
        return n => -2 * n.xx * a * n.f;
    }

    public static NFunc GetSincFn(double a)
    {
        return n => n.xx.Abs() < Values.Epsilon6 ? 1 : Math.Sin(n.xx * a) / (n.xx * a);
    }

    public static NFunc GetDerSincFn(double a)
    {
        return n => n.xx.Abs() < Values.Epsilon6 ? 0 : (Math.Cos(n.xx * a) - n.f) / (n.xx * a);
    }

    public static Func<double, double> GetBaseWeight(double a, double b) => x => a + x * (b - a);

    public static Func<int, bool> Evens => i => i % 2 == 0;
    public static Func<int, bool> EachN0(int n) => i => i % n == 0;
    public static Func<int, bool> None => _ => true;
}
