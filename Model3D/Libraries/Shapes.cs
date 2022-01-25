using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Drawing;
using System.Linq;
using Meta;
using Model3D.Libraries;
using Model3D.Tools;
using View3D.Libraries;

namespace Model.Libraries
{
    public static class Shapes
    {
        private static Vectorizer vectorizer = DI.Get<Vectorizer>();

        private static readonly double hTet2 = 3.0.Sqrt() / 2;
        private static readonly double hTet3 = 6.0.Sqrt() / 3;

        public static Shape Chesss(int n) => new Shape
        {
            Points = Ranges.Range(n, n).SelectMany(pair => Polygons.Square.Points.Select(p => (p / 2 + new Vector2(pair)) / (n - 1) - new Vector2(0.5, 0.5))
                .Select(v => new Vector4(v.x, v.y, 0, 1))).ToArray(),

            Convexes = Ranges.Range(n, n).Select(pair => 4 * n * pair.Item1 + 4 * pair.Item2).Select(i => new int[] { i, i + 1, i + 2, i + 3 }).ToArray()
        };

        public static Shape Coods => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(-5, 0, 0),
                new Vector3(100, 0, 0),
                new Vector3(0, -5, 0),
                new Vector3(0, 100, 0),
                new Vector3(0, 0, -5),
                new Vector3(0, 0, 100),
                new Vector3(97, 1, 1),
                new Vector3(97, -1, -1),
                new Vector3(97, 1, -1),
                new Vector3(97, -1, 1),
                new Vector3(1, 97, 1),
                new Vector3(-1, 97, -1),
                new Vector3(-1, 97, 1),
                new Vector3(1, 97, -1),
                new Vector3(1, 1, 97),
                new Vector3(-1, -1, 97),
                new Vector3(1, -1, 97),
                new Vector3(-1, 1, 97),
            },
            Convexes = new int[][]
            {
                new int[] { 0, 1 },
                new int[] { 2, 3 },
                new int[] { 4, 5 },
                new int[] { 1, 6 },
                new int[] { 1, 7 },
                new int[] { 1, 8 },
                new int[] { 1, 9 },
                new int[] { 3, 10 },
                new int[] { 3, 11 },
                new int[] { 3, 12 },
                new int[] { 3, 13 },
                new int[] { 5, 14 },
                new int[] { 5, 15 },
                new int[] { 5, 16 },
                new int[] { 5, 17 },
            }
        }.Mult(0.01).ToLines3(0.7, Color.Red);

        public static Shape CoodsWithText =>
            Coods +
            IcosahedronSp2.Mult(0.01).ApplyColor(Color.Red) +
            vectorizer.GetText("x", 100, "Georgia").Mult(0.02).Move(0.96, -0.05, 0).ToLines3(0.6, Color.Red) +
            vectorizer.GetText("y", 100, "Georgia").Mult(0.02).Move(0.01, 0.96, 0).ToLines3(0.6, Color.Red) +
            vectorizer.GetText("z", 100, "Georgia").Mult(0.02).Rotate(Rotates.Z_X).Move(0, -0.05, 1).ToLines3(0.6, Color.Red);

        public static Shape Line => new Shape()
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
            },
            Convexes = new int[][]
            {
                new int[] { 0, 1 },
            }
        };

        public static Shape Tetrahedron => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0.5, hTet2, 0),
                new Vector3(0.5, hTet2/3, hTet3),
            },
            Convexes = new int[][]
            {
                new int[] {0, 2, 1},
                new int[] {0, 3, 2},
                new int[] {0, 1, 3},
                new int[] {1, 2, 3}
            }
        }.MassCentered();

        public static Shape Cube => NativeCube.MassCentered();

        public static Shape NativeCube => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
                new Vector3(1, 0, 1),
                new Vector3(1, 1, 1),
                new Vector3(0, 1, 1)
            },
            Convexes = new int[][]
            {
                new int[] {0, 3, 2, 1},
                new int[] {0, 1, 5, 4},
                new int[] {0, 4, 7, 3},
                new int[] {6, 5, 1, 2},
                new int[] {6, 2, 3, 7},
                new int[] {6, 7, 4, 5},
            }
        };

        public static Shape IcosahedronSp1 => Icosahedron.SplitSphere(1.2);
        public static Shape IcosahedronSp2 => IcosahedronSp1.SplitSphere(1.2);
        public static Shape IcosahedronSp3 => IcosahedronSp2.SplitSphere(1.3);
        public static Shape IcosahedronSp4 => IcosahedronSp3.SplitSphere(1.4);
        public static Shape Ball => IcosahedronSp4.SplitSphere(1.5);
        public static Shape GolfBall => Ball.JoinConvexesBy6();
        public static Shape GolfBall4 => IcosahedronSp4.JoinConvexesBy6();
        public static Shape GolfBall3 => IcosahedronSp3.JoinConvexesBy6();
        public static Shape GolfBall2 => IcosahedronSp2.JoinConvexesBy6();
        public static Shape GolfBall1 => IcosahedronSp1.JoinConvexesBy6();

        public static Shape Icosahedron => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(1.618033988749895, 1, 0),
                new Vector3(1.618033988749895, -1, 0),
                new Vector3(-1.618033988749895, -1, 0),
                new Vector3(-1.618033988749895, 1, 0),
                new Vector3(0, 1.618033988749895, 1),
                new Vector3(0, -1.618033988749895, 1),
                new Vector3(0, -1.618033988749895, -1),
                new Vector3(0, 1.618033988749895, -1),
                new Vector3(1, 0, 1.618033988749895),
                new Vector3(1, 0, -1.618033988749895),
                new Vector3(-1, 0, -1.618033988749895),
                new Vector3(-1, 0, 1.618033988749895),
            },
            Convexes = new int[][]
            {
                new int[]{4, 0, 7},
                new int[]{3, 4, 7},
                new int[]{10, 3, 7},
                new int[]{9, 10, 7},
                new int[]{0, 9, 7},
                new int[]{5, 8, 11},
                new int[]{5, 11, 2},
                new int[]{5, 2, 6},
                new int[]{5, 6, 1},
                new int[]{5, 1, 8},
                new int[]{4, 8, 0},
                new int[]{8, 4, 11},
                new int[]{3, 11, 4},
                new int[]{11, 3, 2},
                new int[]{10, 2, 3},
                new int[]{2, 10, 6},
                new int[]{9, 6, 10},
                new int[]{6, 9, 1},
                new int[]{0, 1, 9},
                new int[]{1, 0, 8},
            }
        }.Normed();

        public static Shape Dodecahedron => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, -0.6180340051651001, -1.618033945774331),
                new Vector3(-0.6180340051651001, -1.618033945774331, 0),
                new Vector3(-1.618033945774331, 0, -0.6180340051651001),
                new Vector3(-1, -1, -1),
                new Vector3(-1, -1, 1),
                new Vector3(0, -0.6180340051651001, 1.618033945774331),
                new Vector3(-0.6180340051651001, 1.618033945774331, 0),
                new Vector3(-1.618033945774331, 0, 0.6180340051651001),
                new Vector3(-1, 1, -1),
                new Vector3(-1, 1, 1),
                new Vector3(0, 0.6180340051651001, -1.618033945774331),
                new Vector3(0.6180340051651001, -1.618033945774331, 0),
                new Vector3(1.618033945774331, 0, -0.6180340051651001),
                new Vector3(1, -1, -1),
                new Vector3(1, -1, 1),
                new Vector3(0, 0.6180340051651001, 1.618033945774331),
                new Vector3(0.6180340051651001, 1.618033945774331, 0),
                new Vector3(1.618033945774331, 0, 0.6180340051651001),
                new Vector3(1, 1, -1),
                new Vector3(1, 1, 1),
            },
            Convexes = new int[][]
            {
                new[] {7, 9, 6, 8, 2},
                new[] {9, 15, 19, 16, 6},
                new[] {6, 16, 18, 10, 8},
                new[] {19, 17, 12, 18, 16},
                new[] {4, 5, 15, 9, 7},
                new[] {7, 2, 3, 1, 4},
                new[] {8, 10, 0, 3, 2},
                new[] {17, 19, 15, 5, 14},
                new[] {12, 17, 14, 11, 13},
                new[] {18, 12, 13, 0, 10},
                new[] {13, 11, 1, 3, 0},
                new[] {1, 11, 14, 5, 4}
            }
        }.Normed();
    }
}
