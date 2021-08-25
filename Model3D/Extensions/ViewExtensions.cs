using Model;
using System.Drawing;

namespace Model3D.Extensions
{
    public static class ShapeViewExtensions
    {
        public static ShapeView ToView(this Shape shape, Color color) => new ShapeView
        {
            Shape = shape,
            Color = color
        };
    }
}
