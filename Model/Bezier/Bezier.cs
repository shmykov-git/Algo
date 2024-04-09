using System.Linq;
using System.Net.NetworkInformation;
using System.Numerics;
using Model.Extensions;

namespace Model.Bezier;

public abstract class Bezier
{
    protected int n;
    protected Vector2[] ps;
    protected int[] bs;

    public Vector2[] points => ps.SkipLast(1).ToArray();

    protected Vector2 a => ps[0];
    protected Vector2 b => ps[1];
    protected Vector2 c => ps[2];
    protected Vector2 d => ps[3];

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
