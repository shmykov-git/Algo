using Model;
using Model.Extensions;

namespace Model3D.Extensions
{
    public static class PolygonExtensions
    {
        public static Shape MakeShape(this Polygon polygon, double? triangulationLen = null)
        {
            var shape2 = polygon.Fill();
            if (triangulationLen.HasValue)
                shape2 = shape2.Triangulate(triangulationLen.Value);

            return shape2.ToShape3();
        }
    }
}
