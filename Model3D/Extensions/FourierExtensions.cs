using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Fourier;
using Model.Libraries;

namespace Model3D.Extensions
{
    public static class FourierExtensions
    {
        public static Shape[] ToShapes(this IEnumerable<Fr> frs, int count = 256, double? volume = 0.05, bool triangulateOnly = false) =>
            FourierShapes.Series(frs.ToArray(), volume, triangulateOnly, count);

        public static Shape ToShape(this IEnumerable<Fr> frs, int count = 256, double? volume = 0.05, bool triangulateOnly = false) =>
            FourierShapes.Series(frs.ToArray(), volume, triangulateOnly, count).ToSingleShape();

        public static Shape ToLineShape(this IEnumerable<Fr> frs, int count = 256, double size = 1) =>
            FourierShapes.SingleSeries(frs.ToArray(), count).ToLines(size);

        public static Shape ToFormulaShape(this IEnumerable<Fr> frs) =>
            FourierShapes.SeriesFormula(frs.ToArray());

        public static Fr[] Perfecto(this IEnumerable<Fr> frs)
        {
            return frs
                .GroupBy(k => k.n + k.dn)
                .Select(gk => new Fr()
                {
                    n = gk.Select(kk => kk.n).First(), dn = gk.Select(kk => kk.dn).First(), im = gk.Sum(kk => kk.im),
                    r = gk.Sum(kk => kk.r)
                })
                .Where(k => k.r != 0 || k.im != 0)
                .Where(k => k.n + k.dn != 0)
                .OrderBy(k => k.n + k.dn)
                .ToArray();
        }
    }
}