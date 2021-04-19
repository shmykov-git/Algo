using System;
using System.Linq;

namespace Model.Extensions
{
    public static class PoligonExtensions
    {
        public static Poligon Transform(this Poligon poligon, Func<Point, Point> transformFn)
        {
            return new Poligon
            {
                Points = poligon.Points.Select(transformFn).ToArray()
            };
        }

        public static Poligon Scale(this Poligon poligon, Size bSize)
        {
            return poligon.Scale(Size.One, bSize);
        }

        public static Poligon Scale(this Poligon poligon, Size aSize, Size bSize)
        {
            return poligon.Transform(p => p.Scale(aSize, bSize));
        }

        public static Poligon MirrorY(this Poligon poligon, Size s)
        {
            return poligon.Transform(p => (p.X, s.Height - p.Y));
        }
    }
}
