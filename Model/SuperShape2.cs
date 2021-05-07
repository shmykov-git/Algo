using Model.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Model
{
    public class SuperShape2
    {
        public Vector2[] points;
        public List<Convex> convexes;

        public SuperShape2(Shape2 shape)
        {
            points = shape.Points;
            convexes = shape.Convexes.Select(convex => new Convex { indices = convex.ToList() }).ToList();
        }

        public void Cut(Polygon polygon)
        {
            var sections = polygon.Lines.ToArray();
            var cutConvexInfos = convexes.SelectMany(convex =>
                sections.SelectMany(section =>
                    convex.edges.Where(e => section.IsSectionIntersectedBy((points[e.i], points[e.j])))
                        .Select(e => new
                        {
                            edge = e,
                            convex = convex
                        })));

            var gInfos = cutConvexInfos.GroupBy(c => c.convex).Select(gc => new { cutconvex = gc.Key, cutedges = gc.Select(c => c.edge).Distinct().ToArray() }).ToArray();

            foreach(var info in gInfos)
            {
                //if (info.cutconvex.indices.Count <= info.cutedges.Length+2)
                {
                    convexes.Remove(info.cutconvex);
                }
                //else
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
        }

        public class Convex
        {
            public List<int> indices;
            public IEnumerable<(int i, int j)> edges => indices.CirclePairs();
        }
    }
}
