namespace AI.Libraries;

public delegate float NFunc(float x);

public static class NFuncs
{
    public static NFunc GetDampingFn(float c) => x => c * x;

    public static NFunc GetSigmoidFn() => x => 1 / (1 + MathF.Exp(-x));

    public static Func<float, float, float> GetErrorFn() => (x0, x) => (x0 - x) * x * (1 - x);
}
