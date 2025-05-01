using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using System;
using System.Linq;

namespace Model3D.Extensions
{
    public static class Polygon3Extensions
    {
        private static readonly Vector3 One3 = new Vector3(1, 1, 1);
        private static readonly Vector3 Zero3 = new Vector3(0, 0, 0);

        public static Polygon3 Transform(this Polygon3 polygon, Func<Vector3, Vector3> transformFn)
        {
            return new Polygon3
            {
                Points = polygon.Points.Select(transformFn).ToArray()
            };
        }

        public static Polygon3 Move(this Polygon3 polygon, Vector3 size)
        {
            return polygon.Transform(p => p + size);
        }

        public static Polygon3 Scale(this Polygon3 polygon, Vector3 bSize)
        {
            return polygon.Scale(One3, bSize);
        }

        public static Polygon3 ScaleToOne(this Polygon3 polygon, Vector3 aSize)
        {
            return polygon.Scale(aSize, One3);
        }

        public static Polygon3 Scale(this Polygon3 polygon, Vector3 aSize, Vector3 bSize)
        {
            return polygon.Transform(p => p.Scale(aSize, bSize));
        }

        public static Polygon3 Mult(this Polygon3 polygon, double k)
        {
            return polygon.Transform(p => p * k);
        }

        public static Shape ToShape(this Polygon3 polygon)
        {
            return new Shape
            {
                Points3 = polygon.Points,
                Convexes = new int[][] { polygon.Points.Index().ToArray() }
            };
        }
    }
}
