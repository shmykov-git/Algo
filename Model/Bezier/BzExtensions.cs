using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Model.Extensions;
using Model.Libraries;

namespace Model.Bezier;

public static class BzExtensions
{
    public static Bz[] ToBzs(this Vector2[][] ps, bool closed = false)
    {
        Bz GetBz(Vector2[] aa, Vector2[] bb) => aa.Length switch
        {
            0 => throw new ArgumentException("Incorrect length 0"),
            1 => new Bz(aa[0], bb[0]),
            2 => new Bz(aa[0], aa[1], bb[0]),
            3 => new Bz(aa[0], aa[1], aa[2], bb[0]),
            _ => new Bz(aa.Concat(new[] { bb[0] }).ToArray())
        };

        return closed
            ? ps.SelectCirclePair(GetBz).ToArray()
            : ps.SelectPair(GetBz).ToArray();
    }

    public static Vector2[] LinePoints(this Bz[] bzs) =>
        bzs.Where(b => b.IsLine).SelectCirclePair<Bz, Vector2[]>((x, y) => x.la == y.a ? [x.a] : [x.a, x.la]).SelectMany(v => v).ToArray();

    public static Vector2[] ControlPoints(this Bz[] bzs) => bzs.Where(b => b.IsLine).SelectMany(x => x.ps.Skip(1).SkipLast(1)).ToArray();

    public static Func2 ToBz(this IEnumerable<Bz> bzs0)
    {
        var bzs = bzs0.Where(b => b.IsLine).ToArray();

        var n = bzs.Length;

        return x =>
        {
            var k = (int)(x * n);

            if (k >= n)
                k = n - 1;

            var t = x * n - k;

            return bzs[k].B(t);
        };
    }

    public static Func2 ToBz(this Vector2[][] ps, bool closed = false) => ps.ToBzs(closed).ToBz();


    private static double GetAngle(Vector2 a, Vector2 b) => Math.Acos(a.Normed * b.Normed);

    private static double GetCircleL(double alfa) => 4.0 / 3 * Math.Tan(alfa / 4);

    public static Bz Join(this Bz bzA, Bz bzB, BzJoinType type, double alfa = 0, double betta = 0) => bzA.Join(bzB, new BzJoinOptions { Type = type, Alfa = alfa, Betta = betta });

    public static Bz Join(this Bz bzA, Bz bzB, BzJoinOptions options)
    {
        var a = bzA.la;
        var a0 = bzA.IsPoint ? new Vector2(a.x - 1, a.y).Rotate(bzA.alfa0, a) : bzA.lb;
        var b = bzB.a;
        var b1 = bzB.IsPoint ? new Vector2(b.x - 1, b.y).Rotate(bzB.alfa0, b) : bzB.b;

        if (options.Type == BzJoinType.Line)
            return new Bz(a, b);

        var lineA = new Line2(a0.Rotate(options.Alfa, a), a);
        var lineB = new Line2(b, b1.Rotate(options.Betta, b));

        Bz GetBz2(double x, double y)
        {
            var c = a + x * lineA.One;
            var d = b - y * lineB.One;

            return new Bz(a, c, d, b);
        }

        if (options.Type == BzJoinType.PowerTwo)
            return GetBz2(options.x, options.y);

        if (options.Type == BzJoinType.PowerTwoByDistance)
        {
            var dToA = lineA.Distance(b);
            var dToB = lineB.Distance(a);

            if (dToA < options.Epsilon || dToB < options.Epsilon)
                throw new ArgumentException("Zero distance");

            return GetBz2(options.x * dToA, options.y * dToB);
        }

        var (hasCrossed, cross) = lineA.IntersectionPointChecked(lineB, options.Epsilon);

        if (hasCrossed)
        {
            if (options.Type == BzJoinType.PowerOne)
                return new Bz(a, cross, b);

            var ortA = new Line2(a, a + lineA.Normal);
            var ortB = new Line2(b, b + lineB.Normal);
            var ortCross = ortA.IntersectionPoint(ortB);

            if (options.Type == BzJoinType.PowerTwoLikeCircle)
            {
                var rA = a - ortCross;
                var rB = b - ortCross;
                var radius = rA.Len;

                if ((radius - rB.Len).Abs() > options.Epsilon)
                    throw new ArgumentException("No circle");

                var alfa = GetAngle(rA, rB);
                alfa = lineA.AB * (ortCross - cross) < 0 ? alfa : 2 * Math.PI - alfa;
                var L = GetCircleL(alfa);

                return GetBz2(radius * L, radius * L);
            }

            if (options.Type == BzJoinType.PowerTwoLikeEllipse)
            {
                var rA = a - ortCross;
                var rB = b - ortCross;
                var rCross = cross - ortCross;

                if (rA.Len < options.Epsilon || rB.Len < options.Epsilon)
                    throw new ArgumentException("No ellipse");

                var alfa = 2 * GetAngle(rA, rCross);
                alfa = lineA.AB * (ortCross - cross) < 0 ? alfa : 2 * Math.PI - alfa;

                var betta = 2 * GetAngle(rB, rCross);
                betta = lineB.AB * rCross < 0 ? betta : 2 * Math.PI - betta;

                var LAlfa = GetCircleL(alfa);
                var LBetta = GetCircleL(betta);

                return GetBz2(rA.Len * LAlfa, rB.Len * LBetta);
            }
        }
        else
        {
            if (options.Type == BzJoinType.PowerOne)
                throw new ArgumentException("No cross point");

            var ortB = new Line2(b, b + lineB.Normal);

            if (options.Type == BzJoinType.PowerTwoLikeCircle)
            {
                var checkA = lineA.IntersectionPoint(ortB);

                if ((checkA - a).Len > options.Epsilon)
                    throw new ArgumentException("No circle");

                var L = GetCircleL(Math.PI);
                var radius = 0.5 * (b - a).Len;

                return GetBz2(radius * L, radius * L);
            }

            if (options.Type == BzJoinType.PowerTwoLikeEllipse)
            {
                var L = GetCircleL(Math.PI);
                var radius = 0.5 * lineA.Distance(b);

                if (radius < options.Epsilon)
                    throw new ArgumentException("No ellipse");

                return GetBz2(radius * L, radius * L);
            }
        }

        throw new NotImplementedException($"Join {options.Type}");
    }
}
