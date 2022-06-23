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
                Fn = Funcs3.Spiral(),
                From = 0,
                To = n * 2 * Math.PI,
                N = count,
                Closed = false
            }.GetPoints()
        };

        public static Polygon3 Flower(int n, int count) => new Polygon3
        {
            Points = new Func3Info
            {
                Fn = Funcs3.Flower(n),
                From = -Math.PI,
                To = Math.PI,
                N = count,
                Closed = true
            }.GetPoints()
        };

        public static Polygon3 Cloud(int count) => new Polygon3
        {
            Points = new Func3Info
            {
                Fn = Funcs3.Cloud(1,3),
                From = 0,
                To = 8*Math.PI,
                N = count,
                Closed = true
            }.GetPoints()
        };
    }
}
