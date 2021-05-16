using Aspose.ThreeD.Utilities;
using Model;
using Model3D.Libraries;
using System.Linq;

namespace Model3D.Extensions
{
    public static class Shape2Extensions
    {
        public static Shape ToShape3(this Shape2 shape)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(p.x, p.y, 0, 1)).ToArray(),
                Convexes = shape.Convexes
            };
        }

        public static Shape PullOnSurface(this Shape2 shape, SurfaceFunc fn) => new Shape
        {
            Points3 = shape.Points.Select(p => fn(p.x, p.y)).ToArray(),
            Convexes = shape.Convexes
        };

        public static Shape PullOnSurface90(this Shape2 shape, SurfaceFunc fn) => new Shape
        {
            Points3 = shape.Points.Select(p => fn(p.y, p.x)).ToArray(),
            Convexes = shape.Convexes
        };
    }
}
