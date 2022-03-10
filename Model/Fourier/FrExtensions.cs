using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Extensions;

namespace Model.Fourier
{
    public static class FrExtensions
    {
        public static Fr[] Perfecto(this IEnumerable<Fr> frs)
        {
            return frs
                .GroupBy(k => k.n + k.dn)
                .Select(gk => new Fr()
                {
                    n = gk.Select(kk => kk.n).First(),
                    dn = gk.Select(kk => kk.dn).First(),
                    im = gk.Sum(kk => kk.im),
                    r = gk.Sum(kk => kk.r)
                })
                .Where(k => k.r != 0 || k.im != 0)
                .Where(k => k.n + k.dn != 0)
                .OrderBy(k => k.n + k.dn)
                .ToArray();
        }

        public static Fr[] ModifyLast(this Fr[] frs, Action<Fr> action)
        {
            action(frs[^1]);
            return frs;
        }

        public static Fr[] ModifyTwoLasts(this Fr[] frs, Action<Fr, Fr> action)
        {
            action(frs[^2], frs[^1]);
            return frs;
        }

        public static void WriteToDebug(this Fr[] frs, string prefix = null)
        {
            Debug.WriteLine($"{prefix}{frs.SJoin()}");
        }
    }
}