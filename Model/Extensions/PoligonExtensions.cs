using System;
using System.Linq;

namespace Model.Extensions
{
    public static class PoligonExtensions
    {
        public static Poligon Transform(this Poligon poligon, Func<Vector2, Vector2> transformFn)
        {
            return new Poligon
            {
                Points = poligon.Points.Select(transformFn).ToArray()
            };
        }

        public static Poligon Move(this Poligon poligon, Size size)
        {
            return poligon.Transform(p => p + size);
        }

        public static Poligon Scale(this Poligon poligon, Size bSize)
        {
            return poligon.Scale(Size.One, bSize);
        }

        public static Poligon ScaleToOne(this Poligon poligon, Size aSize)
        {
            return poligon.Scale(aSize, Size.One);
        }

        public static Poligon Scale(this Poligon poligon, Size aSize, Size bSize)
        {
            return poligon.Transform(p => p.Scale(aSize, bSize));
        }

        public static Poligon Mult(this Poligon poligon, double k)
        {
            return poligon.Transform(p => p * k);
        }

        public static Poligon MirrorY(this Poligon poligon, Size s)
        {
            return poligon.Transform(p => (p.X, s.Height - p.Y));
        }
    }
}
