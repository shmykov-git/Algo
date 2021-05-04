using Aspose.ThreeD.Utilities;
using Model;
using System.Linq;

namespace Model3D.Extensions
{
    public static class Shape2Extensions
    {
        public static Shape ToShape3(this Shape2 shape)
        {
            return new Shape
            {
                Points = shape.Points.Select(p => new Vector4(p.X, p.Y, 0, 1)).ToArray(),
                Convexes = shape.Convexes
            };
        }
    }
}
