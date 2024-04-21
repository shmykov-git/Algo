using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Model.Extensions
{
    public static class Vector2Extensions
    {
        public static Polygon ToPolygon(this IEnumerable<Vector2> points) => new() { Points = points.ToArray() };
        public static Shape2 ToShape2(this IEnumerable<Vector2> points, bool closed = false)
        {
            var ps = points.ToArray();

            return new()
            {
                Points = ps,
                Convexes = closed
                    ? ps.Index().SelectCirclePair((i, j) => new[] {i, j}).ToArray()
                    : ps.Index().SelectPair((i, j) => new[] {i, j}).ToArray()
            };
        }

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

        public static Vector2 Rotate(this Vector2 x, double alfa, Vector2? center = null)
        {
            var c = center ?? Vector2.Zero;
            Vector2[] mRotate = [(Math.Cos(alfa), -Math.Sin(alfa)), (Math.Sin(alfa), Math.Cos(alfa))]; // rows

            return (mRotate[0] * (x - c), mRotate[1] * (x - c)) + c;
        }

        public static double Angle(this Vector2 a, Vector2 b) => Math.Acos(a.Normed * b.Normed);

        /// <summary>
        /// [-2, 2]. >0 - goes left, <0 - goes right, =0 - goes stright, -2 or 2 - goes back
        /// </summary>
        public static double ScalarAngle(this Vector2 a, Vector2 b)
        {
            var bOrt = b.Normal;
            var scalar = a.Normed * bOrt.Normed;
            var isAcute = a * b > 0;

            return isAcute ? scalar : 2 * scalar.Sgn() - scalar;
        }

        // check ScalarAngle and FullAngle
        //return new[]
        //{
        //    (1000).SelectInterval(-2*Math.PI, 2*Math.PI, fi => new Vector2(fi, Vector2.OneX.ScalarAngle(new Vector2(Math.Cos(fi), Math.Sin(fi))))).ToShape2().ToShape3().ToPoints(0.5).ApplyColor(Color.Red),
        //    (1000).SelectInterval(-2*Math.PI, 2*Math.PI, fi => new Vector2(fi, Vector2.OneX.FullAngle(new Vector2(Math.Cos(fi), Math.Sin(fi))))).ToShape2().ToShape3().ToPoints(0.5).ApplyColor(Color.Blue),
        //    Shapes.Coods2WithText(4)
        //}.ToSingleShape().ToMotion();

        public static double FullAngle(this Vector2 a, Vector2 b)
        {
            var bOrt = b.Normal;
            var angle = Math.PI / 2 - Math.Acos(a.Normed * bOrt.Normed);
            var isAcute = a * b > 0;
            
            angle.BreakNaN();
            
            return isAcute ? angle : angle.Sgn() * Math.PI - angle;
        }
    }
}
