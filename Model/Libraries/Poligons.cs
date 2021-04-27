using Model.Extensions;
using System;
using System.Linq;

namespace Model.Libraries
{
    public static class Poligons
    {
        public static Poligon Elipse(double a, double b, int count) => new Poligon
        {
            Points = new Func2Info
            {
                Fn = t => (a * Math.Sin(t), b * Math.Cos(t)),
                From = 0,
                To = 2 * Math.PI,
                N = count,
                Closed = true
            }.GetPoints().Reverse().ToArray()
        }.Mult(0.9 * 0.5 / Math.Max(a, b));

        public static Poligon Sinus(double amplitude, double thikness, double n, int count) => new Poligon
        {
            Points = new Func2Info
            {
                Fn = t => (Math.Abs(t), amplitude * ((t < 0 ? -thikness : thikness) + n * Math.Sin(Math.Abs(t)))),
                From = -n * 2 * Math.PI,
                To = n * 2 * Math.PI,
                N = count,
            }.GetPoints().Reverse().ToArray()
        }.Mult(0.9).Move((Math.PI / 4, n * Math.PI)).ScaleToOne((n * 2 * Math.PI, n * 2 * Math.PI)).Move((-0.5, -0.5));

        public static Poligon Poligon5 => new Poligon
        {
            Points = new Vector2[]
            {
                (4, 0), (6.5, 2), (6,5), (2,7), (0.5,5), (0.5, 3.5), (1.5,2.5), (3,3.5),
                (3.5, 3.7), (4,2.5), (4.5, 3.5), (3.5, 4.5), (2.5, 4), (1.5, 3.3), (1,4.2),
                (2.5, 5.5), (5, 4.5), (5.3, 2.2)
            }
        }.ScaleToOne((10, 10)).Move((-0.5, -0.5));

        public static Poligon Spiral(double n, int count) => new Poligon
        {
            Points = new Func2Info
            {
                Fn = t => (-Math.Abs(t) * Math.Sin(t), t * Math.Cos(t)),
                From = n * 2 * Math.PI + Math.PI / 2,
                To = -n * 2 * Math.PI + Math.PI / 2,
                N = count,
            }.GetPoints().Reverse().ToArray()
        }.Mult(0.7).ScaleToOne((n * 4 * Math.PI, n * 4 * Math.PI));

        public static Poligon Poligon4 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 3.5),
                (4.5, 4),
                (6, 7),
                (3, 8),
                (2.5, 5.5),
                (3.5, 4.5),
                (5, 6),
            }
        }.Scale((10, 10), (1, 1)).Move((-0.5, -0.5));

        public static Poligon Poligon3 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8),
                (3, 5),
                (5, 6),
            }
        }.Scale((10, 10), (1, 1)).Move((-0.5, -0.5));

        public static Poligon Poligon2 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8),
                (3, 5),
            }
        }.Scale((10, 10), (1, 1)).Move((-0.5, -0.5));

        public static Poligon Poligon1 => new Poligon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8)
            }
        }.Scale((10, 10), (1, 1)).Move((-0.5, -0.5));

        public static Poligon Square(double size) => new Poligon
        {
            Points = new Vector2[]
            {
                (0, 0),
                (1, 0),
                (1, 1),
                (0, 1)
            }
        }.Move((-0.5, -0.5)).Mult(size);
    }
}
