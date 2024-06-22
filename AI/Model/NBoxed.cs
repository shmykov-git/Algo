namespace AI.Model;

public struct NBoxed
{
    public int i;
    public double[] input;
    public double[] expected;

    public NBoxed(int i, double[] input, double[] expected)
    {
        this.i = i;
        this.input = input;
        this.expected = expected;
    }

    public static implicit operator NBoxed((int i, double[] input, double[] expected) v) => new NBoxed(v.i, v.input, v.expected);
}