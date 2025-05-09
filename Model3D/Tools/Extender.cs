using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Model.Graphs;
using Vector2 = Model.Vector2;

namespace Model3D.Tools
{
    public static class Extender
    {
        public static Shape SplitSphere(Shape shape, double deformation = 1.5)
        {
            var r = shape.GetRadius();
            var shapePoints = shape.Points3.Select(p => p / r).ToArray();
            var newPoints = shape.Convexes.Select(convex => convex.Select(i => shapePoints[i]).Center().Normalize()).ToArray();

            var points = shapePoints.Concat(newPoints).ToArray();
            var indPoints = points.IndexValue().ToArray();
            var net = new Net<Vector2, (int index, Vector3 value)>(indPoints.Select(p => (p.value.ToV2(), p)), 3 * r);
            var distance = deformation * indPoints.SelectMany(a => net.SelectNeighbors(a.value.ToV2()).Where(b=>a.index != b.index).Select(b => (b.value - a.value).Length)).Min();

            // todo: triples
            List<(int index, Vector3 value)> OrderEdges((int index, Vector3 value) a, (int index, Vector3 value)[] ps)
            {
                List<(int index, Vector3 value)> res = new();
                res.Add(ps[0]);

                for (var i = 1; i < ps.Length; i++)
                {
                    for (var j = i; j < ps.Length; j++)
                    {
                        if ((ps[i-1].value - ps[j].value).Length < distance) //  && IsRight(a.value, ps[gi - 1].value, ps[j].value)
                        {
                            var t = ps[i];
                            ps[i] = ps[j];
                            ps[j] = t;
                            
                            break;
                        }
                    }
                    res.Add(ps[i]);
                }

                //Debug.WriteLine(string.Join(", ", res.SelectCirclePair((a,b)=>(b.value-a.value).Length.ToString("F1"))));

                return res;
            }

            var convexes = indPoints.SelectMany(a => OrderEdges(a, net.SelectNeighbors(a.value.ToV2()).Where(b => a.index != b.index && (b.value - a.value).Length < distance).ToArray()).SelectCirclePair((b, c) => new[] { a.index, b.index, c.index })).ToArray();
            var di = convexes.Select(c => c.OrderBy(v => v).ToArray()).Select(c => (c[0], c[1], c[2])).ToArray().DistinctOnlyBi();
            convexes = convexes.IndexValue().Where(v => di.filter[v.index]).Select(v => v.value).Select(c=>IsRight(points[c[0]], points[c[1]], points[c[2]]) ? c : new[] { c[1], c[0], c[2] }).ToArray();

            return new Shape 
            {
                Points3 = points,
                Convexes = convexes
            };
        }

        public static Shape JoinConvexesBy6(Shape shape)
        {
            var g = new Graph(shape.OrderedEdges);
            var excluded = new HashSet<Graph.Node>(g.nodes.Count);
            var nonExcluded = g.Visit().Where(n => n.edges.Count == 5).SelectMany(n=>n.edges.Select(e=>e.Another(n))).ToHashSet();
            foreach(var node in g.PathVisit())
            {
                if ((node.edges.Count == 6 || node.edges.Count == 5) && !nonExcluded.Contains(node) && node.edges.All(e=>!excluded.Contains(e.Another(node))))
                {
                    excluded.Add(node);
                }
            }

            int[] JoinConvexes(int[][] convexes, int k)
            {
                var res = new List<int>();
                var edges = convexes.Select(c => c.Where(i => i != k).ToArray()).ToList();
                var j = edges[0][0];
                do
                {
                    var e = edges.First(e => e[0] == j || e[1] == j);
                    res.Add(j);
                    j = e[0] == j ? e[1] : e[0];
                    edges.Remove(e);
                } while (edges.Count > 0);

                return res.ToArray();
            }

            var joinedConvexes = new List<int[]>();
            var points = shape.Points3;
            var convexes = shape.Convexes.ToList();
            foreach(var node in excluded)
            {
                var planeConvexes = convexes.Where(c => c.Any(i => node.i == i)).ToArray();
                
                foreach (var c in planeConvexes)
                    convexes.Remove(c);

                var joinedConvex = JoinConvexes(planeConvexes, node.i);
                joinedConvexes.Add(joinedConvex);
            }

            var (bi, pointIndices) = points.Index().RemoveBi(excluded.Select(n => n.i));
            var newPoints = pointIndices.Select(i => points[i]).ToArray();
            var newConvexes = convexes.Concat(joinedConvexes).ApplyBi(bi);
            var rightConvexes = newConvexes.Select(c=>IsRight(newPoints[c[0]], newPoints[c[1]], newPoints[c[2]]) ? c : c.Reverse().ToArray()).ToArray();
            
            var resultShape = new Shape
            {
                Points3 = newPoints,
                Convexes = rightConvexes
            };

            return resultShape;
        }

