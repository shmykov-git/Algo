using System;
using Model.Extensions;
using Model3D.Extensions;

namespace Model.Libraries
{
    public static class FourierShapes
    {
        public static Shape Square(double a = 0.11, int count = 100) =>
            Polygons.FourierSeries(count, (a, 3), (1, -1))
                .Fill().TurnOut().ToShape3()
                .Rotate(Math.PI / 4).Perfecto();

        public static Shape Star(double a = 0.15, int count = 100) =>
            Polygons.FourierSeries(count, (a, 4), (1, -1))
                .Fill().TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();

        public static Shape Polygon(int n, double a = 0.1, int count = 100) =>
            Polygons.FourierSeries(count, (6*a/(n-1), n-1), (1, -1))
                .Fill().TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();

        public static Shape Just1(double a = 0.1, double b = 0.2) => Polygons
            .FourierSeries(100, (a, 4), (b, -3), (1, -1)).Fill().TurnOut()
            .ToShape3().Rotate(Math.PI / 2).Perfecto();

        public static Shape Just2(double a = 0.1, double b = 0.2) => Polygons.FourierSeries(100, (a, 2), (b, -3), (1, -1)).Fill().TurnOut()
            .ToShape3().Rotate(Math.PI / 2).Perfecto();
    }
}