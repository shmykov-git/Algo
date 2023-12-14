using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model;
using Model.Extensions;
using Model.Fourier;
using Model.Libraries;

namespace Model3D.Extensions
{
    public static class FourierExtensions
    {
        public static Shape ToSingleConvexShape(this IEnumerable<Fr> frs, int count = 256, bool radiusPerfecto = false, decimal? dis = null) => radiusPerfecto 
            ? frs.ToArray().DoIf(dis != null, v => v.ApplyDiscrete(dis.Value)).RadiusPerfecto().ToPolygon(count).ToSingleConvexShape() 
            : frs.ToArray().DoIf(dis != null, v => v.ApplyDiscrete(dis.Value)).ToPolygon(count).ToSingleConvexShape();

        public static Polygon ToPolygon(this IEnumerable<Fr> frs, int count = 256) => Polygons.FourierSeries(count, frs.GroupMembers());

        public static Shape[] ToShapes(this IEnumerable<Fr> frs, int count = 256, double? volume = 0.05, double pointPrecision = 0.01, int[] indices = null, bool triangulateOnly = false) =>
            FourierShapes.Series(frs.ToArray(), volume, triangulateOnly, count, pointPrecision, indices);

        public static Shape ToShape(this IEnumerable<Fr> frs, int count = 256, double? volume = 0.05, double pointPrecision = 0.01, int[] indices = null, bool triangulateOnly = false, decimal? dis = null) =>
            FourierShapes.Series(frs.ToArray().DoIf(dis != null, v => v.ApplyDiscrete(dis.Value)), volume, triangulateOnly, count, pointPrecision, indices).ToSingleShape();

        public static Shape ToLineShape(this IEnumerable<Fr> frs, int count = 256, double size = 1) =>
            FourierShapes.SingleSeries(frs.ToArray(), count).ToLines(size);

        public static Shape ToNumShape(this IEnumerable<Fr> frs, int count = 256, double size = 1) =>
            FourierShapes.SingleSeries(frs.ToArray(), count).ToNumSpots3(size) + FourierShapes.SingleSeries(frs.ToArray(), count).ToLines(size);

        public static Shape ToNumShapeR90(this IEnumerable<Fr> frs, int count = 256, double size = 1) =>
            FourierShapes.SingleSeries(frs.ToArray(), count).Rotate(-Math.PI / 2).ToNumSpots3(size) + FourierShapes.SingleSeries(frs.ToArray(), count).Rotate(-Math.PI / 2).ToLines(size);

        public static Shape ToFormulaShape(this IEnumerable<Fr> frs) =>
            FourierShapes.SeriesFormula(frs.ToArray());

    }
}