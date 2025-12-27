using Mapster;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model.Fourier
{
    public static class FrExtensions
    {
        public static Fr[] RadiusPerfecto(this IEnumerable<Fr> frs, double r = 1)
        {
            var rr = frs.Sum(fr => fr.r.Abs());

            return frs.Select(fr => fr * (r / rr)).ToArray();
        }

        public static Fr[] DoIf(this Fr[] frs, bool canDo, Func<Fr[], Fr[]> doFn) => canDo ? doFn(frs) : frs.ToArray();

        public static Fr[] GroupMembers(this IEnumerable<Fr> frs)
        {
            return frs
                .GroupBy(k => (k.n + k.dn, k.dis))
                .Select(gk => new Fr()
                {
                    n = gk.Select(kk => kk.n).First(),
                    dn = gk.Select(kk => kk.dn).First(),
                    im = gk.Sum(kk => kk.im),
                    r = gk.Sum(kk => kk.r),
                    dis = gk.Key.dis
                })
                .Where(k => k.r != 0 || k.im != 0)
                .Where(k => k.n + k.dn != 0)
                .OrderBy(k => k.n + k.dn)
                .ToArray();
        }

        public static Fr[] ApplyDiscrete(this Fr[] frs, decimal dis)
        {
            var frsCopy = frs.Adapt<Fr[]>();
            frsCopy.ForEach(fr => fr.dis = (double)dis);

            return frsCopy.ToArray();
        }

        public static Fr[] Modify(this Fr[] frs, Action<Fr> action)
        {
            frs.ForEach(fr => action(fr));

            return frs;
        }

        public static Fr[] ModifyLast(this Fr[] frs, Action<Fr> action)
        {
            action(frs[^1]);
            return frs;
        }

        public static Fr[] ModifyTwoLasts(this Fr[] frs, Action<Fr, Fr> action)
        {
            var frsCopy = frs.Adapt<Fr[]>();

            if (frsCopy.Length == 0)
                Debugger.Break();

            action(frsCopy[^2], frsCopy[^1]);
            return frsCopy;
        }

        public static void WriteToDebug(this Fr[] frs, string prefix = null)
        {
            Debug.WriteLine($"{prefix}{frs.SJoin()}");
        }
    }
}