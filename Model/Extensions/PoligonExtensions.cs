using System;
using System.Linq;

namespace Model.Extensions
{
    public static class PoligonExtensions
    {
        public static Poligon PutInside(this Poligon poligon, Poligon insidePoligon)
        {
            var points = poligon.Points;
            var insidePoints = insidePoligon.Points.Reverse().ToArray();

            var indexes = points.Index();
            var insideIndexes = insidePoints.Index();

            var (minI, minJ) = indexes.SelectMany(i => insideIndexes.Select(j => new
            {
                Pair = (i, j),
                Len2 = (points[i] - insidePoints[j]).Len2
            })).OrderBy(v => v.Len2).First().Pair;

            return new Poligon
            {
                Points = points.Take(minI + 1)
                .Concat(insidePoints.Skip(minJ))
                .Concat(insidePoints.Take(minJ + 1))
                .Concat(points.Skip(minI))
                .ToArray()
            };
        }

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

        public static Poligon MultOne(this Poligon poligon, double k)
        {
            return poligon.Transform(p => (p - Size.HalfOne) * k + Size.HalfOne);
        }

        public static Poligon MirrorY(this Poligon poligon, Size s)
        {
            return poligon.Transform(p => (p.X, s.Height - p.Y));
        }
    }
}