        public static Shape SplitConvexes(Shape shape)
        {
            var points = shape.Points3;
            var n = points.Length;
            var newPoints = shape.Convexes.Select(convex => convex.Select(i => points[i]).Center()).ToArray();
            var newConvexes = shape.Convexes.Index().SelectMany(k => shape.Convexes[k].SelectCirclePair((i,j) => new int[] { i, j, n + k })).ToArray();
            var newShape = new Shape
            {
                Points3 = points.Concat(newPoints).ToArray(),
                Convexes = newConvexes
            };

            var transformed = ShapeExtensions.Transform(newShape, TransformFuncs3.PullOnSphere);
            var ps = transformed.Points3;
            var joinedConvexes = transformed.Convexes.GroupBy(convex => new Plane(ps[convex[0]], ps[convex[1]], ps[convex[2]]).NOne).ToArray();
            var agrConvexes = joinedConvexes.Where(g => g.Count() > 1 && g.First().Intersect(g.Last()).Count() > 1).ToArray();
            var otherConvexes = joinedConvexes.Except(agrConvexes);
            var resConvexes = agrConvexes.Select(g => g.Aggregate((a, b) => a.JoinConvexes(b))).Concat(otherConvexes.SelectMany(g=>g)).ToArray();

            return new Shape
            {
                Points = transformed.Points,
                Convexes = resConvexes
            };
        }
       
        public static Shape[] SplitConvexesByMaterial(Shape shape)
        {
            if (shape.Materials == null)
                return new[] { shape };

            var defaultMaterial = new Material();

            var shapes = new Dictionary<Material, ShapeItem>();

            for (var i = 0; i < shape.Convexes.Length; i++)
            {
                var convex = shape.Convexes[i];
                var material = shape.Materials[i] ?? defaultMaterial;

                if (!shapes.TryGetValue(material, out ShapeItem shapeItem))
                {
                    shapeItem = new ShapeItem();
                    shapes.Add(material, shapeItem);
                }

                var itemConvexes = convex.Select(i => shapeItem.Shifts.GetOrAdd(i, _ =>
                  {
                      shapeItem.Points.Add(shape.Points[i]);

                      return shapeItem.Points.Count - 1;
                  })).ToArray();

                shapeItem.Convexes.Add(itemConvexes);
                shapeItem.Materials.Add(material == defaultMaterial ? null : material);
            }

            return shapes.Values.Select(s=>s.ToShape()).ToArray();
        }

        class ShapeItem
        {
            public List<Vector4> Points = new List<Vector4>();
            public ConcurrentDictionary<int, int> Shifts = new ConcurrentDictionary<int, int>();
            public List<int[]> Convexes = new List<int[]>();
            public List<Material> Materials = new List<Material>();

            public Shape ToShape() => new Shape
            {
                Points = Points.ToArray(),
                Convexes = Convexes.ToArray(),
                Materials = Materials.Count == 0 ? null : Materials.ToArray()
            };
        }

        private static bool IsRight(Vector3 a, Vector3 b, Vector3 c)
        {
            var ba = a - b;
            var bc = c - b;

            return b.MultS(ba.MultV(bc)) < 0;
        }
    }
}
