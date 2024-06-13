using Model.Tools;

namespace AI.Libraries;

public delegate double NFunc(double x);

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

    public static NFunc GetSigmoidDerFFn()
    {
        return f => f * (1 - f);
    }

    public static NFunc GetBaseWeight(double a, double b) => x => a * x + b;
}
