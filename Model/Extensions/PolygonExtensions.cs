using Model.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using Model.Libraries;

namespace Model.Extensions
{
    public static class PolygonExtensions
    {
        public static Polygon Join(this Polygon polygon, Polygon nextPolygon) => polygon.PutInside(nextPolygon, true);

        public static Polygon PutInside(this Polygon polygon, Polygon insidePolygon, bool skipReverse = false)
        {
            var points = polygon.Points;
            var insidePoints = skipReverse ? insidePolygon.Points : insidePolygon.Points.Reverse().ToArray();

            var indexes = points.Index();
            var insideIndexes = insidePoints.Index();

            var (minI, minJ) = indexes.SelectMany(i => insideIndexes.Select(j => new
            {
                Pair = (i, j),
                Len2 = (points[i] - insidePoints[j]).Len2
            })).OrderBy(v => v.Len2).First().Pair;

            if (points[minI] == insidePoints[minJ])
            {
                return new Polygon
                {
                    Points = points.Take(minI)
                        .Concat(insidePoints.Skip(minJ))
                        .Concat(insidePoints.Take(minJ))
                        .Concat(points.Skip(minI))
                        .ToArray()
                };
            }
            else
            {
                return new Polygon
                {
                    Points = points.Take(minI + 1)
                        .Concat(insidePoints.Skip(minJ))
                        .Concat(insidePoints.Take(minJ + 1))
                        .Concat(points.Skip(minI))
                        .ToArray()
                };
            }
        }

        public static Polygon Transform(this Polygon polygon, Func<Vector2, Vector2> transformFn)
        {
            return new Polygon
            {
                Points = polygon.Points.Select(transformFn).ToArray()
            };
        }

        public static Polygon MoveX(this Polygon polygon, double x) => polygon.Move((x, 0));
        public static Polygon MoveY(this Polygon polygon, double y) => polygon.Move((0, y));

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

        public static Polygon Randomize(this Polygon polygon, double shift = 0.00001, int seed = 0)
        {
            var rnd = new Random(seed);
            var delta = shift * polygon.MinLinesLen;

            var points = polygon.Points.Select(p => p + (delta * rnd.NextDouble(), delta * rnd.NextDouble())).ToArray();

            return new Polygon() {Points = points};
        }

        public static Polygon SmoothOut(this Polygon polygon, int repeatCount = 1, double angleFactor = -1, Func<Vector2, bool> conditionFn = null)
        {
            Vector2 Smooth(Vector2 a, Vector2 b, Vector2 c)
            {
                if (conditionFn != null && !conditionFn(b))
                    return b;

                var scalar = (c - b).Normed * (b - a).Normed;

                return scalar >= angleFactor ? (a + b + c) / 3 : b;
            }

            IEnumerable<Vector2> points = polygon.Points;
            (repeatCount).ForEach(_ => points = points.SelectCircleTriple(Smooth));

            return new Polygon()
            {
                Points = points.ToArray()
            };
        }

        public static Polygon SmoothOutFar(this Polygon polygon, int repeatCount = 1, int groupCount = 3, double angleFactor = -1, Func<Vector2, bool> conditionFn = null)
        {
            var gc2 = groupCount / 2;

            Vector2 Smooth(Vector2[] ps)
            {
                var a = ps[0];
                var b = ps[gc2];
                var c = ps[^1];

                if (conditionFn != null && !conditionFn(b))
                    return b;

                var scalar = (c - b).Normed * (b - a).Normed;

                return scalar >= angleFactor ? ps.Center() : b;
            }

            IEnumerable<Vector2> points = polygon.Points;
            (repeatCount).ForEach(_ => points = points.SelectCircleGroup(groupCount, Smooth));

            return new Polygon()
            {
                Points = points.ToArray()
            };
        }

        public static Func<Vector2, double> DistanceFn(this Polygon polygon, double? step = null, double? smothDistance = null)
        {
            var netStep = step ?? polygon.Points.SelectCirclePair((a, b) => (b - a).Len).Min();
            var net = new Net<Vector2, int>(polygon.Points.Select((p, i) => (p, i)), netStep);
            var lines = polygon.Points
                .SelectCirclePair((a, b) => new Line2(a, b))
                .SelectCirclePair((l1, l2) => (l1, l2))
                .ToArray()
                .CircleShift(-1);

            double Fn(Vector2 x) => net.SelectNeighbors(x)
                .Select(i => Math.Min(lines[i].l1.SegmentDistance(x), lines[i].l2.SegmentDistance(x)))
                .Min();

            var dirs = new Vector2[] {(1, 0), (-1, 0), (0, 1), (0, -1)};

            double SmoothFn(Vector2 x)
            {
                return dirs.Select(d => Fn(x + d * smothDistance.Value)).Average();
            }

            return smothDistance.HasValue ? SmoothFn : Fn;
        }
    }
}
