using Model.Tools;
using System;
using System.Linq;

namespace Model.Extensions
{
    public static class PolygonExtensions
    {
        public static Polygon PutInside(this Polygon polygon, Polygon insidePolygon)
        {
            var points = polygon.Points;
            var insidePoints = insidePolygon.Points.Reverse().ToArray();

            var indexes = points.Index();
            var insideIndexes = insidePoints.Index();

            var (minI, minJ) = indexes.SelectMany(i => insideIndexes.Select(j => new
            {
                Pair = (i, j),
                Len2 = (points[i] - insidePoints[j]).Len2
            })).OrderBy(v => v.Len2).First().Pair;

            return new Polygon
            {
                Points = points.Take(minI + 1)
                .Concat(insidePoints.Skip(minJ))
                .Concat(insidePoints.Take(minJ + 1))
                .Concat(points.Skip(minI))
                .ToArray()
            };
        }

        public static Polygon Transform(this Polygon polygon, Func<Vector2, Vector2> transformFn)
        {
            return new Polygon
            {
                Points = polygon.Points.Select(transformFn).ToArray()
            };
        }

        public static Polygon Move(this Polygon polygon, Size size)
        {
            return polygon.Transform(p => p + size);
        }

        public static Polygon Scale(this Polygon polygon, Size bSize)
        {
            return polygon.Scale(Size.One, bSize);
        }

        public static Polygon ScaleToOne(this Polygon polygon, Size aSize)
        {
            return polygon.Scale(aSize, Size.One);
        }

        public static Polygon Scale(this Polygon polygon, Size aSize, Size bSize)
        {
            return polygon.Transform(p => p.Scale(aSize, bSize));
        }

        public static Polygon Mult(this Polygon polygon, double k)
        {
            return polygon.Transform(p => p * k);
        }

        public static Polygon MirrorY(this Polygon polygon, Size s)
        {
            return polygon.Transform(p => (p.X, s.Height - p.Y));
        }

        public static Shape2 Fill(this Polygon polygon)
        {
            var convexes = FillEngine.FindConvexes(polygon);
            
            return new Shape2
            {
                Points = polygon.Points,
                Convexes = convexes,
            };
        }

        public static Shape2 ToShape2(this Polygon polygon)
        {
            return new Shape2
            {
                Points = polygon.Points
            };
        }
    }
}
