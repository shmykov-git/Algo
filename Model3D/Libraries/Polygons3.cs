using Model;
using System;

namespace Model3D.Libraries
{
    public static class Polygons3
    {
        public static Polygon3 Spiral(int n, int count) => new Polygon3
        {
            Points = new Func3Info
            {
                Fn = Funcs3.Spiral,
                From = 0,
                To = n * 2 * Math.PI,
                N = count,
                Closed = false
            }.GetPoints()
        };
    }
}
