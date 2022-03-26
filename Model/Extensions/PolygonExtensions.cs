using Model.Tools;
using System;
using System.Linq;
using Model.Libraries;

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

        public static Polygon Move(this Polygon polygon, Vector2 size)
        {
            return polygon.Transform(p => p + size);
        }

        public static Polygon Scale(this Polygon polygon, Vector2 bSize)
        {
            return polygon.Scale((1,1), bSize);
        }

        public static Polygon ScaleToOne(this Polygon polygon, Vector2 aSize)
        {
            return polygon.Scale(aSize, (1,1));
        }

        public static Polygon Scale(this Polygon polygon, Vector2 aSize, Vector2 bSize)
        {
            return polygon.Transform(p => p.Scale(aSize, bSize));
        }

        public static Polygon Mult(this Polygon polygon, double k)
        {
            return polygon.Transform(p => p * k);
        }

        public static Polygon MirrorY(this Polygon polygon, Vector2 s)
        {
            return polygon.Transform(p => (p.x, s.y - p.y));
        }

        public static Polygon Split(this Polygon polygon, double edgeLen)
        {
            var shape = polygon.ToShape2().SplitEdges(edgeLen);
            return new Polygon
            {
                Points = shape.Convexes[0].Index().Select(i => shape[shape.Convexes[0][i]]).ToArray()
            };
        }

        public static Shape2 Fill(this Polygon polygon, bool triangulate = false)
        {
            var convexes = FillEngine.FindConvexes(polygon);

            if (triangulate)
                convexes = FillEngine.Triangulate(polygon.Points, convexes);

            return new Shape2
            {
                Points = polygon.Points,
                Convexes = convexes,
            };
        }

        public static Polygon Reverse(this Polygon polygon)
        {
            return new Polygon
            {
                Points = polygon.Points.Reverse().ToArray(),
            };
        }

        public static Shape2 Condition(this Polygon polygon, bool apply, Func<Polygon, Shape2> fn)
        {
            return apply ? fn(polygon) : polygon.ToShape2();
        }

        public static Shape2 ToShape2(this Polygon polygon, bool fill = false)
        {
            if (fill)
                return polygon.Fill(false);

            return new Shape2
            {
                Points = polygon.Points,
                Convexes = new int[][] { polygon.Points.Index().ToArray() }
            };
        }

        public static Shape2 PaveInside(this Polygon polygon, Shape2 parquete)
        {
            return Paver.Pave(polygon, parquete, true);
        }

        public static Shape2 PaveOutside(this Polygon polygon, Shape2 parquete)
        {
            return Paver.Pave(polygon, parquete, false);
        }

        public static Shape2 PaveExactInside(this Polygon polygon, Shape2 parquete)
        {
            return Paver.PaveExact(polygon, parquete, true);
        }

        public static Shape2 PaveExactOutside(this Polygon polygon, Shape2 parquete)
        {
            return Paver.PaveExact(polygon, parquete, false);
        }

        public static bool IsLeftByAngle(this Polygon polygon, bool isClosed = true)
        {
            var c = polygon.Points.Center();

            var centerDistance = isClosed
                ? polygon.Points.SelectCirclePair((a, b) => Angle.LeftDirection(a,c,b)).Sum()
                : polygon.Points.SelectPair((a, b) => Angle.LeftDirection(a, c, b)).Sum();

            return centerDistance < 0;
        }

        public static bool IsLeft(this Polygon polygon, bool isClosed = true)
        {
            var c = polygon.Points.Center();

            var centerDistance = isClosed
                ? polygon.Points.SelectCirclePair((a, b) => new Line2(a, b).Fn(c)).Sum()
                : polygon.Points.SelectPair((a, b) => new Line2(a, b).Fn(c)).Sum();

            return centerDistance < 0;
        }

        public static Polygon ToLeft(this Polygon polygon)
        {
            return polygon.IsLeft() ? polygon : polygon.Reverse();
        }

        public static Polygon ToRadiusPointsPolygon(this Polygon polygon)
        {
            var sign = 1;
            var ps = polygon.Points
                .SelectCircleTriple((a, b, c) => new { m = (b - a) * (c - b), p = b })
                .SelectCirclePair((a, b) =>
                {
                    var s = (int)(a.m - b.m).Sign();

                    if (s == 0)
                        s = 1;

                    var res = s == 1 && sign == -1 ? a.p : Vector2.Zero;

                    sign = s;

                    return res;
                })
                .Where(v => v != Vector2.Zero)
                .ToArray();

            return new Polygon() {Points = ps};
        }

        public static Polygon ToGradientPointsPolygon(this Polygon polygon)
        {
            var ps = polygon.Points
                .SelectCircleTriple((a, b, c) =>
                {
                    var p = new Line2(a, c).ProjectionPoint(b);

                    if (new Line2(a, b).IsLeft(c))
                        return p;
                    else
                        return 2 * b - p;
                })
                .ToArray();

            return new Polygon() { Points = ps };
        }

        public static Polygon ToPolygon(this Vector2[] points) => new Polygon() { Points = points };
    }
}
