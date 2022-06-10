using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vector2 = Model.Vector2;

namespace Model3D.Extensions
{
    public static class VectorNExtensions
    {
        private static readonly Vector3 Zero3 = new Vector3(0, 0, 0);
        private static readonly Vector4 Zero4 = new Vector4(0, 0, 0, 0);

        public static Vector3 ToV3(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector2 ToV2(this Vector4 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector4 ToV4(this Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1);
        }

        public static Vector2 ToV2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }

        public static Vector3 MultV(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public static Vector3 MultC(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static double MultS(this Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector4 ToV4(this Vector2 v)
        {
            return new Vector4(v.x, v.y, 0, 1);
        }

        public static Vector3 ToV3(this Vector2 v, double z = 0)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3D ToV3D(this Vector3 v)
        {
            return new Vector3D(v.x, v.y, v.z);
        }

        public static Vector3 ToV3(this Vector3D v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3 ToLen(this Vector3 v, double len)
        {
            return v * len / v.Length;
        }

        public static Vector3 ToLen(this Vector3 v, Func<double, double> lenFn)
        {
            return v * lenFn(v.Length) / v.Length;
        }

        public static Vector3 ToXY(this Vector3 v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        public static Vector3 Center(this IEnumerable<Vector3> vectors)
        {
            var sum = Zero3;
            var count = 0;
            foreach (var v in vectors)
            {
                sum += v;
                count++;
            }

            return sum / count;
        }

        public static Vector4 Center(this IEnumerable<Vector4> vectors)
        {
            var sum = Zero4;
            var count = 0;
            foreach (var v in vectors)
            {
                sum += v;
                count++;
            }

            return sum * (1.0 / count);
        }

        public static Vector3 Sum(this IEnumerable<Vector3> vectors)
        {
            return vectors.Aggregate(Zero3, (a, b) => a + b);
        }

        public static Vector3[] Centered(this Vector3[] vectors)
        {
            var center = vectors.Center();
            return vectors.Select(v => v - center).ToArray();
        }

        public static Shape ToShape(this IEnumerable<Vector3> vectors)
        {
            return new Shape
            {
                Points3 = vectors.ToArray(),
                Convexes = vectors.Index().SelectCirclePair((i, j) => new[] {i,j}).ToArray()
            };
        }

        public static Vector3 Scale(this Vector3 a, Vector3 aSize, Vector3 bSize)
        {
            return new Vector3
            {
                x = a.x * bSize.x / aSize.x,
                y = a.y * bSize.y / aSize.y,
                z = a.z * bSize.z / aSize.z
            };
        }

        public static Vector3 Rotate(this Vector3 v, double x, double y, double z) => v.Rotate(new Vector3(x, y, z));

        public static Vector3 Rotate(this Vector3 v, Vector3 r)
        {
            var q = Quaternion.FromRotation(Vector3.ZAxis, r.Normalize());

            return q * v;
        }

        public static Shape ToShape(this IEnumerable<Vector2> points, double? volume = null, bool triangulate = false) => points.ToArray().ToPolygon().ToShape(volume, triangulate);
    }
}
