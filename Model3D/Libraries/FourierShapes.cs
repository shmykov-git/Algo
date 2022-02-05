using System;
using System.Drawing;
using System.Linq;
using Meta;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Tools;

namespace Model.Libraries
{
    public static class FourierShapes
    {
        private static Vectorizer vectorizer = DI.Get<Vectorizer>();

        public static Shape Square(double a = 0.11, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (a, 3), (1, -1))
                .Condition(fill, p => p.Fill()).TurnOut().ToShape3()
                .Rotate(Math.PI / 4).Perfecto();

        public static Shape Star(double a = 0.15, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (a, 4), (1, -1))
                .Condition(fill, p => p.Fill()).TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();

        public static Shape Polygon(int n, double a = 0.1, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (6*a/(n-1), n-1), (1, -1))
                .Condition(fill, p => p.Fill()).TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();

        public static Shape Series3(int an, int bn, double a,double b, int count, bool fill) => Polygons
            .FourierSeries(count, (a, an), (b, bn), (1, -1))
            .Condition(fill, p => p.Fill())
            .TurnOut().ToShape3().Rotate(Math.PI / 2).Perfecto();

        public static Shape SearchSeries3(int fromI, int toI, int fromJ, int toJ, double a, double b)
        {
            return (
                (toI - fromI + 1, toJ - fromJ + 1).SelectRange((i, j) => (i: i + fromI, j: j + fromJ))
                .Select(v =>
                    FourierShapes.Series3(v.i, v.j, a, b, 100, false).Mult(0.8).Move(v.j, v.i, 0)
                        .ToLines3(2, Color.Blue)
                ).ToSingleShape() +
                (toI - fromI + 1).SelectRange(i =>
                    vectorizer.GetText($"{i + fromI}", 50, "Arial", 1, 1, false).Centered().Mult(0.01).Move(fromJ - 2, i+fromI, 0).ToLines3(3, Color.Red)).ToSingleShape() +
                (toJ - fromJ + 1).SelectRange(j =>
                    vectorizer.GetText($"{j + fromJ}", 50, "Arial", 1, 1, false).Centered().Mult(0.01).Move(j+fromJ, toI+2, 0).ToLines3(3, Color.Red)).ToSingleShape()
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
    }
}