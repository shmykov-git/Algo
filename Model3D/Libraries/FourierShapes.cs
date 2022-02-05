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
    }
}