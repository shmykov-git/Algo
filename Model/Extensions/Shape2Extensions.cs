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

        public static Shape2 Move(this Shape2 shape, Size size)
        {
            return shape.Transform(p => p + size);
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
    }
}
