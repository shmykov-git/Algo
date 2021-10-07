using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vector2 = Model.Vector2;

namespace Model3D.Tools
{
    public static class Extender
    {
        public static Shape SplitSphere(Shape shape, double deformation = 1.2)
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
                        if ((ps[i-1].value - ps[j].value).Length < distance) //  && IsRight(a.value, ps[i - 1].value, ps[j].value)
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

            bool IsRight(Vector3 a, Vector3 b, Vector3 c)
            {
                var ba = a - b;
                var bc = c - b;

                return b.MultS(ba.MultV(bc)) < 0;
            }

            var convexes = indPoints.SelectMany(a => OrderEdges(a, net.SelectNeighbors(a.value.ToV2()).Where(b => a.index != b.index && (b.value - a.value).Length < distance).ToArray()).SelectCirclePair((b, c) => new[] { a.index, b.index, c.index })).ToArray();
            var di = convexes.Select(c => c.OrderBy(v => v).ToArray()).Select(c => (c[0], c[1], c[2])).ToArray().DistinctIndices();
            convexes = convexes.IndexValue().Where(v => di.filter[v.index]).Select(v => v.value).Select(c=>IsRight(points[c[0]], points[c[1]], points[c[2]]) ? c : new[] { c[1], c[0], c[2] }).ToArray();

            return new Shape 
            {
                Points3 = points,
                Convexes = convexes
            };
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

            var transformed = ShapeExtensions.Transform(newShape, TransformFuncs3.Sphere);
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
    }
}
