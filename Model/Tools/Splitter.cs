using System;
using System.Collections.Generic;
using System.Diagnostics;
using Model.Extensions;
using System.Linq;
using Model.Fourier;
using Model.Graphs;

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

        class Road
        {
            public (int i, int j) e;
            public Vector2[] forward;
            public Vector2[] backward;
            public int[] fr;
            public int[] br;
            public override string ToString() => $"{e.i}->{e.j}";
        }

        public static Polygon[] FindPerimeter(Polygon polygon, double pointPrecision = 0.01)
        {
            var points = polygon.Points;
            var lines = polygon.Lines.ToArray();
            var maxLineLen = lines.Max(l => l.Len);
            var net = new Net<Vector2, int>(points.SelectWithIndex((p, i) => (p, i)), 2 * maxLineLen);

            int Prev(int i) => (i + points.Length - 1) % points.Length;
            int Next(int i) => (i + 1) % points.Length;

            int[] IntersectedIndex(int i)
            {
                return net
                    .SelectNeighbors(points[i])
                    .Where(j => j != i && j != Prev(i) && j != Next(i))
                    .Where(j => lines[i].IsSectionIntersectedBy(lines[j]))
                    .ToArray();
            }

            IEnumerable<int> GetRange(int i, int j)
            {
                if (i > j)
                {
                    foreach (var k in Enumerable.Range(i + 1, points.Length - i - 1))
                        yield return k;
                    foreach (var k in Enumerable.Range(0, j + 1))
                        yield return k;
                }
                else
                {
                    foreach (var k in Enumerable.Range(i + 1, j - i))
                        yield return k;
                }
            }

            var baseNoGroupNodes = polygon.Points.Index()
                .Select(i => (i, js: IntersectedIndex(i)))
                .Select(v => (v.i,
                    jjs: v.js.Select(j => (j, p: lines[v.i].IntersectionPoint(lines[j])))
                        .OrderBy(vv => (points[v.i] - vv.p).Len2).ToArray()))
                .SelectMany(v => v.jjs.Select(vv => (v.i, vv.j, nodeKey: (v.i, vv.j).OrderedEdge(), vv.p)))
                .ToArray();

            var baseNodes = baseNoGroupNodes
                .SelectCirclePair((a, b) => (a.i, j: b.i, a.nodeKey, nextNodeKey: b.nodeKey, a.p))
                .GroupBy(v => v.nodeKey)
                .SelectWithIndex((gv, ind) => (nodeKey: gv.Key, i:ind, gv.First().p, roads: gv.Select(v => (v.i, v.j, v.nodeKey, v.nextNodeKey)).ToArray()))
                .ToArray();

            var theSamePointDistance2 = maxLineLen * maxLineLen * pointPrecision.Pow2();
            bool IsTheSamePoint(Vector2 a, Vector2 b) => (b - a).Len2 < theSamePointDistance2;

            var sames = baseNodes.SelectMany(a =>
                    baseNodes.Where(b => a.i < b.i).Where(b => IsTheSamePoint(a.p, b.p)).Select(b => (i: a.i, j: b.i))).ToArray();

            var sameHashes = new List<HashSet<int>>();
            foreach (var same in sames)
            {
                var hashSet = sameHashes.FirstOrDefault(v => v.Contains(same.i) || v.Contains(same.j));
                
                if (hashSet == null)
                {
                    sameHashes.Add(new HashSet<int>(new []{same.i, same.j}));
                }
                else
                {
                    hashSet.Add(same.i);
                    hashSet.Add(same.j);
                }
            }

            int GetSame(int i) => sameHashes.FirstOrDefault(h => h.Contains(i))?.Min() ?? i;

            var sameInds = baseNodes.ToDictionary(n => n.i, n => GetSame(n.i));
            var sameKeys = sameInds.ToDictionary(kv => baseNodes[kv.Key].nodeKey, kv => baseNodes[kv.Value].nodeKey);

            //var bi = baseNodes.Select(n => n.i).Where(i => i == sameInds[i]).BackIndices();

            var nodes = baseNodes
                .GroupBy(n => sameInds[n.i])
                .OrderBy(gn => gn.Key)
                .SelectWithIndex((gn, i) => (
                    i,
                    nodeKey: sameKeys[gn.First().nodeKey],
                    p: gn.Select(v => v.p).Center(),
                    roads: gn.SelectMany(n => n.roads).Select(r =>
                        (r.i, r.j, nodeKey: sameKeys[r.nodeKey], nextNodeKey: sameKeys[r.nextNodeKey])).ToArray()
                ))
                .ToArray();

            //throw new DebugException<Vector2[]>(nodes.Select(n => n.p).ToArray());

            if (!nodes.Any())
                return new[] {polygon};

            var polygons = new List<Polygon>();

            Road[] GetRoads(int aI, int bI)
            {
                Vector2[] GetPoints(IEnumerable<int> r, int nI) =>
                    r.Select(i => points[i]).Concat(new[] { nodes[nI].p }).ToArray();

                IEnumerable<(Vector2[] fw, Vector2[] bw, int[] fr, int[] br)> GetPointRange(int aaI, int bbI) => nodes[aaI]
                    .roads.Where(v => v.nodeKey == nodes[aaI].nodeKey && v.nextNodeKey == nodes[bbI].nodeKey)
                    .Select(v => (
                        fw: GetPoints(GetRange(v.i, v.j), bbI),
                        bw: GetPoints(GetRange(v.i, v.j).Reverse(), aaI), 
                        fr: GetRange(v.i, v.j).ToArray(), 
                        br: GetRange(v.i, v.j).Reverse().ToArray()));

                Road[] GetRds(IEnumerable<(Vector2[] fw, Vector2[] bw, int[] fr, int[] br)> roads) => roads
                    .Where(r => aI != bI || r.fr.Length > 0)
                    .Select(r => new Road()
                    {
                        e = (aI, bI),
                        forward = r.fw,
                        backward = r.bw,
                        fr = r.fr,
                        br = r.br
                    }).ToArray();

                if (aI > bI)
                    (aI, bI) = (bI, aI);

                if (aI == bI)
                {
                    var selfRoads = GetPointRange(aI, bI).ToArray();

                    return GetRds(selfRoads);
                }

                var roads = GetPointRange(aI, bI).Concat(GetPointRange(bI, aI).Select(v=>(fw:v.bw, bw:v.fw, fr:v.br, br:v.fr))).ToArray();

                return GetRds(roads);
            }
            
            var nodeList = nodes.Select(n => n.nodeKey).ToList();

            var edges = nodes.SelectMany(n =>
                    n.roads.Select(v => (i: nodeList.IndexOf(v.nodeKey), j: nodeList.IndexOf(v.nextNodeKey)).OrderedEdge()))
                .Distinct()
                .ToArray();

            var roads = edges.ToDictionary(e => e, e => GetRoads(e.i, e.j));

            var g = new Graph(edges);
            //g.WriteToDebug("Base graph: ");

            double GetAngle(Vector2 a, Vector2 b, Vector2 c)
            {
                var ab = (b - a).Normed;
                var bc = (c - b).Normed;

                return Math.Atan2(ab.Normal * bc, ab * bc);
            }

            // find right road to start with
            var startRoadInfo = g.edges.SelectMany(e => roads[e.e].Select(r => (e, r)))
                .OrderByDescending(v => v.r.forward.Max(vv => vv.x)).First();

            var startNode = startRoadInfo.r.forward.ToPolygon().IsLeft() ? startRoadInfo.e.b : startRoadInfo.e.a;

            var startWay = (b: startNode, startRoadInfo.e, startRoadInfo.r);
            (int i, int j) GetWayDirectionKey((Graph.Node b, Graph.Edge e, Road r) p) => p.e.b == p.b ? p.e.e : p.e.e.Reverse();
            Vector2[] GetWayPoints((Graph.Node b, Graph.Edge e, Road r) p) => GetWayDirectionKey(p) == p.e.e ? p.r.forward : p.r.backward; // тут

            var way = startWay;
            Vector2[] prevWps = null; // todo: first road should have length > 1

            var perimeter = new Dictionary<(int i, int j), Vector2[]>();
            var stopCount = 10 * roads.Values.Sum(v => v.Length);

            // find perimeter
            do
            {
                //Debug.WriteLine($"{GetWayDirectionKey(way)}, {(GetWayDirectionKey(way) == way.e.e ? "forward" : "backward")}");

                var wps = GetWayPoints(way);

                if (way.e.a == way.e.b)
                {
                    polygons.Add(new Polygon()
                    {
                        Points = wps
                    }.ToLeft());
                }
                else
                {
                    perimeter.Add(GetWayDirectionKey(way), wps);
                }

                var a = wps.Length > 1 ? wps[^2] : prevWps[^1];
                var b = wps[^1];
                prevWps = wps;

                var infos = way.b.edges
                    .SelectMany(e => roads[e.e]
                        .Where(r=>r != way.r)
                        .Select(r=>(e.Another(way.b), e, r))
                        .Select(p=>(p, ps:GetWayPoints(p)))
                        .Select(v=>(v.p, v.ps, ang: GetAngle(a, b, v.ps[0]))))
                    .OrderBy(v => v.ang)
                    .ToArray(); // todo: first

                way = infos.First().p;

                //Debug.WriteLine($"angles: {infos.Select(v => $"{v.ang:F2} {GetWayDirectionKey(v.p)}").SJoin(", ")}");
                if (stopCount-- == 0)
                    throw new Exception("stopped");

            } while (startWay.r != way.r);

            if (perimeter.Keys.Count == 0)
                return polygons.ToArray();

            // get polygons from perimeter
            var pg = new Graph(perimeter.Keys);

            while (pg.MetaGroup())
            {
                foreach (var e in pg.edges.Where(e => e.a == e.b).ToArray())
                {
                    //Debug.WriteLine($"{e}");

                    //Debug.WriteLine(e.meta.SelectPair().SJoin());
                    //Debug.WriteLine(e.meta.SelectPair().Select(dirE => perimeter[dirE]).SelectCirclePair((a, b) => (b[0] - a[^1]).Len).SJoin());

                    polygons.Add(new Polygon()
                    {
                        Points = e.meta.SelectPair().SelectMany(dirE => perimeter[dirE]).ToArray()
                    });

                    pg.RemoveEdge(e);
                }

                var doublePairs = pg.edges.Where(e => e.a.edges.Any(ee => ee.e.Reverse() == e.e)).Select(e=>e.e.OrderedEdge()).Distinct().ToArray();

                foreach (var p in doublePairs)
                {
                    var aE = pg.edges.First(e => e.e == p);
                    var bE = pg.edges.First(e => e.e.Reverse() == p);

                    var meta = Graph.JoinMetas(aE.meta, bE.meta);

                    polygons.Add(new Polygon()
                    {
                        Points = meta.SelectPair().SelectMany(dirE => perimeter[dirE]).ToArray()
                    });

                    pg.RemoveEdge(aE);
                    pg.RemoveEdge(bE);
                }
            }

            if (pg.edges.Count > 0)
                Debug.WriteLine("Incorrect perimeter!");

            return polygons.ToArray();
        }

        class Info
        {
            public bool taken;
            public override string ToString() => $"{taken}";
        }

        // todo: покрывает всю поверхность до определенной степени сложности (см. ниже)
        // todo: не умеет четко разделять области чтобы не было пересечений
        // в большинстве случаев сделает полное покрытие фигуры после объединения (чем пока и хорош)
        public static Polygon[] SplitIntersections(Polygon polygon, FrOptions options = null)
        {
            options ??= new FrOptions();

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

            var nodeList = nodes.Select(n => n.nodeKey).ToList();

            Vector2[][] GetRangePoints(int aI, int bI)
            {
                Vector2[] GetPoints(IEnumerable<int> r) =>
                    r.Select(i => points[i]).Concat(new[] { nodes[bI].p }).ToArray();

                var rs1 = nodes[aI]
                    .list.Where(v => v.nodeKey == nodes[aI].nodeKey && v.nextNodeKey == nodes[bI].nodeKey)
                    .Select(v => GetRange(v.i, v.j));

                var rs2 = nodes[bI]
                    .list.Where(v => v.nodeKey == nodes[bI].nodeKey && v.nextNodeKey == nodes[aI].nodeKey)
                    .Select(v => GetRange(v.i, v.j).Reverse());

                return rs1.Select(GetPoints).Concat(rs2.Select(GetPoints)).ToArray();
            }

            var edges = nodes.SelectMany(n =>
                    n.list.Select(v => (i: nodeList.IndexOf(v.nodeKey), j: nodeList.IndexOf(v.nextNodeKey)).OrderedEdge()))
                .ToArray();

            var edgeInfos = edges.Distinct().ToDictionary(v => v, _ => new Info());

            var gEdges = edges
                .SelectMany(v=>new[]{v, v.Reverse()})
                .Distinct()
                .Select(v => (e: v, ps: GetRangePoints(v.i, v.j)))
                .Select(v => (v.e, ps: v.ps.Select(vv => (ps: vv, len: Len(vv), info: edgeInfos[v.e.OrderedEdge()])).ToArray()))
                .ToDictionary(v => v.e, v => v.ps);

            (Vector2[] ps, double len, Info info) GetMinRangePoints(int aI, int bI) => gEdges[(aI, bI)].OrderBy(v => v.len).First();

            (Vector2[] ps, double len, Info[] infos) GetPathRangePoints(int[] path)
            {
                var pathInfos = path.SelectPair(GetMinRangePoints).ToArray();

                return (pathInfos.SelectMany(p => p.ps).ToArray(), pathInfos.Sum(p => p.len),
                    pathInfos.Select(p => p.info).ToArray());
            }

            (Vector2[] points, double aLen, double bLen, bool aTaken, bool bTaken, Info[] infos) GetEdgesRangePoints(Graph.Edge eA, Graph.Edge eB)
            {
                if (eA.meta.Length == 2 && eB.meta.Length == 2)
                {
                    var ee = gEdges[eA.e];

                    Debug.WriteLine($"metaA:({eA.meta.SJoin(", ")}| {ee[0].len}), metaB:({eB.meta.SJoin(", ")}| {ee[0].len})");

                    return (ee[0].ps.Concat(ee[1].ps.Reverse()).ToArray(), ee[0].len, ee[1].len, ee[0].info.taken, ee[1].info.taken, new[]{ee[0].info, ee[1].info});
                }
                else
                {
                    var rsA = GetPathRangePoints(eA.meta);
                    var rsB = GetPathRangePoints(eB.meta);

                    Debug.WriteLine($"metaA:({eA.meta.SJoin(", ")}| {rsA.len}), metaB:({eB.meta.SJoin(", ")}| {rsB.len})");

                    return (rsA.ps.Concat(rsB.ps.Reverse()).ToArray(), rsA.len, rsB.len, rsA.infos.Any(v=>v.taken), rsB.infos.Any(v => v.taken), rsA.infos.Concat(rsB.infos).ToArray());
                }
            }

            void TakeMeta(int[] meta) => meta.SelectPair().SelectMany(e => gEdges[e]).ForEach(v => v.info.taken = true);

            var g = new Graph(edges);
            g.WriteToDebug("Base graph: ");

            var polygons = new List<Polygon>();

            // self polygons
            foreach (var e in g.edges.Where(e=>e.a==e.b).ToArray())
            {
                var ee = gEdges[e.e];
                ee[0].info.taken = true;

                polygons.Add(new Polygon()
                {
                    Points = ee[0].ps
                }.ToLeft());

                g.RemoveEdge(e);
            }

            while (true)
            {
                // two nodes polygons
                foreach (var edgesPair in g.edges.GroupBy(e => e.e).Where(ge => ge.Count() == 2).Select(ge=>ge.ToArray()).ToArray())
                {
                    var e = edgesPair[0];
                    var e1 = edgesPair[1];

                    var pInfo = GetEdgesRangePoints(e, e1);

                    if (!pInfo.aTaken && pInfo.bTaken)
                        (e, e1) = (e1, e);
                    else if (pInfo.aTaken ^ !pInfo.bTaken)
                    {
                        if (pInfo.aLen < pInfo.bLen)
                            (e, e1) = (e1, e);
                    }

                    polygons.Add(new Polygon()
                    {
                        Points = pInfo.points
                    }.ToLeft());

                    var needRemoveAll = e.a.edges.Count == 2 || e.b.edges.Count == 2;

                    g.RemoveEdge(e);

                    if (needRemoveAll)
                        g.RemoveEdge(e1);

                    pInfo.infos.ForEach(v => v.taken = true);
                }

                if (g.edges.Count == 0)
                    break;

                //g.WriteToDebug("before: ");
                var isGrouped = g.MetaGroup(); // replace nodes with 2 edges by single edge saving metadata
                //g.WriteToDebug("after: ");

                if (!isGrouped)
                {
                    // 3 nodes polygon
                    var edgeInfo = g.edges.Where(e =>e.a.Siblings.Intersect(e.b.Siblings).Any())
                        .Select(e=>new{e, info= GetPathRangePoints(e.meta)})
                        .FirstOrDefault();

                    if (edgeInfo == null)
                        break;

                    var edge = edgeInfo.e;

                    var c = edge.a.Siblings.Intersect(edge.b.Siblings).First();
                    var aE = edge.a.edges.First(e => e.Another(edge.a) == c);
                    var bE = edge.b.edges.First(e => e.Another(edge.b) == c);

                    var meta = Graph.JoinMetas(Graph.JoinMetas(edge.meta, aE.meta), bE.meta);

                    polygons.Add(new Polygon()
                    {
                        Points = GetPathRangePoints(meta).ps
                    }.ToLeft());

                    g.RemoveEdge(edge);

                    TakeMeta(meta);
                }
            }

            // 4 and more nodes polygon 
            // todo: немного хак. На сложных графах можно что-то не добрать
            if (g.edges.Count > 0)
            {
                while (true)
                {
                    var edge = g.edges.FirstOrDefault(ee => ee.meta.SelectPair().Any(e=>gEdges[e].Any(v => !v.info.taken)));

                    if (edge == null)
                        break;

                    g.RemoveEdge(edge);
                    var path = g.FindPath(edge.a, edge.b).ToArray();
                    g.AddEdge(edge);

                    var pathEdges = path.SelectCirclePair((a, b) => a.ToEdge(b)).ToArray();
                    var meta = pathEdges.Select(e => e.meta).Aggregate(Graph.JoinMetas);

                    polygons.Add(new Polygon()
                    {
                        Points = GetPathRangePoints(meta).ps
                    }.ToLeft());

                    TakeMeta(meta);
                }
            }

            //gEdges.Values.SelectMany(v => v).Select(v => $"info: {v.ps.Length}, {v.len}, {v.info.taken}").ForEach(v=>Debug.WriteLine(v));

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
