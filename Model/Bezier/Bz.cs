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

    //public Bz Join2(Bz z)
    //{
    //    var line1 = new Line2(lb, la);
    //    var line2 = new Line2(z.a, z.b);
    //    var c = line1.IntersectionPoint(line2);

    //    return new Bz(la, c, z.a);
    //}

    //public Bz JoinCircleClose(Bz z, double epsilon = Values.Epsilon9)
    //{
    //    var line1 = new Line2(lb, la);
    //    var lOrt1 = new Line2(la, la + line1.AB.Normal);
    //    var line2 = new Line2(z.a, z.b);
    //    var lOrt2 = new Line2(z.a, z.a + line2.AB.Normal);

    //    var (isCrossed, cOrt) = lOrt1.IntersectionPointChecked(lOrt2, epsilon);

    //    if (isCrossed)
    //    {
    //        var r1 = line1.B - cOrt;
    //        var r2 = line2.A - cOrt;
    //        var len = r1.Len;

    //        if ((len - r2.Len).Abs() > epsilon)
    //            throw new ArgumentException("Cannot join as a circle");

    //        var alfa = GetAngle(r1, r2);
    //        var c = line1.IntersectionPoint(line2);
    //        alfa = line1.AB * (cOrt - c) < 0 ? alfa : 2 * Math.PI - alfa;

    //        var l = GetCircleL(alfa);

    //        var bb = line1.B + line1.AB.ToLen(len * l);
    //        var cc = line2.A - line2.AB.ToLen(len * l);

    //        return new Bz(la, bb, cc, z.a);
    //    }
    //    else
    //    {
    //        var checkP = line2.IntersectionPoint(lOrt1);

    //        if ((checkP - line2.A).Len > epsilon)
    //            throw new ArgumentException("Cannot join as a circle");

    //        var l = GetCircleL(Math.PI);
    //        var len = 0.5 * (line1.B - line2.A).Len;

    //        var bb = line1.B + line1.AB.ToLen(len * l);
    //        var cc = line2.A - line2.AB.ToLen(len * l);

    //        return new Bz(la, bb, cc, z.a);
    //    }
    //}



    //public Bz Join3(Bz z, double x = 1) => Join3(z, x, x);

    //public Bz JoinLine(Bz z) => new Bz(la, z.a);

    //public Bz Join3(Bz z, double x, double y, double epsilon = Values.Epsilon9)
    //{
    //    var line1 = new Line2(lb, la);
    //    var line2 = new Line2(z.a, z.b);

    //    var d1 = line2.Distance(line1.B);
    //    var d2 = line1.Distance(line2.A);

    //    if (d1 < epsilon) d1 = d2;
    //    if (d2 < epsilon) d2 = d1;

    //    if (d1 < epsilon)
    //        throw new ArgumentException("Already joined");

    //    var bb = line1.B + line1.AB.ToLen(d1 * x);
    //    var cc = line2.A - line2.AB.ToLen(d2 * y);

    //    return new Bz(line1.B, bb, cc, line2.A);
    //}

    //public static Bz operator +(Bz a, Bz b) => a.Join3(b);

    public Bz ToPower3()
    {
        if (n != 2)
            throw new ApplicationException("Bz is not of power 2");

        return new Bz(a, b + 1d / 3 * (a - b), b + 1d / 3 * (c - b), c);
    }
}
