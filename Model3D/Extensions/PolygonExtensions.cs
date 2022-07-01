using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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

        public static Shape ToShape(this Polygon polygon, double? volume = null, bool triangulate = false, double incorrectFix = 0, bool trioStrategy = false)
        {
            if (!volume.HasValue && !triangulate)
                return new Shape
                {
                    Points2 = polygon.Points,
                    Convexes = new[] { polygon.Points.Index().ToArray() }
                };

            int[][] trConvexes;
            if (trioStrategy)
            {
                var convexes = FillEngine.FindConvexes(polygon);
                trConvexes = FillEngine.Triangulate(polygon.Points, convexes);
            }
            else
            {
                trConvexes = Triangulator.Triangulate(polygon, incorrectFix);
            }

            var shape = new Shape()
                {
                    Points2 = polygon.Points,
                    Convexes = trConvexes
                }.Normalize();

            if (!volume.HasValue)
                return shape;

            // todo: сделать нормально функцию добавляения объема по Z - по аналогии с этой (сдвиги только по edges)
            return shape.AddNormalVolume(volume.Value).MoveZ(-volume.Value / 2);
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

        public static Polygon ComposeOthersToFirst(this Polygon[] polygons)
        {
            var internals = (polygons.Length).Range().Skip(1).Select(i => (0, i)).ToArray();

            return polygons.Compose(internals).First();
        }

       
        public static Polygon[] Compose(this Polygon[] polygons, (int main, int child)[] map, bool skipReverse = false)
        {
            var getLevel = map.GetMapLevelFn();

            var excepts = map.Where(v=>getLevel(v.child).Even()).Select(v => v.child).ToHashSet();
            var includes = map.GroupBy(v => v.main).ToDictionary(gv => gv.Key, gv => gv.Select(v => v.child).ToArray());

            return polygons.Select((p, num) => (p, num))
                .Where(v => !excepts.Contains(v.num))
                .Select(v =>
                {
                    var mainP = v.p;

                    if (includes.TryGetValue(v.num, out int[] takeList))
                        takeList.ForEach(i => mainP = mainP.PutInside(polygons[i], skipReverse));

                    return mainP;
                })
                .ToArray();
        }

        public static Polygon[] ComposeObsolet(this Polygon[] polygons, (int takeI, int incJ)[] internals)
            => polygons.Compose(internals.Select(v => v.Reverse()).ToArray());
    }
}
