using System;
using Model.Extensions;
using Model3D.Extensions;

namespace Model.Libraries
{
    public static class FourierShapes
    {
        public static Shape Square(double a = 0.11, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (a, 3), (1, -1))
                .Fill(fill).TurnOut().ToShape3()
                .Rotate(Math.PI / 4).Perfecto();
        public static Shape Star(double a = 0.15, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (a, 4), (1, -1))
                .Fill(fill).TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();
        public static Shape Polygon(int n, double a = 0.1, int count = 100, bool fill = true) =>
            Polygons.FourierSeries(count, (6*a/(n-1), n-1), (1, -1))
                .Fill(fill).TurnOut().ToShape3()
                .Rotate(Math.PI / 2).Perfecto();
    }
}