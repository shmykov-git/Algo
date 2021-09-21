using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Linq;

namespace Model.Libraries
{
    public static class Shapes
    {
        private static readonly double hTet2 = 3.0.Sqrt() / 2;
        private static readonly double hTet3 = 6.0.Sqrt() / 3;

        public static Shape Chesss(int n) => new Shape
        {
            Points = Ranges.Range(n, n).SelectMany(pair => Polygons.Square.Points.Select(p => (p / 2 + new Vector2(pair)) / (n - 1) - new Vector2(0.5, 0.5))
                .Select(v => new Vector4(v.x, v.y, 0, 1))).ToArray(),

            Convexes = Ranges.Range(n, n).Select(pair => 4 * n * pair.Item1 + 4 * pair.Item2).Select(i => new int[] { i, i + 1, i + 2, i + 3 }).ToArray()
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
        }.Centered();

        public static Shape Cube => NativeCube.Centered();

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
        };
    }
}
