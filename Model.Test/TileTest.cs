using Model.Extensions;
using Model.Libraries;
using NUnit.Framework;
using System;
using System.Linq;

namespace Model.Test
{
    public class TileTest
    {
        private double PentagonalCheck(double angleB, double angleD)
        {
            var angleC = 2 * Math.PI - 2 * angleB;
            var angleE = Math.PI - angleD / 2;

            var exteriorAngles = new[] { 0, Math.PI - angleB, Math.PI - angleC, Math.PI - angleD, Math.PI - angleE, Math.PI - angleD };

            double GetExteriorAngle(int k) => (k + 1).SelectRange(i => exteriorAngles[i]).Sum();
            Vector2 GetPoint(int k) => (k).SelectRange(i => new Vector2(Math.Cos(GetExteriorAngle(i)), Math.Sin(GetExteriorAngle(i)))).Sum();

            var points = exteriorAngles.Index().Select(GetPoint).ToArray();
            var convex = exteriorAngles.Index().ToArray();

            var mainShape = new Shape2
            {
                Points = points,
                Convexes = new int[][] { convex }
            };

            var l = new Line2(mainShape[4], mainShape[5]);

            return l.Fn((0, 0)).Abs();
        }

        [Test]
        public void FnTest()
        {
            Func<double, double> fn = x => PentagonalCheck(x, Math.PI / 2);

            var x = 2.0;
            var dx = 0.1;
            while(fn(x) > 0.000000000000001)
            {
                var y1 = fn(x);
                var y2 = fn(x + dx);
                var y3 = fn(x - dx);

                if (y1 < y2)
                    if (y1 < y3)
                        dx = dx / 2;
                    else
                        x = x - dx;
                else
                    x = x + dx;
            }

            var res = x; // 2.1850867345871086
        }
    }
}