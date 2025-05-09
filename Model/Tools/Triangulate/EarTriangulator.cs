using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Extensions;

namespace Model.Tools.Triangulate
{
    public static class EarTriangulator
    {
        public class Options
        {
            public bool DebugTriangulation { get; set; }
            public int ProtectionTriangulationCount { get; set; } = 100000;
            public int TriangulatorNetSize { get; set; } = 100;
        }

        class Node
        {
            public int ui;
            public int i;
            public bool isClone;
            public Vector2 p;
            public Node prev;
            public Node next;
            public Net<Vector2, Node> net;

            public int[] Triangle => new[] { prev.i, i, next.i };
            public bool IsEmpty => next == prev;
            public bool IsExcepted => prev == null || next == null;
            public bool IsOuterLeft => next.p.IsLeft(prev.p, p, true);
            public bool IsInnerLeft => next.p.IsLeft(prev.p, p, false);
            public bool IsInside(Node n) => n.p.IsInside(prev.p, p, next.p);
            public bool IsNotMe(Node n) => n.ui != ui && n.ui != prev.ui && n.ui != next.ui;
            public bool IsNotExcepted(Node n) => !n.IsExcepted;
            public bool IsMyCircle(Node n) => Iterate().Any(m => m == n);
            public Node[] InsideNodes => net.SelectCloseNeighbors(prev.p, p, next.p)
                .Where(IsNotMe)
                .Where(IsNotExcepted)
                .Where(IsInside)
                .Where(IsMyCircle)
                .ToArray();

            public Node MoveNext() => next;
            public Node Except()
            {
                var nextNode = next;

                prev.next = next;
                next.prev = prev;
                prev = null;
                next = null;

                return nextNode;
            }

            public Node Split(Node b)
            {
                var a = this;

                var aa = new Node() { isClone = true, ui = a.ui, i = a.i, p = a.p, prev = b, next = a.next, net = net };
                var bb = new Node() { isClone = true, ui = b.ui, i = b.i, p = b.p, prev = a, next = b.next, net = net };

                a.next.prev = aa;
                b.next.prev = bb;

                a.next = bb;
                b.next = aa;

                return aa;
            }

            public IEnumerable<Node> Iterate()
            {
                var n = this;
                do
                {
                    yield return n;
                    n = n.next;
                } while (n != this);
            }

            public override string ToString() => IsNotExcepted(this)
                ? $"{(isClone ? "[c]" : "")}({prev.i}-{i}-{next.i}): {(p - prev.p).Normal * (next.p - prev.p)}, {p}"
                : $"{(isClone ? "[c]" : "")}(null-{i}-null): {p}";
        }

        public static int[][] Triangulate(Polygon polygon, Options options = null)
        {
            options ??= new Options();

            if (polygon.Points.Length < 3)
                return Array.Empty<int[]>();

            var bi = polygon.Points.DistinctOnlyBi();
            var nodes = polygon.Points.Select((p, i) => new Node() { ui = bi.bi[i], i = i, p = p }).ToArray();

            var size = polygon.Size;
            var maxSize = Math.Max(size.x, size.y);
            var net = new Net<Vector2, Node>(nodes.Select(n => (n.p, n)), maxSize / options.TriangulatorNetSize);

            nodes.ForEach(n =>
            {
                n.net = net;
                n.prev = nodes[(n.i - 1 + nodes.Length) % nodes.Length];
                n.next = nodes[(n.i + 1) % nodes.Length];
            });

            List<int[]> FindTriangles(Node node)
            {
                var res = new List<int[]>();

                var stack = new Stack<Node>();
                stack.Push(node);

                var protectionCount = options.ProtectionTriangulationCount;

                while (stack.Count > 0)
                {
                    var n = stack.Pop();

                    if (n.IsExcepted)
                        Debugger.Break();

                    var n0 = n;

                    do
                    {
                        if (n.IsOuterLeft)
                        {
                            var insideNodes = n.InsideNodes;

                            if (insideNodes.Length > 0)
                            {
                                var nearestNode = insideNodes.OrderBy(v => (n.p - v.p).Len2).First();

                                var nn = n.Split(nearestNode);
                                net.Add(nn.p, nn);
                                stack.Push(nn);

                                if (options.DebugTriangulation)
                                {
                                    Debug.WriteLine($"%: {nearestNode} | {n.next}");
                                    Debug.WriteLine($"%: {n} | {nn}");
                                }

                                if (nn.IsExcepted)
                                    Debugger.Break();

                                n0 = n.prev;
                            }
                            else
                            {
                                res.Add(n.Triangle);

                                if (options.DebugTriangulation)
                                    Debug.WriteLine($"+: {n}");

                                if (stack.Contains(n))
                                    Debugger.Break();

                                n = n.Except();
                                n0 = n.prev;
                            }
                        }
                        else
                        {
                            if (n.IsInnerLeft)
                            {
                                if (options.DebugTriangulation)
                                    Debug.WriteLine($"-: {n}");

                                if (stack.Contains(n))
                                    Debugger.Break();
                                n = n.Except();
                                n0 = n.prev;
                            }
                            else
                            {
                                n = n.MoveNext();
                            }
                        }

                        if (--protectionCount == 0)
                        {
                            DebugNode(n);
                            Debugger.Break();
                        }
                    } while (!n.IsEmpty && n != n0);
                }

                return res;
            }

            return FindTriangles(nodes[0]).ToArray();
        }

        private static int Validate(Node n)
        {
            var check = 100000;

            var count1 = 0;
            var m = n;
            do
            {
                if (count1++ == check)
                    Debugger.Break();

                m = m.next;
            } while (m != n);

            var count2 = 0;
            do
            {
                if (count2++ == check)
                    Debugger.Break();

                m = m.prev;
            } while (m != n);

            if (count1 != count2)
                Debugger.Break();

            return count1;
        }

        private static void DebugNode(Node n)
        {
            Debug.WriteLine("Debug Node");
            n.Iterate().ForEach(m => Debug.WriteLine(m));
        }
    }
}