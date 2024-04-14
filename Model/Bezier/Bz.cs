using System;
using System.Linq;
using System.Numerics;
using Model.Extensions;

namespace Model.Bezier;

public class Bz
{
    public double alfa0 = 0;
    public int[] bs;
    public Vector2[] ps;

    public bool IsPoint => n == 0;
    public bool IsLine => n > 0;

    public int n => ps.Length - 1;

    public Vector2 a { get => ps[0]; set => ps[0] = value; }
    public Vector2 b { get => ps[1]; set => ps[1] = value; }
    public Vector2 c { get => ps[2]; set => ps[2] = value; }
    public Vector2 d { get => ps[3]; set => ps[3] = value; }

    public Vector2 la { get => ps[^1]; set => ps[^1] = value; }
    public Vector2 lb { get => ps[^2]; set => ps[^2] = value; }

    public Bz(Vector2[] ps)
    {
        this.ps = ps;
        bs = (n + 1).SelectRange(k => C(n, k)).ToArray();
    }

    private Bz(Vector2[] ps, int[] bs)
    {
        this.ps = ps;
        this.bs = bs;
    }

    public Bz(Vector2 a, double alfa0 = 0) : this([a], [1])
    {
        this.alfa0 = alfa0;
    }

    public Bz(Vector2 a, Vector2 b) : this([a, b], [1, 1])
    {
    }

    public Bz(Vector2 a, Vector2 b, Vector2 c) : this([a, b, c], [1, 2, 1])
    {
    }

    public Bz(Vector2 a, Vector2 b, Vector2 c, Vector2 d) : this([a, b, c, d], [1, 3, 3, 1])
    {
    }


    public Vector2 B(double t) => (n + 1).SelectRange(k => bs[k] * ps[k] * Pow(t, k) * Pow(1 - t, n - k)).Sum();



    public bool CanJoin2(Bz z, double curveRatio = 0.3)
    {
        var line1 = new Line2(lb, la);
        var line2 = new Line2(z.a, z.b);
        var (hasPoint, c) = line1.IntersectionPointChecked(line2);

        if (!hasPoint)
            return false;

        var curveLen = Math.Min((c - line1.B).Len, (line2.B - c).Len);
        var minCurveLen = curveRatio * Math.Min(line1.AB.Len, line2.AB.Len);

        if (curveLen < minCurveLen)
            return false;

        var correctSide =
            line1.AB.MultS(c - line1.B).Sgn() == 1 && 
            line2.AB.MultS(line2.B - c).Sgn() == 1;

        return correctSide;
    }

    public Bz ToPower3()
    {
        if (n != 2)
            throw new ApplicationException("Bz is not of power 2");

        return new Bz(a, b + 1d / 3 * (a - b), b + 1d / 3 * (c - b), c);
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
}
