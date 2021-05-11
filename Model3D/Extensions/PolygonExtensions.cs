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
    }
}
