using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Model.Extensions;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Model.Trees;

namespace Model.Tools
{
    public static class Splitter
    {
        public static Shape2 SplitEdges(Shape2 s, double edgeLen)
        {
            var indicesOffset = s.Points.Length;

            ((int i, int j) edge, Vector2[] points, int[] indices, int[] backIndices) GetEdgeData((int i, int j) e)
            {
                var a = s.Points[e.i];
                var b = s.Points[e.j];
                var oneAB = (b - a).Normed;
                var n = (int)((b - a).Len / edgeLen) + 1;
                var len = (b - a).Len / n;
                var abNewPoints = Enumerable.Range(1, n - 1).Select(k => a + k * len * oneAB).ToArray();
                var abAllIndices = new[] { e.i }.Concat(abNewPoints.Index().Select(k => k + indicesOffset)).ToArray();
                var baAllIndices = new[] { e.j }.Concat(abNewPoints.Index().Reverse().Select(k => k + indicesOffset)).ToArray();

                indicesOffset += abNewPoints.Length;

                return (e, abNewPoints, abAllIndices, baAllIndices);
            }

            var edgeDatas = s.OrderedEdges.Select(GetEdgeData).ToArray();
            var edgesConvexes = edgeDatas.SelectMany(v => new[] { (v.edge, v.indices), (v.edge.ReversedEdge(), v.backIndices) }).ToDictionary(v => v.Item1, v => v.Item2);

            return new Shape2
            {
                Points = s.Points.Concat(edgeDatas.SelectMany(e => e.points)).ToArray(),
                Convexes = s.ConvexesIndices.Select(convex => convex.SelectMany(edge => edgesConvexes[edge]).ToArray()).ToArray()
            };
        }

        public static Polygon[] SplitIntersections(Polygon polygon)
        {
            var points = polygon.Points;
            var lines = polygon.Lines.ToArray();
            var net = new Net<Vector2, int>(points.SelectWithIndex((p, i) => (p, i)), 2*lines.Max(l => l.Len));

            int Prev(int i) => (i + points.Length - 1) % points.Length;
            int Next(int i) => (i + 1) % points.Length;

            (int? j, Vector2? p) IntersectedIndex(int i)
            {
                var jj = net
                    .SelectNeighbors(points[i])
                    .Where(j => j != i && j != Prev(i) && j != Next(i))
                    .Where(j => lines[i].IsSectionIntersectedBy(lines[j]))
                    .Select(j => (int?)j)
                    .FirstOrDefault();

                return jj.HasValue ? (jj, lines[i].IntersectionPoint(lines[jj.Value])) : (jj, null);
            }

            var intersections = polygon.Points.Index().Select(i => (i, jj: IntersectedIndex(i))).Where(v => v.jj.j.HasValue)
                .Select(v => (v.i, j: v.jj.j.Value, p:v.jj.p)).ToArray();

            if (!intersections.Any())
                return new[] {polygon};

            var rangeCount = 0;

            int[] GetRange(int i)
            {
                var r = Enumerable.Range(rangeCount, i + 1 - rangeCount).ToArray();
                rangeCount = i + 1;

                return r;
            }

            var set = new HashSet<(int, int)>();
            int GetKey() => set.Select(v => v.GetHashCode()).Aggregate(0, HashCode.Combine);

            var values = new Dictionary<int, List<(int[] r, Vector2? p, int num)>>();

            var k = 0;
            foreach (var vv in intersections)
            {
                var v = (vv.i, vv.j);

                var key = GetKey();
                
                values.TryAdd(key, new List<(int[] r, Vector2? p, int num)>());
                values[key].Add((GetRange(vv.i), vv.p, set.Count));

                //Debug.WriteLine($"{k++}: {v}, {set.Count}");

                if (set.Contains(v.Reverse()))
                    set.Remove(v.Reverse());
                else
                    set.Add(v);
            }

            values[0].Add((GetRange(points.Length - 1), null, set.Count));

            Debug.WriteLine($"Check split: {set.Count == 0} {values.Count}");

            Vector2[] CondReverse(Vector2[] points, bool reverce) => reverce ? points.Reverse().ToArray() : points;

            var polygons = values.Values.OrderBy(v=>v[0].num)
                .Select(value => new Polygon()
                {
                    Points = CondReverse(value.SelectMany(v => 
                                v.r.Select(i=>points[i])
                               .Concat(v.p.HasValue ? new[] { v.p.Value } : new Vector2[0])
                            ).ToArray(), false/*value[0].num % 2 == 1*/)
                }).ToArray();

            return polygons;
        }
    }
}
