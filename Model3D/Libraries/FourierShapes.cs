using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Meta;
using Model.Extensions;
using Model.Fourier;
using Model.Tools;
using Model3D.Extensions;
using Model3D.Tools;

namespace Model.Libraries
{
    public static class FourierShapes
    {
        private static Vectorizer vectorizer = DI.Get<Vectorizer>();

        public static Shape Square(double a = 0.11, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (3, a), (-1, 1))
                .Condition(fill, p => p.Fill()).TurnOut().ToShape3()
                .Rotate(Math.PI / 4).Perfecto();

        public static Shape Star(double a = 0.15, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (4, a), (-1, 1))
                .Condition(fill, p => p.Fill()).TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();

        public static Shape Polygon(int n, double a = 0.1, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (n-1, 6*a/(n-1)), (-1, 1))
                .Condition(fill, p => p.Fill()).TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();

        public static Shape Series3(int an, int bn, double a, double b, int count, bool fill, double da = 0, double db = 0) => Polygons
            .FourierSeries(count, (an, a, da), (bn, b, db), (-1, 1))
            .Condition(fill, p => p.Fill())
            .TurnOut().ToShape3().Rotate(Math.PI / 2).Perfecto();

        public static Shape[] Series(Fr[] members, double? volume = 0.05, bool triangulateOnly = false, int count = 256,
            double pointPrecision = 0.01, int[] indices = null)
        {
            var polygon = Polygons.FourierSeries(count, members.Perfecto());
            var polygons = Splitter.FindPerimeter(polygon, pointPrecision);

            if (indices != null)
                polygons = indices.Select(i => polygons[i]).ToArray();

            var shapes = polygons.Select(p => (triangulateOnly ? p.ToShape(null, true) : p.ToShape(volume)).Rotate(Math.PI / 2)).ToArray();

            var size = polygons.Select(p => p.ToShape().Rotate(Math.PI / 2)).ToSingleShape().Size;

            var maxXY = Math.Max(size.x, size.y);

            shapes.Index().ForEach(i =>
            {
                shapes[i] = shapes[i].Scale(1 / maxXY, 1 / maxXY, 1);
            });

            return shapes;
        }

        public static Shape SingleSeries(Fr[] members, int count = 256)
        {
            return Polygons.FourierSeries(count, members.Perfecto()).ToShape().Rotate(Math.PI / 2).Adjust();
        }

        public static Shape SearchSeries(Fr[] main, double a, double b, int fromI, int toI, int fromJ, int toJ, int count = 100,
            double da = 0, double db = 0)
        {
            var lenI = toI - fromI + 1;
            var lenJ = toJ - fromJ + 1;

            return (
                (lenI, lenJ).SelectRange((i, j) => (i: i + fromI, j: j + fromJ))
                .Select(v =>
                    FourierShapes.SingleSeries(main.Concat(new Fr[] { (v.i, a, da), (v.j, b, db) }).ToArray(), count).Mult(0.8).Move(v.j, lenI - v.i, 0)
                        .ToLines(2, Color.Blue)
                ).ToSingleShape() +
                (lenI).SelectRange(i =>
                    vectorizer.GetText($"{i + fromI}", 50, "Arial", 1, 1, false).Centered().Mult(0.01).Move(fromJ - 2, lenI - (i + fromI), 0).ToLines(3, Color.Red)).ToSingleShape() +
                (lenJ).SelectRange(j =>
                    vectorizer.GetText($"{j + fromJ}", 50, "Arial", 1, 1, false).Centered().Mult(0.01).Move(j + fromJ, toI + lenI + 2, 0).ToLines(3, Color.Red)).ToSingleShape()
            ).Perfecto();
        }

