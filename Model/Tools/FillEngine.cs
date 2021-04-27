using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Tools
{
    public class FillEngine
    {
        public Action<int[]> OnDebug;

        public (bool, int[][], Trio[]) FillPoligonByTriangles(Poligon poligon)
        {
            var maxCircles = 10 * poligon.Points.Length;

            var vertices = poligon.Points.Index().ToList();
            List<List<int>> convexes = new List<List<int>>();

            Trio ToTriangleTrio(Trio trio) => new Trio(vertices[trio.I], vertices[trio.J], vertices[trio.K]);

            int CorrectInd(int i) => (i + vertices.Count) % vertices.Count;
            int CorrectDelInd(int i, int delI) => i < delI ? i : i - 1;
            int NextInd(int i, int count = 1) => CorrectInd(i + count);
            int PrevInd(int i, int count = 1) => CorrectInd(i - count);
            Trio NextTrio(Trio trio) => new Trio(trio.J, trio.K, NextInd(trio.K));

            TrioPairInfo ToPairInfo(Trio trio) => new TrioPairInfo
            {
                Line = new Line2(poligon[trio.I], poligon[trio.J]),
                Point = poligon[trio.K]
            };

            Vector2 GetPoint(int i) => poligon[vertices[i]];
            TrioPairInfo GetTrioPairInfo(Trio trio) => ToPairInfo(ToTriangleTrio(trio));
            bool IsLeftTrio(Trio trio) => GetTrioPairInfo(trio).IsLeftPoint;

            var t = new Trio(0, 1, 2);
            var n = 0;
            while (vertices.Count > 2 && n++ < maxCircles)
            {
                var validateCount = 0;
                while(!IsLeftTrio(t) && validateCount++ < vertices.Count)
                    t = NextTrio(t);

                if (validateCount == vertices.Count)
                {
                    n = maxCircles;
                    break;
                }

                var convexStartOutInd = PrevInd(t.I);

                var convexEndInfo = GetTrioPairInfo(t);
                var convexCount = 0;
                while (convexCount++ < vertices.Count)
                {
                    var tInfo = GetTrioPairInfo(t);
                    if (!tInfo.IsLeftPoint)
                        break;

                    if (!convexEndInfo.CheckLeftPoint(GetPoint(t.K)))
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

                var l = PrevInd(t.I);
                var pointCount = 2;
                while (l != convexStartOutInd && pairInfo.CheckLeftPoint(GetPoint(l)))
                {
                    l = PrevInd(l);
                    pointCount++;
                }

                for (var i = 1; i <= pointCount; i++)
                    convex.Add(vertices[NextInd(l, i)]);

                var convexLines = convex.SelectCirclePair((j, k) => new Line2(poligon[j], poligon[k])).ToArray();
                var hasInsidePoint = vertices.Except(convex).Select(i => poligon[i]).Any(p => convexLines.All(l => l.IsLeft(p)));

                if (hasInsidePoint)
                    continue;

                //if (convexes.Count == 5)
                //    Debugger.Break();

                for (var i = 2; i < pointCount; i++)
                {
                    var delI = NextInd(l, 2);
                    vertices.RemoveAt(delI);
                    l = CorrectDelInd(l, delI);
                }

                convexes.Add(convex);
                if (OnDebug != null)
                    OnDebug(convex.ToArray());

                t = new Trio(PrevInd(l, 2), PrevInd(l), l);
                //var debugT = ToTriangleTrio(t);
            }

            List<Trio> GetConvexTrios(List<int> convex)
            {
                int CorrectInd(int i) => (i + convex.Count) % convex.Count;

                var halfLen = convex.Count / 2;

                var shift = Enumerable.Range(0, convex.Count / 2)
                    .Select(i => new { I = i, Len2 = (poligon[convex[CorrectInd(i)]] - poligon[convex[CorrectInd(i + halfLen)]]).Len2 })
                    .OrderBy(v => v.Len2)
                    .First()
                    .I;

                return Enumerable.Range(0, convex.Count - 2).Select(i => new Trio(convex[CorrectInd(shift)], convex[CorrectInd(shift + i + 1)], convex[CorrectInd(shift + i + 2)])).ToList();
            }

            return (n < maxCircles, convexes.Select(list=>list.ToArray()).ToArray(), convexes.SelectMany(GetConvexTrios).ToArray());
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
