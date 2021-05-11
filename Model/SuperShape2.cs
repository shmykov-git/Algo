using Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class SuperShape2
    {
        public Vector2[] points;
        public List<Convex> convexes;

        private SuperPoint[] superPoints;

        public SuperShape2(Shape2 shape)
        {
            points = shape.Points;
            convexes = shape.Convexes.Select(convex => new Convex { indices = convex.ToList() }).ToList();
        }

        private void MakeSuperPoints()
        {
            if (superPoints != null)
                return;

            superPoints = points.Index().Select(i => new SuperPoint { i = i }).ToArray();
            foreach (var convex in convexes)
                foreach (var edge in convex.edges)
                {
                    if (!superPoints[edge.i].neighbors.Contains(edge.j))
                        superPoints[edge.i].neighbors.Add(edge.j);

                    if (!superPoints[edge.j].neighbors.Contains(edge.i))
                        superPoints[edge.j].neighbors.Add(edge.i);
                }
        }

        public void Cut(Polygon polygon, bool inside = true)
        {
            var sections = polygon.Lines.ToArray();
            var cutConvexInfos = convexes.SelectMany(convex =>
                sections.SelectMany(section =>
                    convex.edges.Where(e => section.IsSectionIntersectedBy((points[e.i], points[e.j])))
                        .Select(e => new
                        {
                            edge = e,
                            left = section.IsLeft(points[e.i]) ? e.i : e.j,
                            convex = convex
                        })));

            var gInfos = cutConvexInfos.GroupBy(c => c.convex).Select(gc => new
            {
                cutconvex = gc.Key,
                lefts = gc.Select(c => c.left).Distinct().ToArray(),
                rights = gc.Select(c => c.edge.Another(c.left)).Distinct().ToArray(),
                cutedges = gc.Select(c => c.edge).Distinct().ToArray()
            }).ToArray();

            var rights = gInfos.SelectMany(g => g.rights).Distinct();
            var lefts = gInfos.SelectMany(g => g.lefts).Distinct();
            var pure = inside ? lefts.Except(rights).ToList() : rights.Except(lefts).ToList();

            foreach (var info in gInfos)
                convexes.Remove(info.cutconvex);

            MakeSuperPoints();

            var field = GetField(pure);

            foreach (var i in EmptyPoints)
                field.Remove(i);

            var backIndices = field.BackIndices();
            var cutConvexes = convexes.Select(c => c.indices.ToArray()).Where(c => c.All(i => backIndices.ContainsKey(i))).Transform(i => backIndices[i]);
            var cutPoints = field.Select(i => points[i]).ToArray();

            points = cutPoints;
            convexes = cutConvexes.Select(convex => new Convex { indices = convex.ToList() }).ToList();
            superPoints = null;
        }

        public Polygon FindPolygon(bool inside = false, Vector2? insideClosestPoint = null)
        {
            MakeSuperPoints();

            var insideStartPoint = inside ? (insideClosestPoint ?? points.Center()) : Vector2.Zero;

            var startPoint = inside
                ? superPoints.OrderBy(p => (insideStartPoint - points[p.i]).Len2).First()
                : superPoints.OrderBy(p => points[p.i].X).First();

            int GetNext(int ai, Vector2 a, SuperPoint b)
            {
                Line2 ab = (a, points[b.i]);

                if (inside)
                    return b.neighbors.Where(i => i != ai).OrderBy(i => ab.FnAngleB(points[i])).First();
                else
                    return b.neighbors.Where(i => i != ai).OrderByDescending(i => ab.FnAngleB(points[i])).First();
            }

            List<int> polygon = new List<int>();
            var p = startPoint;
            var previ = -1;
            Vector2 prevp = (points[p.i].X - 1, points[p.i].Y);

            do
            {
                polygon.Add(p.i);
                var nexti = GetNext(previ, prevp, p);
                previ = p.i;
                prevp = points[p.i];
                p = superPoints[nexti];
            } while (p != startPoint);

            return new Polygon
            {
                Points = polygon.Select(i => points[i]).ToArray()
            };
        }

        private IEnumerable<int> EmptyPoints => superPoints.Where(p => p.neighbors.Count == 0).Select(p => p.i);

        private List<int> GetField(IEnumerable<int> fieldPart)
        {
            var field = points.Index().Select(i => new PointInfo { i = i }).ToArray();
            FindField(field, fieldPart.ToList());

            return field.Where(f => f.visited).Select(f => f.i).ToList();
        }

        private void FindField(PointInfo[] points, List<int> visits)
        {
            foreach(var i in visits)
            {
                if (points[i].visited)
                    continue;

                points[i].visited = true;

                FindField(points, superPoints[i].neighbors);
            }
        }

        class PointInfo
        {
            public int i;
            public bool visited;
        }

        class SuperPoint
        {
            public int i;
            public List<int> neighbors = new List<int>();
        }

        public class Convex
        {
            public List<int> indices;
            public IEnumerable<(int i, int j)> edges => indices.CirclePairs();
        }
    }
}
