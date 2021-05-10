using Model.Extensions;
using System;
using System.Linq;

namespace Model.Libraries
{
    public static class Polygons
    {
        public static Polygon Heart(double a, double b, int count) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = Funcs2.Heart(),
                From = 0,
                To = 2 * Math.PI,
                N = count,
                Closed = true
            }.GetPoints().Reverse().ToArray()
        }.Scale((a, b));

        public static Polygon Elipse(double a, double b, int count) => new Polygon
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

        public static Polygon Sinus(double amplitude, double thikness, double n, int count) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = t => (Math.Abs(t), amplitude * ((t < 0 ? -thikness : thikness) + n * Math.Sin(Math.Abs(t)))),
                From = -n * 2 * Math.PI,
                To = n * 2 * Math.PI,
                N = count,
            }.GetPoints().Reverse().ToArray()
        }.Mult(0.9).Move((Math.PI / 4, n * Math.PI)).ScaleToOne((n * 2 * Math.PI, n * 2 * Math.PI)).Move((-0.5, -0.5));

        public static Polygon Polygon5 => new Polygon
        {
            Points = new Vector2[]
            {
                (4, 0), (6.5, 2), (6,5), (2,7), (0.5,5), (0.5, 3.5), (1.5,2.5), (3,3.5),
                (3.5, 3.7), (4,2.5), (4.5, 3.5), (3.5, 4.5), (2.5, 4), (1.5, 3.3), (1,4.2),
                (2.5, 5.5), (5, 4.5), (5.3, 2.2)
            }
        }.ScaleToOne((10, 10)).Move((-0.5, -0.5));

        public static Polygon Spiral(double n, int count) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = t => (-Math.Abs(t) * Math.Sin(t), t * Math.Cos(t)),
                From = n * 2 * Math.PI + Math.PI / 2,
                To = -n * 2 * Math.PI + Math.PI / 2,
                N = count,
            }.GetPoints().Reverse().ToArray()
        }.Mult(0.7).ScaleToOne((n * 4 * Math.PI, n * 4 * Math.PI));

        public static Polygon Polygon4 => new Polygon
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

        public static Polygon Polygon3 => new Polygon
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

        public static Polygon Polygon2 => new Polygon
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

        public static Polygon Polygon1 => new Polygon
        {
            Points = new Vector2[]
            {
                (2, 2),
                (4, 4),
                (6, 7),
                (3, 8)
            }
        }.Scale((10, 10), (1, 1)).Move((-0.5, -0.5));

        public static Polygon Square => new Polygon
        {
            Points = new Vector2[]
            {
                (0, 0),
                (1, 0),
                (1, 1),
                (0, 1)
            }
        }.Move((-0.5, -0.5));
    }
}
