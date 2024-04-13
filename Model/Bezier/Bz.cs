using System;
using Model.Extensions;
using Model.Libraries;

namespace Model.Bezier;

public class Bz : Bezier
{
    public Bz(Vector2 a, Vector2 b) : base(1, [a, b], [1, 1])
    {
    }

    public Bz(Vector2 a, Vector2 b, Vector2 c) : base(2, [a, b, c], [1, 2, 1])
    {
    }

    public Bz(Vector2 a, Vector2 b, Vector2 c, Vector2 d) : base(3, [a, b, c, d], [1, 3, 3, 1])
    {
    }

    public Bz(Vector2[] ps): base(ps)
    {
    }

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

    public Bz Join2(Bz z)
    {
        var line1 = new Line2(lb, la);
        var line2 = new Line2(z.a, z.b);
        var c = line1.IntersectionPoint(line2);

        return new Bz(la, c, z.a);
    }

    public Bz JoinCircle(Bz z, double epsilon = Values.Epsilon9)
    {
        var line1 = new Line2(lb, la);
        var lOrt1 = new Line2(la, la + line1.AB.Normal);
        var line2 = new Line2(z.a, z.b);
        var lOrt2 = new Line2(z.a, z.a + line2.AB.Normal);

        var (isCrossed, cOrt) = lOrt1.IntersectionPointChecked(lOrt2);

        if (isCrossed)
        {
            var r1 = la - cOrt;
            var r2 = z.a - cOrt;
            var len = r1.Len;

            if ((len - r2.Len).Abs() > epsilon)
                throw new ArgumentException("Cannot join as a circle");

            var c = line1.IntersectionPoint(line2);

            var alfa = 2 * GetAngle(r1, c - cOrt);

            var l = GetCircleL(alfa);

            var bb = la + (la - lb).ToLen(len * l);
            var cc = z.a + (z.a - z.b).ToLen(len * l);

            return new Bz(la, bb, cc, z.a);
        }
        else
        {
            var checkP = line2.IntersectionPoint(lOrt1);

            if ((checkP - line2.A).Len > epsilon)
                throw new ArgumentException("Cannot join as a circle");

            var l = GetCircleL(Math.PI / 2);
            var len = 0.5 * (line1.B - line2.A).Len;

            var bb = la + (la - lb).ToLen(len * l);
            var cc = z.a + (z.a - z.b).ToLen(len * l);

            return new Bz(la, bb, cc, z.a);
        }
    }

    public Bz Join3(Bz z, double x) => Join3(z, x, x);

    private double GetAngle(Vector2 a, Vector2 b) => Math.Acos(a.Normed * b.Normed);
    private double GetCircleL(double alfa) => 4.0 / 3 * Math.Tan(alfa / 4);

    public Bz Join3(Bz z, double x, double y)
    {
        ///4 / 3 * Math.Tan(alfa / 4);
        var line1 = new Line2(lb, la);
        var lOrt1 = new Line2(la, la + line1.AB.Normal);

        var line2 = new Line2(z.a, z.b);
        var lOrt2 = new Line2(z.a, z.a + line2.AB.Normal);

        var cnt = lOrt1.IntersectionPoint(lOrt2);

        //var c1 = line1.IntersectionPoint(lOrt1);
        var r1 = la - cnt;
        //var c2 = line2.IntersectionPoint(lOrt2);
        var r2 = z.a - cnt;

        var c = line1.IntersectionPoint(line2);

        var alfa1 = 2*GetAngle(r1, c - cnt);
        var alfa2 = 2*GetAngle(r2, c - cnt);

        var l1 = GetCircleL(alfa1);
        var l2 = GetCircleL(alfa2);

        var bb = la + (la - lb).ToLen(r2.Len*l2);
        var cc = z.a + (z.a - z.b).ToLen(r1.Len*l1);

        //var bb = la + x * (la - lb);
        //var cc = z.a + y * (z.a - z.b);

        return new Bz(la, bb, cc, z.a);
    }

    //public static implicit operator Bz(Vector2 v)
    //{
    //    return new Vector2 { x = v.a, y = v.b };
    //}

    public Bz ToPower3()
    {
        if (n != 2)
            throw new ApplicationException("Bz is not of power 2");

        return new Bz(a, b + 1d / 3 * (a - b), b + 1d / 3 * (c - b), c);
    }
}
