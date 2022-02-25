using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Model.Extensions;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Model.Fourier;
using Model.Graphs;
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

        public static Polygon[] SplitIntersections(Polygon polygon, FrOptions options = null)
        {
            options ??= new FrOptions();

            // собрать направленные кривые. Точка входа и точка выхода (возможно одна точка)
            // набор точке пересечений и связь кривых с точками
            // в направленном графе найти все минимальные замкнутые пути

            var points = polygon.Points;
            var lines = polygon.Lines.ToArray();
            var net = new Net<Vector2, int>(points.SelectWithIndex((p, i) => (p, i)), 2 * lines.Max(l => l.Len));

            int Prev(int i) => (i + points.Length - 1) % points.Length;
            int Next(int i) => (i + 1) % points.Length;

            int? IntersectedIndex(int i)
            {
                return net
                    .SelectNeighbors(points[i])
                    .Where(j => j != i && j != Prev(i) && j != Next(i))
                    .Where(j => lines[i].IsSectionIntersectedBy(lines[j]))
                    .Select(j => (int?)j)
                    .FirstOrDefault();
            }

            IEnumerable<int> GetRange(int i, int j)
            {
                if (i > j)
                {
                    foreach (var k in Enumerable.Range(i + 1, points.Length - i - 1))
                        yield return k;
                    foreach (var k in Enumerable.Range(0, j))
                        yield return k;
                }
                else
                {
                    foreach (var k in Enumerable.Range(i + 1, j - i - 1))
                        yield return k;
                }
            }

            var nodes = polygon.Points.Index()
                .Select(i => (i, j: IntersectedIndex(i)))
                .Where(v => v.j.HasValue)
                .Select(v => (v.i, j: v.j.Value, nodeKey: (v.i, v.j.Value).OrderedEdge()))
                .SelectCirclePair((a, b) => (a.i, j: b.i, a.nodeKey, nextNodeKey: b.nodeKey))
                .GroupBy(v => v.nodeKey)
                .Select(gv => (nodeKey: gv.Key, p: lines[gv.Key.i].IntersectionPoint(lines[gv.Key.j]), list: gv.Select(v => (v.i, v.j, v.nodeKey, v.nextNodeKey)).ToArray()))
                .ToArray();

            if (!nodes.Any())
                return new[] { polygon };

            double Len(Vector2[] ps) => ps.SelectPair((a, b) => (b - a).Len).Sum();

            Vector2[][] GetRangePoints(int aI, int bI)
            {
                Vector2[] GetPoints(IEnumerable<int> r) =>
                    r.Select(i => points[i]).Concat(new[] {nodes[bI].p}).ToArray();

                var rs1 = nodes[aI]
                    .list.Where(v => v.nodeKey == nodes[aI].nodeKey && v.nextNodeKey == nodes[bI].nodeKey)
                    .Select(v => GetRange(v.i, v.j));

                var rs2 = nodes[bI]
                    .list.Where(v => v.nodeKey == nodes[bI].nodeKey && v.nextNodeKey == nodes[aI].nodeKey)
                    .Select(v => GetRange(v.i, v.j).Reverse());

                return rs1.Select(GetPoints).Concat(rs2.Select(GetPoints)).ToArray();
            }

            Vector2[] GetMinRangePoints(int aI, int bI)
            {
                return GetRangePoints(aI, bI).OrderBy(Len).First();
            }
            //foreach (var node in nodes)
            //{
            //    Debug.WriteLine($"{node.p}");
            //}
            //return new Polygon[]
            //{
            //    new Polygon(){Points = nodes.Select(n=>n.p).ToArray()}
            //};

            var nodeList = nodes.Select(n => n.nodeKey).ToList();

            var edges = nodes.SelectMany(n =>
                n.list.Select(v => (i:nodeList.IndexOf(v.nodeKey), j:nodeList.IndexOf(v.nextNodeKey)))).ToArray();

            var g = new Graph(edges);

            var polygons = new List<Polygon>();

            foreach (var e in g.edges.Where(e=>e.a==e.b).ToArray())
            {
                polygons.Add(new Polygon()
                {
                    Points = GetRangePoints(e.a.i, e.b.i)[0]
                }.ToLeft());

                g.RemoveEdge(e);
            }

            foreach (var ge in g.edges.GroupBy(e=>e.e).Where(ge => ge.Count() == 2).ToArray())
            {
                var e = ge.First();
                var rs = GetRangePoints(e.a.i, e.b.i);

                polygons.Add(new Polygon()
                {
                    Points = rs[0].Concat(rs[1].Reverse()).ToArray()
                }.ToLeft());

                g.RemoveEdge(e);
            }

            //g.WriteToDebug();

            Polygon GetPolygon(Graph.Node[] path)
            {
                return new Polygon()
                {
                    Points = path.SelectCirclePair((a, b) => GetMinRangePoints(a.i, b.i)).SelectMany(v => v).ToArray()
                }.ToLeft();
            }


            //return new Polygon[]
            //{
            //    new Polygon() {Points = g.Edges.SelectMany(e => new[] {points[e.i], points[e.j]}).ToArray()}
            //};

           
            var gs = g.SplitToMinCircles();

            polygons.AddRange(gs.Select(GetPolygon));

            return polygons.ToArray();
        }

        public static Polygon[] SplitIntersections1(Polygon polygon)
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
                    Points = value.SelectMany(v => 
                                v.r.Select(i=>points[i])
                               .Concat(v.p.HasValue ? new[] { v.p.Value } : new Vector2[0])
                            ).ToArray()
                }.ToLeft()).ToArray();

            return polygons;
        }
    }
}
