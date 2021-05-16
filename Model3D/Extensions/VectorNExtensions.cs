using Aspose.ThreeD.Utilities;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model3D.Extensions
{
    public static class VectorNExtensions
    {
        private static readonly Vector3 Zero3 = new Vector3(0, 0, 0);

        public static Vector3 ToV3(this Vector4 v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Model.Vector2 ToV2(this Vector4 v)
        {
            return new Model.Vector2(v.x, v.y);
        }

        public static Vector4 ToV4(this Vector3 v)
        {
            return new Vector4(v.x, v.y, v.z, 1);
        }

        public static Vector4 ToV4(this Model.Vector2 v)
        {
            return new Vector4(v.x, v.y, 0, 1);
        }

        public static Vector3 ToV3(this Model.Vector2 v, double z = 0)
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

        public static Vector3 Sum(this IEnumerable<Vector3> vectors)
        {
            return vectors.Aggregate(Zero3, (a, b) => a + b);
        }

        public static Vector3[] Centered(this Vector3[] vectors)
        {
            var center = vectors.Center();
            return vectors.Select(v => v - center).ToArray();
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
    }
}
