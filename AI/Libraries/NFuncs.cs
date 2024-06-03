using Model.Tools;

namespace AI.Libraries;

public delegate float NFunc(float x);

public static class NFuncs
{
    public static NFunc GetDampingFn(float c) => x => c * x;

    public static NFunc GetSigmoidFn(float alfa) => x => 
    {
        var y = 1 / (1 + MathF.Exp(-2 * alfa * x));

        //if (y < 0.001f)
        //    return 0.001f;

        //if (y > 0.999f)
        //    return 0.999f;

        return y;
    };

    public static NFunc GetBaseWeight(float a, float b) => x => a * x + b;
}
