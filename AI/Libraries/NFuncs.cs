using Model.Tools;

namespace AI.Libraries;

public delegate float NFunc(float x);

public static class NFuncs
{
    public static NFunc GetDampingFn(float c)
    {
        if (c > 0)
        {
            var a = (1 - c);

            return x => a * x;
        }

        return x => x;
    }

    public static NFunc GetSigmoidFn(float alfa)
    {
        var a = -2 * alfa;

        return x => 1 / (1 + MathF.Exp(a * x));
    }

    public static NFunc GetBaseWeight(float a, float b) => x => a * x + b;
}
