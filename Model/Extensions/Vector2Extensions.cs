using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Model.Extensions
{
    public static class Vector2Extensions
    {
        public static Vector2 Scale(this Vector2 a, Vector2 aSize, Vector2 bSize)
        {
            return new Vector2
            {
                x = a.x * bSize.x / aSize.x,
                y = a.y * bSize.y / aSize.y
            };
        }

        public static Vector2 Center(this IEnumerable<Vector2> vectors)
        {
            var sum = Vector2.Zero;
            var count = 0;
            foreach(var v in vectors)
            {
                sum += v;
                count++;
            }
            
            return sum / count;
        }

        public static Vector2 SizeCenter(this IEnumerable<Vector2> vectors)
        {
            var points = vectors.ToArray();

            var minX = points.Length == 0 ? 0 : points.Min(p => p.x);
            var maxX = points.Length == 0 ? 0 : points.Max(p => p.x);
            var minY = points.Length == 0 ? 0 : points.Min(p => p.y);
            var maxY = points.Length == 0 ? 0 : points.Max(p => p.y);

            return (0.5 * (minX + maxX), 0.5 * (minY + maxY));
        }

        public static Vector2 Sum(this IEnumerable<Vector2> vectors)
        {
            return vectors.Aggregate(Vector2.Zero, (a, b) => a + b);
        }

        public static Vector2[] Centered(this Vector2[] vectors)
        {
            var center = vectors.Center();
            return vectors.Select(v => v - center).ToArray();
        }

        public static (double x, double y) ToValueTuple(this Vector2 a)
        {
            return (a.x, a.y);
        }

        public static Vector2 ToV2(this Vector v)
        {
            return new Vector2(v[0], v[1]);
        }

        public static bool IsLeft(this Vector2 x, Vector2 a, Vector2 b, bool isOuter = true, double epsilon = double.Epsilon)
        {
            return (b - a).Normal * (x - a) < (isOuter ? -epsilon : epsilon);
        }

        public static bool IsInside(this Vector2 x, Vector2 a, Vector2 b, Vector2 c)
        {
            return x.IsLeft(a, b, false) && x.IsLeft(b, c, false) && x.IsLeft(c, a, true);
        }
    }
}
