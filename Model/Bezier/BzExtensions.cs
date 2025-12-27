using Meta.Model;
using Model.Extensions;
using Model.Libraries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Model.Bezier;

public static class BzExtensions
{
    private const double zeroDistance = 0.25;

    private static (BzJoinType type, double mult)[] power2s = [(BzJoinType.PowerTwo, 1), (BzJoinType.PowerTwoHalf, 0.5), (BzJoinType.PowerTwoDouble, 2)];
    private static (BzJoinType type, double mult)[] power2Distances = [(BzJoinType.PowerTwoByDistance, 1), (BzJoinType.PowerTwoByDistanceHalf, 0.5), (BzJoinType.PowerTwoByDistanceDouble, 2)];
    private static Dictionary<BzJoinType, double> power2Dic = power2s.ToDictionary(v => v.type, v => v.mult);
    private static Dictionary<BzJoinType, double> power2DistanceDic = power2Distances.ToDictionary(v => v.type, v => v.mult);

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

    public static Func2 ToFn(this IEnumerable<Bz> bzs0)
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

    private static (int x, int y) SgnZ(Vector2 x, double epsilon = Values.Epsilon9) => (x.x.SgnZ(epsilon), x.y.SgnZ(epsilon));

    public static Func2 ToBz(this Vector2[][] ps, bool closed = false) => ps.ToBzs(closed).ToFn();

    private static double GetCircleL(double alfa) => 4.0 / 3 * Math.Tan(alfa / 4);

    private static (double aAlfa, double bAlfa) CalcLeftAlfa0(Bz az, Bz bz, double epsilon)
    {
        var any = 0;
        var right = 0;
        var left = Math.PI;
        var up = Math.PI / 2;
        var down = -Math.PI / 2;
        var aOut = az.la;
        var bIn = bz.a;

        if (az.IsLine && bz.IsLine)
            return (0, 0);

        var d = SgnZ(bIn - aOut, epsilon);

        if (az.IsPoint && bz.IsPoint)
        {
            return d switch
            {
                (1, 1) => (right, down),
                (1, -1) => (down, left),
                (-1, 1) => (up, right),
                (-1, -1) => (left, up),
            };
        }

        if (az.IsLine)
        {
            var aLine = az.OutLine();
            var lr = aLine.IsLeft(bIn) ? 'L' : 'R';

            return (lr, d.x, d.y) switch
            {
                ('L', 1, 1) => (any, down),
                ('R', 1, 1) => (any, left),

                ('L', 1, -1) => (any, left),
                ('R', 1, -1) => (any, up),

                ('L', -1, 1) => (any, right),
                ('R', -1, 1) => (any, down),

                ('L', -1, -1) => (any, up),
                ('R', -1, -1) => (any, right),
            };
        }

        if (bz.IsLine)
        {
            var bLine = bz.InLine();
            var lr = bLine.IsLeft(aOut) ? 'L' : 'R';

            return (lr, d.x, d.y) switch
            {
                ('L', 1, 1) => (up, any),
                ('R', 1, 1) => (right, any),

                ('L', 1, -1) => (right, any),
                ('R', 1, -1) => (down, any),

                ('L', -1, 1) => (left, any),
                ('R', -1, 1) => (up, any),

                ('L', -1, -1) => (down, any),
                ('R', -1, -1) => (left, any),
            };
        }

        throw new AlgorithmException("Cannot return here");
    }

    public static Bz Join(this Bz bzA, Bz bzB, BzJoinType type, double alfa = 0, double betta = 0) => bzA.Join(bzB, new BzJoinOptions { Type = type, Alfa = alfa, Betta = betta });

    public static Bz Join(this Bz bzA, Bz bzB, BzJoinOptions options)
    {
        var a = bzA.la;
        var b = bzB.a;

        if (options.Type == BzJoinType.Line)
            return new Bz(a, b);

        var (alfa0, betta0) = CalcLeftAlfa0(bzA, bzB, options.Epsilon);
        var lineA = bzA.OutLine(options.Alfa, alfa0);
        var lineB = bzB.InLine(options.Betta, betta0);

        Bz GetBz2(double x, double y)
        {
            var c = a + options.x * x * lineA.One;
            var d = b - options.y * y * lineB.One;

            return new Bz(a, c, d, b);
        }

        if (power2Dic.TryGetValue(options.Type, out var multP2))
            return GetBz2(multP2, multP2);

        if (power2DistanceDic.TryGetValue(options.Type, out var multDP2))
        {
            var dToA = lineA.Distance(b);
            var dToB = lineB.Distance(a);

            var dA = dToA < options.Epsilon ? zeroDistance : dToA;
            var dB = dToB < options.Epsilon ? zeroDistance : dToB;

            return GetBz2(multDP2 * dA, multDP2 * dB);
        }

        var (hasCrossed, cross) = lineA.IntersectionPointChecked(lineB, options.Epsilon);

        if (hasCrossed)
        {
            if (options.Type == BzJoinType.PowerOne)
                return new Bz(a, cross, b);

            var ortA = new Line2(a, a + lineA.Normal);
            var ortB = new Line2(b, b + lineB.Normal);
            var ortCross = ortA.IntersectionPoint(ortB);

            if (options.Type == BzJoinType.PowerTwoLikeEllipse)
            {
                var rA = a - ortCross;
                var rB = b - ortCross;

                if (rA.Len < options.Epsilon || rB.Len < options.Epsilon)
                    throw new ArgumentException("No ellipse");

                var alfa = rA.Angle(rB);
                alfa = lineA.AB * (ortCross - cross) < 0 ? alfa : 2 * Math.PI - alfa;
                var L = GetCircleL(alfa);

                return GetBz2(rB.Len * L, rA.Len * L);
            }
        }
        else
        {
            if (options.Type == BzJoinType.PowerOne)
                throw new ArgumentException("No cross point");

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
