using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Model.Extensions;
using Model.Libraries;

namespace Model.Tools.Triangulate
{
    public static class Triangulator
    {
        class Node : IEnumerable<Node>
        {
            public int i;
            public Node prev;
            public Node next;
            public Vector2 p;
            public double ang;
            public Func<Vector2, bool>[] insideFns;
            public Func<Vector2, bool> isCorrectFn;

            public IEnumerator<Node> GetEnumerator()
            {
                var node = this;
                do
                {
                    yield return node;
                    node = node.next;
                } while (node != this);
            }

            public override string ToString() => $"{i}: {ang:F5}, ({p.x:F5}, {p.y:F5})";
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        // одна точка - один угол
        // пересчитать углы, если забрал точку
        // может sortedstack
        // забирать треугольники с минимальными углами
        // проверить, что взятие треугольника возможно (внутри нет точек)
        // брать по очереди тот треугольник, который возможен
        public static int[][] Triangulate(Polygon polygon, double incorrectFix = 0)
        {
            if (polygon.Points.Length < 3)
                return Array.Empty<int[]>();

            var maxLen = polygon.MaxLinesLen;
            var brokenPolygonCoff = incorrectFix * maxLen;

            var nodes = polygon.Points.SelectWithIndex((p, i) => new Node() { i = i, p = p }).ToArray();

            nodes.ForEach(n =>
            {
                n.prev = nodes[(n.i - 1 + nodes.Length) % nodes.Length];
                n.next = nodes[(n.i + 1) % nodes.Length];
            });

            double GetAng(Node n) => -Angle.LeftDirection(n.prev.p, n.p, n.next.p);

            Func<Vector2, bool> GetIsLeftFn(Vector2 a, Vector2 b, double epsilon) =>
                new Line2(a, b).GetIsLeftFn(epsilon);

            Func<Vector2, bool>[] GetInsideFns(Node n) => new[]
            {
                GetIsLeftFn(n.prev.p, n.p, -brokenPolygonCoff),
                GetIsLeftFn(n.p, n.next.p, -brokenPolygonCoff),
                GetIsLeftFn(n.next.p, n.prev.p, -brokenPolygonCoff),
            };

            Func<Vector2, bool> GetIsCorrectFn(Node n) => GetIsLeftFn(n.prev.p, n.p, brokenPolygonCoff);

            nodes.ForEach(n =>
            {
                n.ang = GetAng(n);
                n.insideFns = GetInsideFns(n);
                n.isCorrectFn = GetIsCorrectFn(n);
            });

            var net = new Net<Vector2, Node>(nodes.Select(n => (n.p, n)), maxLen);

            var sortedStack = new SortedStack<Node>(nodes.Length);
            nodes.ForEach(n => sortedStack.Push(n, n.ang));

            bool IsInside(Node n, Vector2 p) => n.insideFns.All(isLeft => isLeft(p));

            bool CanTake(Node n) => n.isCorrectFn(n.next.p) &&
                                    net.SelectNeighbors(n.p)
                                        .Where(nn => nn != n && nn != n.prev && nn != n.next)
                                        .All(nn => !IsInside(n, nn.p));

            void TakeNode(Node n)
            {
                n.prev.next = n.next;
                n.next.prev = n.prev;
                n.prev.ang = GetAng(n.prev);
                n.next.ang = GetAng(n.next);
                n.prev.insideFns = GetInsideFns(n.prev);
                n.next.insideFns = GetInsideFns(n.next);
                n.prev.isCorrectFn = GetIsCorrectFn(n.prev);
                n.next.isCorrectFn = GetIsCorrectFn(n.next);
                sortedStack.Update(n.prev, n.prev.ang);
                sortedStack.Update(n.next, n.next.ang);
            }

            Node FindNode()
            {
                var n = sortedStack.FirstOrDefault(CanTake);
                sortedStack.Pop(n);

                return n;
            }

            int[] GetTriangle(Node n) => new[] { n.prev.i, n.i, n.next.i };

            var triangles = new List<int[]>(nodes.Length - 2);

            while (sortedStack.Count > 2)
            {
                //sortedStack.ForEach(n => Debug.WriteLine($"{n.i}: {n.ang * 180 / Math.PI:N}"));
                //Debug.WriteLine("===========================");

                var n = FindNode();

                if (n == null)
                {
                    Debug.WriteLine($"Incorrect triangulation: {sortedStack.Count - 2}/{nodes.Length}");
                    break;

                    //var ll = sortedStack.First().ToList();
                    //sortedStack.ForEach(n => Debug.WriteLine($"{ll.IndexOf(n)}, {n.i}: {n.ang * 180 / Math.PI:N}"));
                    //n = sortedStack.First();

                    //var ens = net.SelectNeighbors(n.p)
                    //    .Where(nn => nn != n && nn != n.prev && nn != n.next)
                    //    .Where(nn => IsInside(n, nn.p))
                    //    .ToArray();

                    //throw new DebugException<(Polygon, int[][], Vector2[])>((polygon, triangles.ToArray(),
                    //    ll.Select(n => n.p).ToArray()));
                }

                TakeNode(n);
                triangles.Add(GetTriangle(n));
            }

            return triangles.ToArray();
        }
    }
}