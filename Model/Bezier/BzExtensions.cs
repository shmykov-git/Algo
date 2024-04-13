using System;
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
        bzs.SelectCirclePair<Bz, Vector2[]>((x, y) => x.la == y.a ? [x.a] : [x.a, x.la]).SelectMany(v => v).ToArray();

    public static Vector2[] ControlPoints(this Bz[] bzs) => bzs.SelectMany(x => x.ps.Skip(1).SkipLast(1)).ToArray();

    public static Func2 ToBz(this Bz[] bzs)
    {
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
}
