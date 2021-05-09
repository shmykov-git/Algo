using Model.Extensions;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class SuperShape2
    {
        public Vector2[] points;
        public List<Convex> convexes;

        private SuperPoint[] superPoints;
        private readonly Shape2 shape;

        public SuperShape2(Shape2 shape)
        {
            this.shape = shape;
            Init(shape);
        }

        private void Init(Shape2 shape)
        {
            points = shape.Points;
            convexes = shape.Convexes.Select(convex => new Convex { indices = convex.ToList() }).ToList();
        }

        private void MakeSuperPoints()
        {
            superPoints = points.Index().Select(i => new SuperPoint { i = i }).ToArray();
            foreach (var convex in convexes)
                foreach (var edge in convex.edges)
                {
                    superPoints[edge.i].neighbors.Add(edge.j);
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

            foreach(var info in gInfos)
                convexes.Remove(info.cutconvex);

            MakeSuperPoints();

            var rights = gInfos.SelectMany(g => g.rights).Distinct();
            var lefts = gInfos.SelectMany(g => g.lefts).Distinct();
            var pure = inside ? lefts.Except(rights).ToList() : rights.Except(lefts).ToList();

            var field = GetField(pure);

            foreach (var i in EmptyPoints)
                field.Remove(i);

            var newShape = this.ToShape().Cut(field);
            Init(newShape);


            //if (info.cutconvex.indices.Count <= info.cutedges.Length+2)
            //{
            //    convexes.Remove(info.cutconvex);
            //}
            ////else
            //{
            //    if (info.cutedges.Length == 1)
            //    {
            //        info.cutconvex.indices = info.cutconvex.indices.Where(i => info.cutedges[0].i != i && info.cutedges[0].j != i).ToList();
            //    }
            //    else
            //    {
            //        List<List<int>> newConvexes = new List<List<int>>();
            //        List<int> newConvex = new List<int>();
            //        foreach (var edge in info.cutconvex.edges)
            //        {
            //            if (info.cutedges.Contains(edge))
            //            {
            //                if (newConvex != null)
            //                {
            //                    newConvexes.Add(newConvex);
            //                    newConvex = null;
            //                }
            //            }
            //            else
            //            {
            //                if (newConvex == null)
            //                    newConvex = new List<int>();

            //                newConvex.Add(edge.i);
            //            }
            //        }
            //        if (newConvexes.Count > 2)
            //        {
            //            newConvexes.First().AddRange(newConvexes.Last());
            //            newConvexes.Remove(newConvexes.Last());
            //        }

            //        convexes.Remove(info.cutconvex);
            //        convexes.AddRange(newConvexes.Select(c => new Convex { indices = c }));
            //    }
            //}

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
