using System;
using System.Data;
using System.Linq;
using Model.Extensions;
using Model.Libraries;

namespace Model.Bezier;

public static class BzExtensions
{
    public static Func2 ToBz(this Vector2[][] ps, bool closed = false)
    {
        Bz GetBz(Vector2[] aa, Vector2[] bb) => aa.Length switch
        {
            0 => throw new ArgumentException("Incorrect length 0"),
            1 => new Bz(aa[0], bb[0]),
            2 => new Bz(aa[0], aa[1], bb[0]),
            3 => new Bz(aa[0], aa[1], aa[2], bb[0]),
            _ => new Bz(aa.Concat(new[] { bb[0] }).ToArray())
        };

        var bzs = closed 
            ? ps.SelectCirclePair(GetBz).ToArray() 
            : ps.SelectPair(GetBz).ToArray();

        var n = closed ? ps.Length : ps.Length - 1;

        Vector2 fn(double x)
        {
            var k = (int)(x * n);
            
            if (k >= n)
                k = n - 1;

            var t = x * n - k;

            return bzs[k].B(t);
        }

        return fn;
    }
}
