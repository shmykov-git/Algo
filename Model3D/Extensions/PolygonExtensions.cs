using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Tools;

namespace Model3D.Extensions
{
    public static class PolygonExtensions
    {
        public static Shape MakeShape(this Polygon polygon, bool triangulate = false)
        {
            return polygon.Fill(triangulate).ToShape3();
        }

        public static Shape ToShape(this Polygon polygon, bool withVolume = false) =>
            ToShape(polygon, withVolume ? 0.1 : null);

        public static Shape ToShape(this Polygon polygon, double? volume = null)
        {
            if (!volume.HasValue)
                return new Shape
                {
                    Points2 = polygon.Points,
                    Convexes = new[] { polygon.Points.Index().ToArray() }
                };

            var convexes = FillEngine.FindConvexes(polygon);
            var halfVolume = new Vector3(0, 0, volume.Value / 2);
            var trConvexes = FillEngine.Triangulate(polygon.Points, convexes);

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
    }
}
