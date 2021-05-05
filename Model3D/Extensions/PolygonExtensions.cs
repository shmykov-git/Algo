using Model;
using Model.Extensions;
using Model.Tools;

namespace Model3D.Extensions
{
    public static class PolygonExtensions
    {
        //public static Shape MakeTriangulatedShape(this Polygon polygon, double edgeLen = 0.1)
        //{
        //    return Triangulator.Triangulate(polygon, edgeLen).ToShape3();
        //}

        public static Shape MakeShape(this Polygon polygon, bool triangulate = false)
        {
            return polygon.Fill(triangulate).ToShape3();
        }
    }
}
