﻿using Model;
using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vector2 = Model.Vector2;
using Vector3 = Model3D.Vector3;
using Model.Libraries;
using Model.Hashes;

namespace Model3D.Extensions
{

    public static class VectorNExtensions
    {
        private static readonly Vector3 Zero3 = Vector3.Origin;
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

        public static (float x, float y, float z) ToFloat(this Vector3 v)
        {
            return ((float)v.x, (float)v.y, (float)v.z);
        }

        public static Vector3 SetX(this Vector3 v, double a)
        {
            return new Vector3(a, v.y, v.z);
        }

        public static Vector3 SetY(this Vector3 v, double a)
        {
            return new Vector3(v.x, a, v.z);
        }

        public static Vector3 SetZ(this Vector3 v, double a)
        {
            return new Vector3(v.x, v.y, a);
        }

        public static Vector3 ZeroX(this Vector3 v)
        {
            return new Vector3(0, v.y, v.z);
        }

        public static Vector3 ZeroY(this Vector3 v)
        {
            return new Vector3(v.x, 0, v.z);
        }

        public static Vector3 ZeroZ(this Vector3 v)
        {
            return new Vector3(v.x, v.y, 0);
        }

        public static Vector3 VectorX(this Vector3 v)
        {
            return new Vector3(v.x, 0, 0);
        }

        public static Vector3 VectorY(this Vector3 v)
        {
            return new Vector3(0, v.y, 0);
        }

        public static Vector3 VectorZ(this Vector3 v)
        {
            return new Vector3(0, 0, v.z);
        }

        public static Vector3 MultV(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        }

        public static Vector3 MultC(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 DivC(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        public static double MultS(this Vector3 a, Vector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static Vector3 Proj(this Vector3 a, Vector3 b)
        {
            return (a.MultS(b) / a.Length2) * a;
        }

        public static (bool hasValue, Vector3 value) ProjWithCheck(this Vector3 a, Vector3 b, double epsilon = Values.Epsilon9)
        {
            var ln2 = a.Length2;

            if (ln2 < epsilon * epsilon)
                return (false, Vector3.Origin);

            return (true, (a.MultS(b) / ln2) * a);
        }

        public static double MinXyz(this Vector3 a)
        {
            return Math.Min(a.x, Math.Min(a.y, a.z));
        }

        public static double MaxXyz(this Vector3 a)
        {
            return Math.Max(a.x, Math.Max(a.y, a.z));
        }

        public static bool EqualsV(this Vector3 a, Vector3 b, double epsilon = Values.Epsilon9)
        {
            return (a.x - b.x).Abs() < epsilon && (a.y - b.y).Abs() < epsilon && (a.z - b.z).Abs() < epsilon;
        }

        public static Vector4 ToV4(this Vector2 v)
        {
            return new Vector4(v.x, v.y, 0, 1);
        }

        public static Vector3 ToV3(this Vector2 v, double z = 0)
        {
            return new Vector3(v.x, v.y, z);
        }

        public static Vector3D ToVc3D(this Vector3 v)
        {
            return new Vector3D(v.x, v.y, v.z);
        }

        public static Vector3 ToV3(this Vector3D v)
        {
            return new Vector3(v.x, v.y, v.z);
        }

        public static Vector3 ToLen(this Vector3 v, double len)
        {
            return v * (len / v.Length);
        }

        public static Vector3 ToLenWithCheck(this Vector3 v, double len, double epsilon = Values.Epsilon9, bool debugCheck = false)
        {
            var len0 = v.Length;
            var check = len0.Abs() < epsilon;

            if (check)
            {
                if (debugCheck)
                    Debug.WriteLine("ToLenWithCheck zero detected");

                return Vector3.Origin;
            }

            return v * (len / len0);
        }

        public static Vector3 ToLen(this Vector3 v, Func<double, double> lenFn)
        {
            return v * (lenFn(v.Length) / v.Length);
        }

        public static Vector3 ToLenWithCheck(this Vector3 v, Func<double, double> lenFn, double epsilon = Values.Epsilon9)
        {
            var ln = v.Length;

            return ln < epsilon ? new Vector3(0, 0, 0) : v * (lenFn(ln) / ln);
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

        public static Shape ToPointsShape(this IEnumerable<Vector3> vectors)
        {
            return new Shape
            {
                Points3 = vectors.ToArray(),
            };
        }

        public static Shape ToShape(this IEnumerable<Vector3> vectors, bool isClosed = true)
        {
            return new Shape
            {
                Points3 = vectors.ToArray(),
                Convexes = isClosed 
                    ? vectors.Index().SelectCirclePair((i, j) => new[] {i,j}).ToArray()
                    : vectors.Index().SelectPair((i, j) => new[] { i, j }).ToArray()
            };
        }

        public static Vector3 Boxed(this Vector3 a, Vector3 scale, Vector3 center) => a.Scale(scale) + center;

        public static Vector3 Scale(this Vector3 a, Vector3 aSize, Vector3 bSize)
        {
            return new Vector3
            {
                x = a.x * bSize.x / aSize.x,
                y = a.y * bSize.y / aSize.y,
                z = a.z * bSize.z / aSize.z
            };
        }

        public static Vector3 Scale(this Vector3 a, Vector3 aSize)
        {
            return new Vector3
            {
                x = a.x * aSize.x,
                y = a.y * aSize.y,
                z = a.z * aSize.z
            };
        }

        public static Vector3 Rotate(this Vector3 v, double x, double y, double z) => v.Rotate(new Vector3(x, y, z));

        public static Vector3 Rotate(this Vector3 v, Vector3 r)
        {
            var q = Quaternion.FromRotation(Vector3.ZAxis, r.Normalize());

            return q * v;
        }

        public static Vector3 RotateOz(this Vector3 v, double alfa)
        {
            var q = Quaternion.FromAngleAxis(alfa, Vector3.ZAxis);

            return q * v;
        }

        public static Vector3 ToV3(this Vector v)
        {
            return new Vector3(v[0], v[1], v[2]);
        }

        public static Shape ToShape(this Vector2[] points, double? volume = null, bool triangulate = false) => points.ToPolygon().ToShape(volume, triangulate);

        public static bool IsInside(this Vector3[] vs, Vector3 x)
        {
            return vs.SelectCircleTriple((a, b, c) => (b - a).MultV(c - b).MultS((b - a).MultV(x - b)) < 0 ? -1 : 1)
                .Sum().Abs() == vs.Length;
        }

        public static Vector3 GetPlaneNormal(this Vector3 center, Vector3 a, Vector3 b) => (a - center).MultV(b - center);
        public static double GetVolume0(this Vector3 c, Vector3 a, Vector3 b) => c.MultS(c.GetPlaneNormal(a, b));

        public static Hashed<Vector3> ToHashed(this Vector3 v, double equalityRadius = Values.Epsilon9) => new Hashed<Vector3>(v, a => a.GetHashCode(), (a, b) => (b - a).Length2 < equalityRadius * equalityRadius);    
    }
}
