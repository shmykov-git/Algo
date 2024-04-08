using Model.Extensions;

namespace Model.Bezier;

public abstract class Bezier
{
    protected int n;
    protected Vector2[] ps;
    protected double[] bs;

    protected Bezier(int n, Vector2[] ps, double[] bs)
    {
        this.n = n;
        this.ps = ps;
        this.bs = bs;
    }

    private double Pow(double x, int power) => power switch
    {
        0 => 1,
        1 => x,
        2 => x * x,
        3 => x * x * x,
        _ => x.Pow(power)
    };

    public Vector2 B(double t) => (n + 1).SelectRange(k => bs[k] * ps[k] * Pow(t, k) * Pow(1 - t, n - k)).Sum();
}
