using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Libraries;

namespace Model3D.Extensions
{
    public static class FourierExtensions
    {
        public static Shape ToShape(this IEnumerable<Fr> frs, double? volume = 0.05, int count = 256) =>
            FourierShapes.Series(frs.ToArray(), volume, count).ToSingleShape();

        public static Shape ToFormulaShape(this IEnumerable<Fr> frs) =>
            FourierShapes.SeriesFormula(frs.ToArray());
    }
}