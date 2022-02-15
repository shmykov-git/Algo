using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Libraries;

namespace Model3D.Extensions
{
    public static class FourierExtensions
    {
        public static Shape[] ToShapes(this IEnumerable<Fr> frs, int count = 256, double? volume = 0.05, bool triangulateOnly = false) =>
            FourierShapes.Series(frs.ToArray(), volume, triangulateOnly, count);

        public static Shape ToShape(this IEnumerable<Fr> frs, int count = 256, double? volume = 0.05, bool triangulateOnly = false) =>
            FourierShapes.Series(frs.ToArray(), volume, triangulateOnly, count).ToSingleShape();

        public static Shape ToLineShape(this IEnumerable<Fr> frs, int count = 256) =>
            FourierShapes.Series(frs.ToArray(), null, false, count).ToSingleShape();

        public static Shape ToFormulaShape(this IEnumerable<Fr> frs) =>
            FourierShapes.SeriesFormula(frs.ToArray());
    }
}