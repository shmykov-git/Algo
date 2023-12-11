using Model.Extensions;
using System;
using System.Linq;
using Model.Fourier;

namespace Model.Libraries
{
    public static class Polygons
    {
        //public static Polygon FourierSeries(int count, params Fr[] members) =>
        //    FourierSeries(count, members.Select(m => ((m.r, m.im), m.n + m.dn)).ToArray());

        public static Polygon FourierSeries3(int an, int bn, double a, double b, int count, double da, double db) =>
            FourierSeries(count, (an + da, a), (bn + db, b), (-1, 1));

        public static Polygon FourierSeries5(int an, int bn, int cn, int dn, double a, double b, double c, double d,
            int count, double da = 0, double db = 0, double dc = 0, double dd = 0) =>
            FourierSeries(count, (an + da, a), (bn + db, b), (cn + dc, c), (dn + dd, d), (-1, 1));

        //public static Polygon FourierSeries(int count, params (double r, double k)[] args) =>
        //    FourierSeries(count, args.Select(a => ((a.r, 0d), a.k)).ToArray());

        //public static Polygon FourierSeries(int count, params ((double, double) c, double k)[] args) => new Polygon
        //{
        //    Points = new Func2Info
        //    {
        //        Fn = t => Fourier.Exp(t, args),
        //        From = 0,
        //        To = 1,
        //        N = count,
        //        Closed = true
        //    }.GetPoints().Reverse().ToArray()
        //};

        public static Polygon FourierSeries(int count, params Fr[] members) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = t => Fourier.Exp(t, members),
                From = 0,
                To = 1,
                N = count,
                Closed = true
            }.GetPoints().Reverse().ToArray()
        };

        public static Polygon FourierSeries(int count, Func2 fourierFn) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = fourierFn,
                From = 0,
                To = 1,
                N = count,
                Closed = true
            }.GetPoints().Reverse().ToArray()
        };

        public static Polygon Flower(double a, int n, int count) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = Funcs2.Flower(n, a),
                From = -Math.PI,
                To = Math.PI,
                N = count,
                Closed = true
            }.GetPoints().Reverse().ToArray()
        };

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

        public static Polygon Circle(double r, int count) => new Polygon
        {
            Points = new Func2Info
            {
                Fn = t => (r * Math.Sin(t), r * Math.Cos(t)),
                From = 0,
                To = 2 * Math.PI,
                N = count,
                Closed = true
            }.GetPoints().Reverse().ToArray()
        };

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
