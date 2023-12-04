using Aspose.ThreeD.Utilities;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Drawing;
using System.Linq;
using Meta;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Tools;
using Model3D.Tools.Model;
using View3D.Libraries;
using Model.Fourier;

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

        public static Shape Arrow => ArrowR(30, 1, 0.01, 0.05, 0.02);

        public static Shape ArrowR(int n,
            double lineLn = 1, double lineR = 0.01,
            double arrowLn = 0.05, double arrowR = 0.02) =>
            (
                (
                    (Surfaces.Cylinder(n, 2) + Surfaces.Circle(n, 2)).Scale(lineR, lineR, lineLn) +
                    (Surfaces.Circle(n, 2) + Surfaces.ConeM(n, 2).Move(0,0,1)).Scale(arrowR, arrowR, arrowLn).Move(0, 0, lineLn)
                )
            ).Normalize();

        public static Shape CylinderR(int n, double lineLn = 1, double lineR = 0.1, int m = 2) =>
        (
            (Surfaces.Cylinder(n, m) +
             Surfaces.Circle(n, 2) +
             Surfaces.CircleM(n, 2).Move(0, 0, m-1))
            .Scale(lineR, lineR, lineLn/(m-1))
        ).Normalize();

        public static Shape LineCoods => new Shape
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
        }.Mult(0.01).ToLines(0.7, Color.Red);

        public static Shape Coods
        {
            get
            {
                var a = ArrowR(10, 1.1, 0.005).Move(0, 0, -0.1).ApplyColor(Color.Red);

                return a + a.Rotate(Rotates.Z_X) + a.Rotate(Rotates.Z_Y);
            }
        }

        public static Shape Coods2
        {
            get
            {
                var a = ArrowR(10, 1.1, 0.005).Move(0, 0, -0.1).ApplyColor(Color.Red);

                return a.Rotate(Rotates.Z_X) + a.Rotate(Rotates.Z_Y);
            }
        }

        public static Shape CoodsWithText =>
            Coods +
            IcosahedronSp2.Mult(0.02).ApplyColor(Color.Red) +
            vectorizer.GetText("x", 100, "Georgia").Mult(0.03).Move(0.95, -0.06, 0).ApplyColor(Color.Red) +
            vectorizer.GetText("y", 100, "Georgia").Mult(0.03).Move(0.02, 0.96, 0).ApplyColor(Color.Red) +
            vectorizer.GetText("z", 100, "Georgia").Mult(0.03).Rotate(Rotates.Z_X).Move(0, -0.06, 1).ApplyColor(Color.Red);

        public static Shape Coods2WithText =>
            Coods2 +
            IcosahedronSp2.Mult(0.02).ApplyColor(Color.Red) +
            vectorizer.GetText("x", 100, "Georgia").Mult(0.03).Move(0.95, -0.06, 0).ApplyColor(Color.Red) +
            vectorizer.GetText("y", 100, "Georgia").Mult(0.03).Move(0.02, 0.96, 0).ApplyColor(Color.Red);

        public static Shape CoodsNet => CoodsWithText + Surfaces.Plane(11, 11).Perfecto().ToLines(0.3, Color.Khaki);

        public static Shape Point => new Shape()
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
            },
            Convexes = new int[][]
            {
            }
        };

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

        public static Shape LineN(int n) =>
            new Shape()
            {
                Points3 = (n).SelectRange(i => new Vector3(i, 0, 0)).ToArray(),
                Convexes = (n).SelectRange(i=>i).SelectPair((i,j)=> new[] {i, j}).ToArray()
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

        public static Shape PerfectCube => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 1),
                new Vector3(1, 1, 0),
                new Vector3(1, 1, 1),
            },
            Convexes = new int[][]
            {
                new[]{0, 1, 3, 2},
                new[]{1, 5, 7, 3},
                new[]{4, 6, 7, 5},
                new[]{0, 2, 6, 4},
                new[]{0, 4, 5, 1},
                new[]{2, 3, 7, 6}
            }
        };

        public static Shape CubeT => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 1),
                new Vector3(1, 1, 0),
                new Vector3(1, 1, 1),
            },
            Convexes = new int[][]
            {
                //new[]{0, 1, 3, 2},
                new[]{0, 1, 3},
                new[]{3, 2, 0},
                //new[]{1, 5, 7, 3},
                new[]{1, 5, 7},
                new[]{7, 3, 1},
                //new[]{5, 4, 6, 7},
                new[]{5, 4, 6},
                new[]{6, 7, 5},
                //new[]{4, 0, 2, 6},
                new[]{4, 0, 2},
                new[]{2, 6, 4},
                //new[]{0, 4, 5, 1},
                new[]{0, 4, 5},
                new[]{5, 1, 0},
                //new[]{2, 3, 7, 6}
                new[]{2, 3, 7},
                new[]{7, 6, 2}
            }
        };

        public static Shape PerfectCubeWithCenter => new Shape
        {
            Points3 = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(0, 0, 1),
                new Vector3(0, 1, 0),
                new Vector3(0, 1, 1),
                new Vector3(1, 0, 0),
                new Vector3(1, 0, 1),
                new Vector3(1, 1, 0),
                new Vector3(1, 1, 1),
                new Vector3(0.5, 0.5, 0.5),
            },
            Convexes = new int[][]
            {
                new[]{0, 1, 3, 2},
                new[]{1, 5, 7, 3},
                new[]{5, 4, 6, 7},
                new[]{4, 0, 2, 6},
                new[]{0, 4, 5, 1},
                new[]{2, 3, 7, 6},
                new int[] {8, 0},
                new int[] {8, 1},
                new int[] {8, 2},
                new int[] {8, 3},
                new int[] {8, 4},
                new int[] {8, 5},
                new int[] {8, 6},
                new int[] {8, 7}            }
        };

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

        public static Shape NativeCubeWithCenterPoint => new Shape
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
                new Vector3(0, 1, 1),
                new Vector3(0.5, 0.5, 0.5)
            },
            Convexes = new int[][]
            {
                new int[] {0, 3, 2, 1},
                new int[] {0, 1, 5, 4},
                new int[] {0, 4, 7, 3},
                new int[] {6, 5, 1, 2},
                new int[] {6, 2, 3, 7},
                new int[] {6, 7, 4, 5},
                new int[] {8, 0},
                new int[] {8, 1},
                new int[] {8, 2},
                new int[] {8, 3},
                new int[] {8, 4},
                new int[] {8, 5},
                new int[] {8, 6},
                new int[] {8, 7},
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

        public static Shape Stone(int n, int seed = 0, double power = 1, int quality = 2, Vector3? scale = null)
        {
            var rnd = new Random(seed);
            scale ??= new Vector3(1, 3, 1);

            var dirs = (n).SelectRange(_ => rnd.NextCenteredV3().Normalize()).ToArray();

            Shape GetStoneShape(int stoneQuality) =>
                stoneQuality switch
                {
                    1 => Shapes.Icosahedron,
                    2 => Shapes.IcosahedronSp2,
                    3 => Shapes.IcosahedronSp3,
                    4 => Shapes.IcosahedronSp4,
                    5 => Shapes.Ball,
                    _ => throw new NotSupportedException(stoneQuality.ToString())
                };

            Vector3 TransformFn(Vector3 v)
            {
                return v + power * dirs.Select(dir =>
                {
                    var m = v.MultS(dir);

                    if (m < 0)
                        return Vector3.Origin;

                    return m * dir;
                }).Sum();
            }

            return GetStoneShape(quality).Scale(scale.Value).TransformPoints(TransformFn).Perfecto();
        }

        public static Shape PlaneByTriangles(int m, int n) => Parquets.Triangles(m, n).ToShape3().Adjust();

        public static Shape SquarePlatform(double x = 3, double y = 3, double z = 0.1) => Surfaces.Plane(2, 2)
            .Perfecto().Scale(x,y,1).AddNormalVolume(z).ToOyM().ApplyColor(Color.Black);

        public static Shape CirclePlatform(double x = 3, double y = 3, double z = 0.1) => Surfaces.Circle(100, 2)
            .Perfecto().Scale(x, y, 1).AddNormalVolume(z).ToOy().ApplyColor(Color.Black);

        public static Shape CirclePlatformWithLines(double x = 3, double y = 3, double z = 0.1, int nLine = 10, Color? platformColor = null, Color? lineColor = null) =>
            Surfaces.Circle(100, 2).Mult(0.5).Scale(x, y, 1).AddNormalVolume(z).ToOy().ApplyColor(platformColor??Color.Black)
            + Surfaces.Circle(nLine + 1, 2).Mult(0.5).Scale(x, y, 1).ToOy().ToLines(1, lineColor??Color.Red);

        public static Shape HeartPlatform(double x = 3, double y = 3, double z = 0.1) => Polygons.Heart(1, 1, 100)
            .ToShape(1).Centered().Scale(2.3 * x, 2.3 * y, z).ToOy().MoveY(-z / 2)
            .ApplyColor(Color.Black);

        public static Shape MandelbrotPlatform(double x = 3, double y = 3, double z = 0.1) => MandelbrotFractalSystem.GetPoints(2, 0.002, 1000)
            .ToPolygon().ToShape(4).Mult(0.25).Scale(3 * x, 3 * y, z).ToOy().ToOx().MoveY(-z / 2)
            .ApplyColor(Color.Black);

        public static Shape BatmanPlatform(double x = 3, double y = 3, double z = 0.1) => vectorizer
            .GetContentShapeObsolet("b14")
            .ToPerimeterPolygons()[2].SmoothOut().SmoothOut()
            .ToShape(1).Scale(1.3 * x, 1.3 * y, z).ToOy().MoveY(-z / 2)
            .ApplyColor(Color.Black);

        public static Shape ButterflyPlatform(double x = 3, double y = 3, double z = 0.1) =>
            vectorizer.GetContentShape("b7", new ShapeOptions()
                {
                    PolygonLevelStrategy = LevelStrategy.TopLevel,
                })
                .Scale(1.3 * x, 1.3 * y, z).ToOy().MoveY(-z / 2).ApplyColor(Color.Black);

        public static Shape Butterfly2Platform(double x = 3, double y = 3, double z = 0.1) =>
            vectorizer.GetContentShape("b7", volume:z)
                .Scale(1.3 * x, 1.3 * y, 1).ToOy().MoveY(-z / 2).ApplyColor(Color.Black);

        public static Shape WorldPlatform(double x = 3, double y = 3, double z = 0.1) => vectorizer.GetContentShape(
            "map", new ShapeOptions()
            {
                ColorLevel = 150,
                PolygonLevelStrategy = LevelStrategy.OddLevel
            }).Scale(1.3 * x, 1.3 * y, z).ToOy().MoveY(-z / 2).ApplyColor(Color.Black);

        public static Shape RoundTreePlatform(double x = 3, double y = 3, double z = 0.1) => vectorizer.GetContentShapeObsolet("t3_small", 190)
            .ToPerimeterPolygons().Select(p => p.SmoothOut(2)).ToArray().ComposeObsolet(new[] { (1, 0), (3, 2) })
            .Select(p => p.ToShape(1)).ToSingleShape()
            .Scale(x, y, z).ToOy().MoveY(-z / 2).ApplyColor(Color.Black);

        public static Shape ChristmasTree(int n = 6, int m = 12, double radiusRatio = 0.3, double height = 2)
        {
            var r1 = 8;
            var r2 = r1* radiusRatio;

            var n2 = 2 * n;

            var frs = new Fr[] { (-1, r1), (n - 1, r1 - r2) }.RadiusPerfecto(); // new Fr[] { (-3, 1), (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }

            Vector3 Zfn(Vector2 v, double z)
            {
                var vv = v * (1 - 0.7 * z).Pow2();

                return vv.ToV3(z - 0.1 * (v.Len).Pow2());
            }

            Vector3[] ChristmasTree(int i, double z)
            {
                var polygon = frs.ToPolygon(n2);
                //return polygon.Points.Select(p => p.ToV3(z)).ToArray();
                var ps = polygon.Points.Select(p => Zfn(p, z)).ToArray();

                var s = ps.ToShape();
                if (i % 2 == 1)
                {
                    s = s.Rotate(i % 2 * Math.PI / n);
                    s.Points = s.Points.CircleShift(1);
                }

                return s.Points3;
            }

            var levels = (m).SelectInterval(z => ChristmasTree(z.i, z.v)).ToArray();
            var topP = Zfn((0, 0), 1);
            var bottomP = new Vector3(0, 0, 0);
            var psC = levels.Sum(l => l.Length);

            var shape = new Shape
            {
                Points3 = levels.SelectMany(v => v).Concat(new[] { bottomP, topP }).ToArray(),
                //new int[] { (j + 1) % n2 + n2 * i, (j + 1) % n2 + n2 * (i + 1), j + n2 * (i + 1), j + n2 * i },
                Convexes = (m - 1, n2).SelectRange((i, j) =>
                (i + j) % 2 == 1
                ? new int[][]
                {
                    new int[]{(j + 1) % n2 + n2 * i, (j + 1) % n2 + n2 * (i + 1), j + n2 * (i + 1) },
                    new int[]{j + n2 * (i + 1), j + n2 * i, (j + 1) % n2 + n2 * i },    
                }
                : new int[][]
                {
                    new int[]{(j + 1) % n2 + n2 * (i + 1), j + n2 * (i + 1), j + n2 * i },
                    new int[]{j + n2 * i, (j + 1) % n2 + n2 * i, (j + 1) % n2 + n2 * (i + 1) },
                }
                ).SelectMany(v => v)
                .Concat((n2).SelectRange(v => v).SelectCirclePair((i, j) => new int[] { j, i, psC }))
                .Concat((n2).SelectRange(v => v).SelectCirclePair((i, j) => new int[] { i + psC - n2, j + psC - n2, psC + 1 }))
                .ToArray()
            };

            return shape.ScaleZ(height);
        }
    }
}
