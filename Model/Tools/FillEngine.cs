using Model.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model.Tools
{
    public static class FillEngine
    {
        public static int[][] Triangulate(Vector2[] points, int[][] convexes)
        {
            int[][] TriangulateConvex(int[] convex)
            {
                int CorrectInd(int i) => (i + convex.Length) % convex.Length;

                var halfLen = convex.Length / 2;

                var shift = Enumerable.Range(0, convex.Length / 2)
                    .Select(i => new { I = i, Len2 = (points[convex[CorrectInd(i)]] - points[convex[CorrectInd(i + halfLen)]]).Len2 })
                    .OrderBy(v => v.Len2)
                    .First()
                    .I;

                return Enumerable.Range(0, convex.Length - 2)
                    .Select(i => new[] { convex[CorrectInd(shift)], convex[CorrectInd(shift + i + 1)], convex[CorrectInd(shift + i + 2)] })
                    .ToArray();
            }

            return convexes.SelectMany(convex => TriangulateConvex(convex)).ToArray();
        }

        public static int[][] FindConvexes(Polygon polygon)
        {
            var maxCircles = 10 * polygon.Points.Length;

            var vertices = polygon.Points.Index().ToList();
            List<List<int>> convexes = new List<List<int>>();

            Trio ToTriangleTrio(Trio trio) => new Trio(vertices[trio.i], vertices[trio.j], vertices[trio.k]);

            int CorrectInd(int i) => (i + vertices.Count) % vertices.Count;
            int CorrectDelInd(int i, int delI) => i < delI ? i : CorrectInd(i - 1);
            int NextInd(int i, int count = 1) => CorrectInd(i + count);
            int PrevInd(int i, int count = 1) => CorrectInd(i - count);
            Trio NextTrio(Trio trio) => new Trio(trio.j, trio.k, NextInd(trio.k));

            TrioPairInfo ToPairInfo(Trio trio) => new TrioPairInfo
            {
                Line = new Line2(polygon[trio.i], polygon[trio.j]),
                Point = polygon[trio.k]
            };

            Vector2 GetPoint(int i) => polygon[vertices[i]];
            TrioPairInfo GetTrioPairInfo(Trio trio) => ToPairInfo(ToTriangleTrio(trio));
            bool IsLeftTrio(Trio trio) => GetTrioPairInfo(trio).IsLeftPoint;

            var t = new Trio(0, 1, 2);
            var n = 0;
            while (vertices.Count > 2 && n++ < maxCircles)
            {
                var validateCount = 0;
                while (!IsLeftTrio(t) && validateCount++ < vertices.Count)
                    t = NextTrio(t);

                if (validateCount == vertices.Count)
                {
                    n = maxCircles;
                    break;
                }

                var convexStartOutInd = PrevInd(t.i);

                var convexEndInfo = GetTrioPairInfo(t);
                var convexCount = 0;
                while (convexCount++ < vertices.Count)
                {
                    var tInfo = GetTrioPairInfo(t);
                    if (!tInfo.IsLeftPoint)
                        break;

                    if (!convexEndInfo.CheckLeftPoint(GetPoint(t.k)))
                        break;

                    t = NextTrio(t);
                }

                if (convexCount == vertices.Count)
                {
                    convexes.Add(vertices);
                    break;
                }

                var pairInfo = GetTrioPairInfo(t);
                var convex = new List<int>();

                var l = PrevInd(t.i);
                var pointCount = 2;
                while (l != convexStartOutInd && pairInfo.CheckLeftPoint(GetPoint(l)))
                {
                    l = PrevInd(l);
                    pointCount++;
                }

                for (var i = 1; i <= pointCount; i++)
                    convex.Add(vertices[NextInd(l, i)]);

                var convexLines = convex.SelectCirclePair((j, k) => new Line2(polygon[j], polygon[k])).ToArray();
                var hasInsidePoint = vertices.Except(convex).Select(i => polygon[i]).Any(p => convexLines.All(l => l.IsLeft(p)));

                if (hasInsidePoint)
                    continue;

                for (var i = 2; i < pointCount; i++)
                {
                    var delI = NextInd(l, 2);
                    vertices.RemoveAt(delI);
                    l = CorrectDelInd(l, delI);
                }

                convexes.Add(convex);

                t = new Trio(PrevInd(l, 2), PrevInd(l), l);
            }

            var result = convexes.Select(list => list.ToArray()).ToArray();

            if (n == maxCircles)
                throw new PolygonFillException(result);

            return result;
        }

        struct TrioInfo
        {
            public TrioPairInfo[] Info;

            public bool IsInsidePoint(Vector2 point) => Info.All(v => v.CheckLeftPoint(point));
            public bool IsLeftTriangle => Info.All(v => v.IsLeftPoint);
        };

        struct TrioPairInfo
        {
            public Line2 Line;
            public Vector2 Point;

            public bool CheckLeftPoint(Vector2 point) => Line.Fn(point) < 0;
            public bool IsLeftPoint => CheckLeftPoint(Point);
        }
    }
}
