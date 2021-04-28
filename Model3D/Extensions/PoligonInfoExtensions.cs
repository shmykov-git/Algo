using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using System.Linq;

namespace Model3D.Extensions
{

    public static class PoligonExtensions
    {
        public static Shape MakeShape(this Poligon poligon)
        {
            return poligon.Fill().ToShape();
        }
    }

    public static class PoligonInfoExtensions
    {
        public static Shape ToShape(this PoligonInfo poligonInfo)
        {
            return new Shape
            {
                Points = poligonInfo.Poligon.Points.Select(p => new Vector4(p.X, p.Y, 0, 1)).ToArray(),
                Convexes = poligonInfo.Convexes
            };
        }
    }
}
