using Model.Libraries;
using Model.Tools;
using System;
using System.Linq;

namespace Model.Extensions
{
    public static class Shape2Extensions
    {
        public static Shape2 Normalize(this Shape2 shape)
        {
            var points = shape.Points.Distinct().ToList();
            var convexes = shape.Convexes.Select(convex => convex.Select(i => points.IndexOf(shape.Points[i])).ToArray()).ToArray();

            return new Shape2
            {
                Points = points.ToArray(),
                Convexes = convexes
            };
        }

        public static Shape2 Transform(this Shape2 shape, Func<Vector2, Vector2> transformFn)
        {
            return new Shape2
            {
                Points = shape.Points.Select(transformFn).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape2 Reverse(this Shape2 shape)
        {
            var points = shape.Points.Reverse().ToArray();
            var convexes = shape.Convexes.Transform(i => points.Length - 1 - i);

            return new Shape2
            {
                Points = points,
                Convexes = convexes
            };
        }

        public static Shape2 Move(this Shape2 shape, Size size)
        {
            return shape.Transform(p => p + size);
        }

        public static Shape2 Move(this Shape2 shape, Vector2 move)
        {
            return shape.Transform(p => p + move);
        }

        public static Shape2 Scale(this Shape2 shape, Size bSize)
        {
            return shape.Scale(Size.One, bSize);
        }

        public static Shape2 ScaleToOne(this Shape2 shape, Size aSize)
        {
            return shape.Scale(aSize, Size.One);
        }

        public static Shape2 Scale(this Shape2 shape, Size aSize, Size bSize)
        {
            return shape.Transform(p => p.Scale(aSize, bSize));
        }

        public static Shape2 Mult(this Shape2 shape, double k)
        {
            return shape.Transform(p => p * k);
        }

        public static Shape2 MirrorY(this Shape2 shape, Size s)
        {
            return shape.Transform(p => (p.X, s.Height - p.Y));
        }

        public static Shape2 Rotate(this Shape2 shape, double angle)
        {
            var m = Rotates2.Rotate(angle);

            return shape.Transform(p => m * p);
        }

        public static Shape2 Rotate(this Shape2 shape, Vector2 center, double angle)
        {
            var m = Rotates2.Rotate(angle);

            return shape.Transform(p => center + m * (p - center));
        }

        public static Shape2 Mirror(this Shape2 shape, Line2 line)
        {
            var n = line.Normal;

            return shape.Transform(p => p - 2 * line.Fn(p) * n / line.Normal.Len2).Reverse();
        }

        public static Shape2 Mirror(this Shape2 shape, Vector2 center)
        {
            return shape.Transform(p => p - 2 * (p - center));
        }

        public static Shape2 Join(this Shape2 shape, Shape2 another)
        {
            return new Shape2
            {
                Points = shape.Points.Concat(another.Points).ToArray(),
                Convexes = shape.Convexes.Concat(another.Convexes.Transform(i => i + shape.Points.Length)).ToArray()
            };
        }

        public static Shape2 TriangulateConvexes(this Shape2 shape)
        {
            return new Shape2
            {
                Points = shape.Points,
                Convexes = FillEngine.Triangulate(shape.Points, shape.Convexes)
            };            
        }

        public static SuperShape2 ToSuperShape(this Shape2 shape)
        {
            return new SuperShape2(shape);
        }

        public static Shape2 ToShape(this SuperShape2 superShape)
        {
            return new Shape2
            {
                Points = superShape.points,
                Convexes = superShape.convexes.Select(convex => convex.indices.ToArray()).ToArray()
            };
        }
    }
}
