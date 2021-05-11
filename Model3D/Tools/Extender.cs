using Model;
using Model.Extensions;
using Model3D.Extensions;
using Model3D.Libraries;
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
    }
}
