using Model.Extensions;
using Model.Tools;
using System;
using System.Linq;

namespace Model.Libraries
{
    public static class Tiles
    {
        private static readonly double h3 = 3.0.Sqrt() / 2;

        public static Tile Triangles => new Tile
        {
            ShiftX = new Vector2[] { (1, 0) },
            ShiftY = new Vector2[] { (0, 2 * h3) },
            Points = new Vector2[]
            {
                (0, 0), (1, 0),
                (0.5, h3), (1.5, h3),
                (0, 2*h3), (1, 2*h3)
            },
            Convexes = new int[][]
            {
                new int[] {0, 1, 2},
                new int[] {1, 3, 2},
                new int[] {2, 5, 4},
                new int[] {2, 3, 5}
            }
        };

        public static Tile Triangles2 => new Tile
        {
            ShiftX = new Vector2[] { (1, 0) },
            ShiftY = new Vector2[] { (0.5, h3), (-0.5, h3) },
            Points = new Vector2[]
            {
                (0, 0), (1, 0),
                (0.5, h3), (1.5, h3)
            },
            Convexes = new int[][]
            {
                new int[] {0, 1, 2},
                new int[] {1, 3, 2}
            }
        };

        public static Tile Squares => new Tile
        {
            ShiftX = new Vector2[] { (1, 0) },
            ShiftY = new Vector2[] { (0, 1) },
            Points = new Vector2[]
            {
                (0, 0), (1, 0),
                (1, 1), (0, 1)
            },
            Convexes = new int[][]
            {
                new int[] {0, 1, 2, 3},
            }
        };

        public static Tile Hexagon => new Tile
        {
            ShiftX = new Vector2[] { (1.5, h3), (1.5, -h3) },
            ShiftY = new Vector2[] { (0, 2 * h3) },
            Points = new Vector2[]
            {
                (1, 0), (0.5, h3),
                (-0.5, h3), (-1, 0),
                (-0.5, -h3), (0.5, -h3)
            },
            Convexes = new int[][]
            {
                new int[] {0, 1, 2, 3, 4, 5},
            }
        };

        private static double PentagonalKershner8MinimizeFn(double angleB, double angleD)
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

        public static Tile PentagonalKershner8(double angleD)
        {
            var angleB = SimpleMinimizer.Minimize(2, 0.1, 0.00000000000001, x => PentagonalKershner8MinimizeFn(x, angleD));
            var angleD0 = Math.PI / 2;

            var angleC = 2 * Math.PI - 2 * angleB;
            var angleE = Math.PI - angleD / 2;

            var exteriorAngles = new[] { 0, Math.PI - angleB, Math.PI - angleC, Math.PI - angleD, Math.PI - angleE };

            double GetExteriorAngle(int k) => (k+1).SelectRange(i => exteriorAngles[i]).Sum();
            Vector2 GetPoint(int k) => (k).SelectRange(i => new Vector2(Math.Cos(GetExteriorAngle(i)), Math.Sin(GetExteriorAngle(i)))).Sum();

            var points = exteriorAngles.Index().Select(GetPoint).ToArray();
            var convex = exteriorAngles.Index().ToArray();

            var mainShape = new Shape2
            {
                Points = points,
                Convexes = new int[][] { convex }
            };

            var rightBottom = mainShape;
            var leftBottom = rightBottom.Mirror((points[4], points[0])); // reversed

            var leftUp = mainShape.Rotate(Math.PI);
            leftUp = leftUp.Move(rightBottom[4] - leftUp[3]);
            var rightUp = leftUp.Mirror((leftUp[0], leftUp[4])); // reversed

            var mainBigShape = rightBottom.Join(leftBottom).Join(leftUp).Join(rightUp);

            var bigLeftBottom = mainBigShape;
            var bigRightBottom = mainBigShape.Rotate(GetExteriorAngle(1));
            bigRightBottom = bigRightBottom.Move(mainBigShape[2] - bigRightBottom[10]);
            bigRightBottom = bigRightBottom.Mirror((bigRightBottom[10], bigRightBottom[14]));
            var bigRightUp = mainBigShape.Move(mainBigShape[17] - mainBigShape[8]);
            var bigLeftUp = bigRightBottom.Move(bigLeftBottom[18] - bigRightBottom[18]);

            var shape = bigLeftBottom.Join(bigRightBottom).Join(bigRightUp).Join(bigLeftUp);

            var shiftX = bigRightUp[1] - bigLeftUp[1];
            var shiftY = bigLeftUp[12] - bigLeftBottom[1];

            var normalizedShape = shape.Normalize();

            return new Tile
            {
                ShiftX = new Vector2[] { shiftX },
                ShiftY = new Vector2[] { shiftY },
                //ShiftX = new Vector2[] { (1, 0) },
                //ShiftY = new Vector2[] { (0, 1) },
                Points = normalizedShape.Points,
                Convexes = normalizedShape.Convexes
            };
        }
         
    }
}
