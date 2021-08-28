using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Model3D.Tools
{
    public static class Extender
    {
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
