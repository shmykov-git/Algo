using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using Model.Extensions;

namespace Model.Bezier;

public abstract class Bezier
{
    protected int n;
    protected int[] bs;

    public Vector2[] ps;

    public Vector2[] points => ps.SkipLast(1).ToArray();

    public Vector2 a { get => ps[0]; set => ps[0] = value; }
    public Vector2 b { get => ps[1]; set => ps[1] = value; }
    public Vector2 c { get => ps[2]; set => ps[2] = value; }
    public Vector2 d { get => ps[3]; set => ps[3] = value; }

    protected Bezier(Vector2[] ps)
    {
        n = ps.Length - 1;
        this.ps = ps;
        bs = (n + 1).SelectRange(k => C(n, k)).ToArray();
    }

    protected Bezier(int n, Vector2[] ps, int[] bs)
    {
        this.n = n;
        this.ps = ps;
        this.bs = bs;
    }

    private BigInteger F(int n) => (n).SelectRange(i => new BigInteger(i + 1)).Aggregate(new BigInteger(1), (a, b) => a * b);

    private int C(int n, int k) => (int)(F(n) / F(k) / F(n - k));

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
