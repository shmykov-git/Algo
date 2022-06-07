using System.Collections.Generic;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Vector2 = Model.Vector2;

namespace Model3D.Extensions
{
    public static class PolygonExtensions
    {
        public static Shape MakeShape(this Polygon polygon, bool triangulate = false)
        {
            return polygon.Fill(triangulate).ToShape3();
        }

        public static Shape ToShape(this Polygon polygon, double? volume = null, bool triangulate = false, double incorrectFix = 0)
        {
            if (!volume.HasValue && !triangulate)
                return new Shape
                {
                    Points2 = polygon.Points,
                    Convexes = new[] { polygon.Points.Index().ToArray() }
                };

            //var convexes = FillEngine.FindConvexes(polygon);
            //throw new DebugException<(Shape p, Shape t)>((polygon.ToShape(), new Shape()
            //{
            //    Points2 = polygon.Points,
            //    Convexes = convexes
            //}));
            //var trConvexes = FillEngine.Triangulate(polygon.Points, convexes);

            var trConvexes = Triangulator.Triangulate(polygon, incorrectFix);
            //throw new DebugException<(Shape p, Shape t)>((polygon.ToShape(), new Shape()
            //{
            //    Points2 = polygon.Points,
            //    Convexes = trConvexes
            //}));

            if (!volume.HasValue)
                return new Shape()
                {
                    Points2 = polygon.Points,
                    Convexes = trConvexes
                };

            var halfVolume = new Vector3(0, 0, volume.Value / 2);

            return new Shape
            {
                Points3 = polygon.Points.Select(p => p.ToV3() - halfVolume)
                    .Concat(polygon.Points.Select(p => p.ToV3() + halfVolume)).ToArray(),

                Convexes = trConvexes.Select(c => c.Reverse().ToArray())
                    .Concat(trConvexes.Transform(v => v + polygon.Points.Length))
                    .Concat(polygon.Points.Index().Reverse().SelectCirclePair((i, j) => new int[] { i, i + polygon.Points.Length, j + polygon.Points.Length, j }).ToArray())
                    .ToArray()
            };
        }

        public static Shape ToTriangulatedShape(this Polygon polygon, int countTriangle = 30, double? volume = null, double incorrectFix = 0)
        {
            var border = polygon.Border;
            var size = border.b - border.a;
            var center = 0.5 * (border.a + border.b);

            var n = countTriangle;
            var m = (int) (n * size.y / (size.x * 3d.Sqrt()));
            // что с масштабом?
            var net = Shapes.PlaneByTriangles(m, n).Mult(1.3*size.x).Move(center.ToV3());
            var cutNet = net.Cut(polygon);
            var cutPoints = cutNet.Points2;

            var perimeters = cutNet.GetPerimeters();
            var perimeterPolygons = perimeters.Select(p => p.Select(i => cutPoints[i]).ToArray().ToPolygon()).ToArray();

            var borderPolygon = perimeterPolygons.Aggregate(polygon, JoinInside);
            var triangulatedBorder = borderPolygon.ToShape(triangulate: true, incorrectFix: incorrectFix);

            var triangulatedShape = (triangulatedBorder + cutNet).Normalize();

            if (volume == null)
                return triangulatedShape;

            var halfVolume = new Vector3(0, 0, volume.Value / 2);
            var nn = triangulatedShape.Points.Length;

            return new Shape
            {
                Points3 = new []
                {
                    triangulatedShape.Points.Select(p => p.ToV3() - halfVolume),
                    triangulatedShape.Points.Select(p => p.ToV3() + halfVolume)
                }.ManyToArray(),
                    
                Convexes = new []
                {
                    triangulatedShape.Convexes.Select(c => c.Reverse().ToArray()),
                    triangulatedShape.Convexes.Transform(v => v + nn),
                    polygon.Points.Index().Reverse().SelectCirclePair((i, j) => new[] { i, i + nn, j + nn, j }).ToArray()
                }.ManyToArray()
            };
        }

        public static Polygon JoinInside(this Polygon polygon, Polygon joinPolygon)
        {
            var (i, j, _) = (polygon.Points.Length, joinPolygon.Points.Length)
                .SelectRange((i, j) => (i, j, len2: (polygon[i] - joinPolygon[j]).Len2))
                .OrderBy(v => v.len2).First();

            var points = new[]
            {
                polygon.Points[..(i+1)],
                joinPolygon.Points[..(j+1)].Reverse(),
                joinPolygon.Points[j..].Reverse(),
                polygon.Points[i..]
            }.SelectMany(v => v).ToArray();

            return new Polygon() {Points = points};
        }
    }
}
