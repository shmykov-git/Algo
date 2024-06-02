using Model.Tools;

namespace AI.Libraries;

public delegate float NFunc(float x);

public static class NFuncs
{
    public static NFunc GetDampingFn(float c) => x => c * x;

    public static NFunc GetSigmoidFn() => x => 1 / (1 + MathF.Exp(-x));

    public static NFunc GetSigmoidDerFn() => s => s * (1 - s);

    public static NFunc GetBaseWeight(float a, float b) => x => a * x + b;
}
