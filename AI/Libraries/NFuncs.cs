using Model.Tools;

namespace AI.Libraries;

public delegate float NFunc(float x);

public static class NFuncs
{
    public static NFunc GetDampingFn(float c) => x => c * x;

    public static NFunc GetSigmoidFn(float alfa) => x => 1 / (1 + MathF.Exp(-2 * alfa * x));

    public static NFunc GetBaseWeight(float a, float b) => x => a * x + b;
}
