using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model.Tools
{
    public static class Triangulator
    {
        private static readonly Func<double, double, double> hFn = (a, b) => (4 *a *a - b*b).Sqrt() / 2;

        struct TrioInfo
        {
            public Trio Trio;
            public Trio TriangleTrio;
            public Line2 Line;
            public Vector2 C;

            public Vector2 A => Line.A;
            public Vector2 B => Line.B;
            public Vector2 MiddleOfAC => (Line.A + C) / 2;
            public Vector2[] Points => new[] { A, B, C };

            public double ACLen2 => (C - Line.A).Len2;
            public bool CheckLeftPoint(Vector2 point) => Line.IsLeft(point);
            public bool IsLeftPoint => CheckLeftPoint(C);
            public double Scalar => -Line.AB * (C - Line.B);
            public double NormedScalar => -Line.AB.Normed * (C - Line.B).Normed;
            public bool IsAcuteAngle => Scalar > 0;
            public bool IsLess120Angle => NormedScalar > -0.5;
            public bool Is180Angle => NormedScalar < -0.5;
        }

        class TrShape2
        {
            public List<Vector2> Points;
            public List<int> Perimeter;
            public List<int> Dynamic;
            public List<Trio> Trios;

            public int CorrectInd(int i) => (i + Perimeter.Count) % Perimeter.Count;
            public int NextInd(int i, int count = 1) => CorrectInd(i + count);
            public int PrevInd(int i, int count = 1) => CorrectInd(i - count);
            public Trio NextTrio(Trio trio) => new Trio(trio.j, trio.k, NextInd(trio.k));
            public Trio PrevTrio(Trio trio) => new Trio(PrevInd(trio.i), trio.i, trio.j);

            public int CorrectDelInd(int i, int delI) => i < delI ? i : CorrectInd(i - 1);
            public Trio CorrectDelTrio(Trio trio, int delI) => new Trio(CorrectDelInd(trio.i, delI), CorrectDelInd(trio.j, delI), CorrectDelInd(trio.k, delI));

            public Trio GetTriangleTrio(Trio trio) => new Trio(Perimeter[trio.i], Perimeter[trio.j], Perimeter[trio.k]);

            public Vector2 GetPoint(int i) => Points[Perimeter[i]];

            public TrioInfo GetTrioInfo(Trio trio) => new TrioInfo
            {
                Trio = trio,
                TriangleTrio = GetTriangleTrio(trio),
                Line = new Line2(GetPoint(trio.i), GetPoint(trio.j)),
                C = GetPoint(trio.k)
            };

            public Trio TakeTriangle(Trio t)
            {
                Trios.Add(GetTriangleTrio(t));
                Perimeter.RemoveAt(t.j);
                t = CorrectDelTrio(t, t.j);

                return t;
            }

            public Trio AddTrioPoint(Trio t, Vector2 point)
            {
                var ind = Points.Count;
                Points.Add(point);
                Dynamic.Add(ind);
                Trios.Add(new Trio(Perimeter[t.i], Perimeter[t.j], ind));
                Trios.Add(new Trio(Perimeter[t.j], Perimeter[t.k], ind));
                Perimeter[t.j] = ind;
                t = PrevTrio(PrevTrio(t));

                return t;
            }

            public Trio AddTrioCenterPoint(Trio t)
            {
                var center = t.ToArray().Select(i => GetPoint(i)).Center();
                return AddTrioPoint(t, center);
            }

            public Trio AddTwoTrioCenterPoint(Trio t)
            {
                var points = new Vector2[] { GetPoint(t.i), GetPoint(t.j), GetPoint(t.k), GetPoint(NextInd(t.k)) };
                return AddTrioPoint(t, points.Center());
            }

            Vector2 GetEdgePoint(int i, int j, double edgeLen)
            {
                var a = GetPoint(i);
                var b = GetPoint(j);
                var h = hFn((b - a).Len, (b - a).Len);
                var n = (a - b).Normal.Normed;
                var p = 0.5 * (a + b) + h * n;

                return p;
            }

            public Trio AddTrioEdgePoint(Trio t, double edgeLen)
            {
                var edgePoint = GetEdgePoint(t.i, t.j, edgeLen);
                var ind = Points.Count;
                Points.Add(edgePoint);
                Dynamic.Add(ind);
                Trios.Add(new Trio(Perimeter[t.i], Perimeter[t.j], ind));
                Perimeter.Insert(t.j, ind);

                return t;
            }

            public IEnumerable<TrioInfo> SelectPerimeterLeftTrioInfos()
            {
                Trio t = Trio.Start;
                do
                {
                    var info = GetTrioInfo(t);
                    if (info.IsLeftPoint)
                        yield return info;

                    t = NextTrio(t);
                } while (!t.IsStart);
            }

        }

        private static void FillByTriangles(TrShape2 trShape, double edgeLen)
        {
            var debug = 20;
            var edgeLen2 = edgeLen * edgeLen;

            Trio t = Trio.Start;
            var repeat = 3;
            while (true)
            {
                if (t.IsStart && repeat-- == 0)
                    break;

                t = trShape.NextTrio(t);

                var trioInfo = trShape.GetTrioInfo(t);
                if (!trioInfo.IsLeftPoint)
                    continue;

                if (trioInfo.ACLen2 < edgeLen2)
                {
                    t = trShape.TakeTriangle(t);
                    repeat = 3;
                    continue;
                }

                //var points4 = new []{ trioInfo.A, trioInfo.B, trioInfo.C, nextTrioInfo.C };
                //var center4 = points4.Center();
                //if (points4.All(p => (p - center4).Len2 < edgeLen2))
                //{
                //    t = trShape.AddTrioPoint(t, center4);
                //    repeat = 3;
                //    continue;
                //}

                if (trioInfo.IsLess120Angle)
                {
                    var nextTrioInfo = trShape.GetTrioInfo(trShape.NextTrio(t));
                    if (nextTrioInfo.IsLess120Angle)
                    {
                        t = trShape.AddTwoTrioCenterPoint(t);
                    }
                    else
                    {
                        t = trShape.AddTrioCenterPoint(t);
                    }

                    repeat = 3;
                    continue;
                }

                //if (проверка расстояния)
                if (repeat == 1)
                {
                    t = trShape.AddTrioEdgePoint(t, edgeLen);

                    if (--debug == 0)
                        return;

                    repeat = 3;
                    continue;
                }
            }
        }

        //private static void FillCircumferentially(TrShape2 trShape, double edgeLen)
        //{
        //    var edgeLen2 = edgeLen * edgeLen;

        //    Vector2 GetEdgePoint(int i, int j)
        //    {
        //        var a = trShape.Points[i];
        //        var b = trShape.Points[j];
        //        var n = (a - b).Normal;
        //        var p = 0.5 * (a + b) + h * n;

        //        return p;
        //    }


        //    Trio t = Trio.Start;
        //    do
        //    {
        //        var threeth = t.ToArray().Select(i => trShape.GetPoint(i)).ToArray();
        //        var center3 = threeth.Center();

        //        if (threeth.All(p => (p - center3).Len2 < edgeLen2))
        //        {
        //            t = trShape.AddTrioPoint(t, center3);
        //        }

        //        t = trShape.NextTrio(t);
        //    } while (!t.IsStart);



        //    //var pairs = trShape.Perimeter.Index().CirclePairs().Evens().ToArray();

        //    //var nPoint = trShape.Points.Count;
        //    //foreach ((int i, int j) in pairs)
        //    //{
        //    //    var ii = trShape.Perimeter[i];
        //    //    var jj = trShape.Perimeter[j];
        //    //    var p = GetEdgePoint(ii, jj);

        //    //    var kk = trShape.Points.Count();
        //    //    trShape.Points.Add(p);
        //    //    trShape.Dynamic.Add(kk);
        //    //    trShape.Trios.Add(new Trio(ii, jj, kk));
        //    //}

        //    //var insertCount = 0;
        //    //var ind = nPoint;
        //    //foreach ((int i, int j) in pairs)
        //    //    trShape.Perimeter.Insert(i+1 + insertCount++, ind++);
        //}

        public static Shape2 Triangulate(Polygon polygon, double edgeLen)
        {
            var edgeLen2 = edgeLen * edgeLen;

            var shape = new Shape2
            {
                Polygon = polygon,
                Convexes = new[] { polygon.Points.Index().ToArray() },
                IsValid = false
            };
            
            shape = SplitEdges(shape, edgeLen);

            var trShape = new TrShape2
            {
                Points = shape.Points.ToList(),
                Perimeter = shape.Convexes[0].ToList(),
                Trios = new List<Trio>(),
                Dynamic = new List<int>(),
            };

            var n = 2;
            while (n-- > 0)
            {
                foreach (var info in trShape.SelectPerimeterLeftTrioInfos())
                    if (info.ACLen2 < edgeLen2)
                        trShape.TakeTriangle(info.Trio);

                foreach (var info in trShape.SelectPerimeterLeftTrioInfos())
                    if (info.IsLess120Angle)
                    {
                        var nextTrioInfo = trShape.GetTrioInfo(trShape.NextTrio(info.Trio));
                        if (nextTrioInfo.IsLess120Angle)
                        {
                            trShape.AddTwoTrioCenterPoint(info.Trio);
                        }
                        else
                        {
                            trShape.AddTrioCenterPoint(info.Trio);
                        }
                    }

                trShape.AddTrioEdgePoint(Trio.Start, edgeLen);
            }

            //while (trShape.Perimeter.Count > 3)
            {
                //FillByTriangles(trShape, edgeLen);

                //FillCircumferentially(trShape);

                //Trio t = Trio.Start;
                //do
                //{

                //    t = trShape.NextTrio(t);
                //} while (!t.IsStart);

                //FillSimplePoints(trShape, edgeLen);
            }

            shape = new Shape2
            {
                Points = trShape.Points.ToArray(),
                Convexes = trShape.Trios.Select(t => t.ToArray()).Concat(new[] { trShape.Perimeter.ToArray() }).ToArray(),
                IsValid = false
            };

            //shape = TriangulateFn(shape);
            //shape = SplitEdges(shape, edgeLen);
            //shape = TriangulateFn(shape);
            //shape = Shake(shape, edgeLen, 1);

            return shape;
        }

        private static Shape2 SplitEdges(Shape2 s, double edgeLen)
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


        private static Shape2 Shake(Shape2 shape, double edgeLen, int count = 10)
        {
            List<Node> nodes = new List<Node>();

            //bool IsInside(Vector2 v) => shape.Bounds.SelectCirclePair((i,j))

            Vector2 GetNodeStress(Node node)
            {
                return node.links.Select(link =>
                {
                    var ab = shape.Points[node.i] - shape.Points[link.i];
                    var abNormAdge = edgeLen * ab.Normed;

                    return 0.5 / count * (abNormAdge - ab);
                }).Sum();
            }

            Node GetNode(int i, bool isStatic = false)
            {
                if (i < nodes.Count)
                    return nodes[i];

                var node = new Node { i = i, isStatic = isStatic, links = new List<Node>() };
                nodes.Add(node);

                return node;
            }

            foreach ((int i, int j) in shape.OrderedEdges)
            {
                var iNode = GetNode(i);
                var jNode = GetNode(j);
                iNode.links.Add(jNode);
                jNode.links.Add(iNode);
            }

            var flowNodes = nodes.Where(node => !node.isStatic).ToList();
            var points = shape.Points.ToArray();
            for (var i = 0; i < count; i++)
            {
                flowNodes.ForEach(node => points[node.i] += GetNodeStress(node));
            }

            return new Shape2
            {
                Points = points,
                Convexes = shape.Convexes
            };
        }


        class Node
        {
            public int i;
            public List<Node> links;
            public bool isStatic;
        }

    }
}