        public static Shape SearchSeriesOffset(Fr[] main, int frI, int frJ, double step = 0.1, int count = 100)
        {
            var fromI = -10;
            var fromJ = -10;
            var toI = 10;

            var lenI = (int)(2/step) + 1;
            var lenJ = (int)(2/step) + 1;

            Fr[] Apply(int i, int j)
            {
                main[frI].dn = i * 0.1;
                main[frJ].dn = j * 0.1;
                return main;
            }

            return (
                (lenI, lenJ).SelectRange((i, j) => (i: i + fromI, j: j + fromJ))
                .Select(v =>
                    FourierShapes.SingleSeries(Apply(v.i, v.j), count).Mult(0.8).Move(v.j, lenI - v.i, 0)
                        .ToLines(2, Color.Blue)
                ).ToSingleShape() +
                (lenI).SelectRange(i =>
                    vectorizer.GetText($"{i + fromI}", 50, "Arial", 1, 1, false).Centered().Mult(0.01).Move(fromJ - 2, lenI - (i + fromI), 0).ToLines(3, Color.Red)).ToSingleShape() +
                (lenJ).SelectRange(j =>
                    vectorizer.GetText($"{j + fromJ}", 50, "Arial", 1, 1, false).Centered().Mult(0.01).Move(j + fromJ, toI + lenI + 2, 0).ToLines(3, Color.Red)).ToSingleShape()
            ).Perfecto();
        }

        public static Shape Just1(double a = 0.1, double b = 0.2, int count = 100) => Series3(4, -3, a, b, count, true);

        public static Shape Just2(double a = 0.1, double b = 0.2, int count = 100) => Series3(2, -3, a, b, count, true);

        public static Shape Just3(double a = 0.1, double b = 0.2, int count = 100) => Series3(5, -3, a, b, count, true);

        public static Shape Vase(double a = 0.1, double b = 0.2, int count = 100) => Series3(4, -5, a, b, count, true);

        public static Shape Man(double a = 0.09, double b = 0.25, int count = 200) => Series3(-10, 4, a, b, count, true);

        public static Shape Sun(double a = 0.1, double b = 0.2, int count = 200) => Series3(-12, 10, a, b, count, true);

        public static Shape Fire(double a = 0.15, double b = 0.22, int k = 0, int count = 200, bool fill = true) => Series3(-(8+k), 7+k, a, b, count, fill);
        public static Shape Crown(double a = 0.1, double b = 0.2, int count = 200) => Series3(-5, 4, a, b, count, true);

        public static Shape SeriesFormula(Fr[] fShape)
        {
            var n = 70;
            var bold = 6;
            var font = "Libertinus Math";

            var e = vectorizer.GetText("e", n, font, 1 , 1, false).Mult(1d/n).ToLines(bold);
            var pref = vectorizer.GetText("f(t) =", n, font, 1, 1, false).Mult(1d / n).Move(0, -0.1, 0).ToLines(bold);
            var interval = vectorizer.GetText(", t ∈ [0, 2π]", n, font, 1, 1, false).Mult(1d / n).Move(0, -0.1, 0).ToLines(bold);

            string FormatV(double x, string tail = "", bool plus = false)
            {
                var sx = x.Abs().ToString(CultureInfo.InvariantCulture);

                if (x.Abs() == 1)
                    sx = "";

                if (plus && x > 0)
                    sx = "+ " + sx;

                if (x <= 0)
                    sx = "- " + sx;

                if (tail.HasText())
                    sx = sx.HasText() ? $"{sx} {tail}" : tail;
                
                return sx;
            }

            var koffs = fShape.Perfecto()
                .SelectWithIndex((k, ind) =>
                vectorizer.GetText($"{FormatV(k.r, "", ind > 0)}", n, font, 1, 1, false).Mult(0.7d / n).AlignX(1).Move(-0.1, 0.1, 0).ToLines(bold) + 
                e + 
                vectorizer.GetText($"{FormatV(k.n + k.dn, "it")}", n, font, 1, 1, false).Mult(0.5 / n).Move(1, 0.6, 0).ToLines(bold))
                .Select(s=>s.AlignX(0));

            var f = new[] {pref, koffs.CompoundOx(0.3)}.CompoundOx(0.5);
            var txt = new[] { f, interval}.CompoundOx(0);

            return txt.Mult(0.1);
        }
    }
}