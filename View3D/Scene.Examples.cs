using System;
using System.Drawing;
using System.Linq;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Fourier;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Tools;
using View3D.Libraries;

namespace View3D
{
    partial class Scene
    {
        // var polygon = Sinus(3, 50);
        // var polygon = Spiral(3, 60);
        // var polygon = Elipse(1, 0.3, 30);
        // var polygon = Elipse(0.4, 1, 10);
        // var polygon = Square.PutInside(Spiral(3, 60));
        // var polygon = Square.PutInside(Square.MultOne(0.9));
        // var polygon = Polygons.Square.PutInside(Polygons.Sinus(3, 100));
        // var shape = Polygons.Square(1).PutInside(Polygons.Spiral(10, 800).Mult(1)).MakeShape().Transform(Multiplications.Cube);
        // var polygon = Polygons.Square.PutInside(Polygons.Sinus(1.7, 1.2, 3, 300));
        // var polygon = Polygons.Elipse(1, 1, 50).PutInside(Polygons.Sinus(1.7, 1.2, 3, 300).Mult(0.8));
        // var shape = Polygons.Square(1).PutInside(Polygons.Elipse(1, 1, 50).Mult(0.7)).Fill().ToShape().Transform(Transformations.Plane);
        // var shape = Shapes.Chesss(25).Mult(2).AddZVolume(1.0 / 25).ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
        // Spiral Sphere // var shape = Polygons.Elipse(1, 1, 50).PutInside(Polygons.Spiral(15, 1000).Mult(1.23)).MakeShape().Transform(Multiplications.Cube).ToSphere();
        // Sphere Heart //var shape = Polygons.Spiral(25, 4000).Mult(1.23).MakeShape().Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);
        // Arabica Heart // var shape = Polygons.Spiral(25, 4000).Mult(1.23).MakeShape().ApplyZ(Funcs3Z.Hyperboloid).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);
        // Snake Heart // var shape = Polygons.Spiral(25, 4000).Mult(1.23).Mult(2).MakeShape(true).ApplyZ(Funcs3Z.Hyperboloid).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);
        // Kershner Heart // var shape = Polygons.Square.PaveInside(Parquets.PentagonalKershner8(0.02, 1.5)).ToShape3().ToMetaShape(0.4, 40).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart).Scale(0.7, 1, 1).Rotate(Rotates.Z_Y);
        // Saddle Net // var shape = Parquets.Triangles(0.1).ToShape3().Mult(2).ToMetaShape().ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y);
        // Saddle Hexagon Net // var shape = Parquets.Hexagon(0.1).ToShape3().Mult(2).ToMetaShape().ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
        // VualLy //var shape = Parquets.PentagonalKershner8(0.05, 1.5).ToShape3().ToMetaShape(0.5, 20).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y);
        // NeedToCut // var polygon = Polygons.Heart(1, 1, 50).Move((0,-0.1)).Mult(1.2); var shape = Paver.Pave(polygon, Parquets.PentagonalKershner8(0.03, 1.5).Mult(1)).Join(polygon.ToShape2()).ToShape3().ToMetaShape(0.5, 20).ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
        // Broken Heart // var shape = Polygons.Square.Scale((2 * Math.PI-0.1, Math.PI)).PaveInside(Parquets.PentagonalKershner8(0.015, 1.5).Mult(7)).Move((Math.PI / 2, -Math.PI/2)).ToShape3().ToMetaShape(1, 20).Transform(TransformFuncs3.HeartWrap).Scale(0.4, 1, 1).Rotate(Rotates.Z_Y);
        // Kersher8 Tubes // var shape = Parquets.PentagonalKershner8ForTube(3, 1, 1.5).ToShape3().ToMetaShape(8 / 2.0, 20).AddVolumeZ(0.05).Transform(TransformFuncs3.CylinderWrapZ).Rotate(Rotates.Z_Y);
        // Kersher8 Tube // var shape = Parquets.PentagonalKershner8ForTube(3, 10, 1.5).ToShape3().ToPlaneLines(40).AddVolumeZ(0.05).Transform(TransformFuncs3.CylinderWrapZ).Rotate(Rotates.Z_Y);
        // Last Construction day ever! // var shape = Parquets.PentagonalKershner8ForTube(3, 54, 1.5).ToShape3().ToPlaneLines(40).AddVolumeZ(0.05).Transform(TransformFuncs3.CylinderWrapZ).Scale(0.1, 0.1, 1).Move(0, 0, -5).CurveZ(Funcs3.Spiral4);
        // Inside sphere // var shape = Parquets.Squares(31, 52, 0.1).Scale((Math.PI/3.1, Math.PI / 3.1)).Move((Math.PI, -Math.PI/2)).ToShape3().ToPlaneLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.SphereWrapZ).Rotate(Rotates.Z_Y);
        // Under Construction // var shape = Parquets.Triangles(12, 40, 0.1).Scale((Math.PI/3.1, 3.0.Sqrt() / 1.7)).Move((Math.PI, -Math.PI/2)).ToShape3().ToPlaneLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.HeartWrapZ).Rotate(Rotates.Z_Y).Scale(1,1,0.7);
        // Still Kersher8 // var shape = Parquets.PentagonalKershner8(0.05, 0.3).ToShape3().ToMetaShape(0.5, 20).AddVolumeZ(0.01);
        // Kershner8 flower // var shape = Parquets.PentagonalKershner8ForTube(15, 15, 1.6).Move((0, 0)).ToShape3().ToPlaneLines(5).AddVolumeZ(0.005).Transform(TransformFuncs3.Flower(1.5, 3, 7)).ApplyZ(Funcs3Z.Paraboloid).Rotate(Rotates.Z_Y).Scale(1, 0.2, 1);
        // Alive Mistake // var shape = Parquets.PentagonalKershner8ForTube(15, 15, 1.6).Move((0, 0)).ToShape3().ToPlaneLines(5).AddVolumeZ(0.005).Transform(TransformFuncs3.Flower(1.5, 3, 5)).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).Scale(1, 0.2, 1).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y);            
        // Mistake Evolution // var shape = Parquets.PentagonalKershner8ForTube(30, 30, 1.6).ToShape3().ToPlaneLines(5).AddVolumeZ(0.005).Transform(TransformFuncs3.Flower(1.5, 3, 15)).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).Scale(1, 0.2, 1).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y);            
        // Print, Kershner8 // var shape = Parquets.PentagonalKershner8(0.075, 1.5).Rotate(-1.15).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
        // Print, Kershner8MugStand // var shape = Polygons.Elipse(1, 1, 50).PaveInside(Parquets.PentagonalKershner8(0.03, 1.5).Mult(1.5)).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
        // Print, SpiralMugStand // var shape = Polygons.Elipse(1, 1, 100).PutInside(Polygons.Spiral(25, 6000).Mult(1.25)).MakeShape().AddVolumeZ(0.01);
        // Print, Kershner8HeartMugStand // var shape = Polygons.Heart(1, 1, 100).PaveInside(Parquets.PentagonalKershner8(0.01, 1.9).Mult(1.5)).ToShape3().ToMetaShape(0.2,50).AddVolumeZ(0.01);
        // Shamrock // var shape = Surfaces.Shamrock(400, 30).ToLines(4).Rotate(Rotates.Z_Y);
        // Kershner8 shamrock // var shape = Parquets.PentagonalKershner8ForTube(5, 75+5, 1.6).Scale((1, 0.4/3)).Move(0, -Math.PI/12).PullOnSurface90(SurfaceFuncs.Shamrock).ToLines(4).Rotate(Rotates.Z_Y);
        // Shell // var shape = Parquets.PentagonalKershner8ForTube(10, 75, 1.6).Scale((1, 0.8/3)).PullOnSurface90(SurfaceFuncs.Shell).ToLines(8).Rotate(Rotates.Z_Y);
        // See Shell // var shape = Parquets.PentagonalKershner8ForTube(10, 75, 1.6).Scale((1, 1.6/3)).PullOnSurface90(SurfaceFuncs.SeeShell).ToLines(20).Rotate(Rotates.Z_Y);
        // Dini surface // var shape = Surfaces.DiniSurface(100, 50).ToLines(2).Rotate(Rotates.Z_Y); // var shape = Surfaces.DiniSurface(120, 30).MassCentered().Normed().Move(0, 0, 1).ToLines(0.2, Color.Blue)
        // Mobius Strip // var shape = Surfaces.MobiusStrip(62, 10).ToLines(2).Rotate(Rotates.Z_Y);
        // Kershner try Mobius Strip // var shape = Parquets.PentagonalKershner8ForTube(31, 10, 1.6).Scale(0.98, 1).Move(Math.PI, -1 + 0.1).PullOnSurface(SurfaceFuncs.MobiusStrip).ToLines(1).Rotate(Rotates.Z_Y);
        // Mobius is so ...ing spectial // var shape = Surfaces.MobiusStrip(124, 20).Rotate(Rotates.Z_Y).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).ApplyZ(Funcs3Z.Hyperboloid).ToLines(2);
        // Fractal, Tree3 // var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10).Rotate(Rotates.Z_Y);
        // Never Mind // var shape = ShapeFractals.NeverMindTree(6).Rotate(Rotates.Z_Y) + Shapes.Cube;
        // Never Mind 3D // var shape = ShapeFractals.NeverMindTree3D(5).Rotate(Rotates.Z_Y) + Shapes.Cube;
        // Parabola Tree // var shape = ShapeFractals.ParabolaTree(5).Rotate(Rotates.Z_Y) + Shapes.Cube;
        // Normal Distribution // var shape = Surfaces.NormalDistribution(30, 30, 0.6, 0, 6).ToMetaShape3(3,3);
        // Normal Distribution gradient // var shape = Surfaces.NormalDistribution(55, 55, 0.5, 10, 4).ToMetaShape3(5, 5).Rotate(Rotates.Z_Y).ApplyColorGradientY(Color.DarkRed, Color.Red, Color.White);
        // Dark Heart //var shape = Parquets.Triangles(12, 40, 0.1).Scale((Math.PI / 3.1, 3.0.Sqrt() / 1.7)).Move((Math.PI, -Math.PI / 2)).ToShape3().ToPlaneLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.HeartWrapZ).Rotate(Rotates.Z_Y).Scale(1, 1, 0.7).Rotate(Rotates.Y_mZ).ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.FromArgb(20, 20, 20), Color.Red, Color.Red);
        // Green tree // var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10, true, Color.FromArgb(0, 10, 0), Color.FromArgb(0, 50, 0)).Rotate(Rotates.Z_Y);
        // Polynom // var shape = Surfaces.Cylinder(8, 61).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.RootPolinomY(1.0/20, new[]{ -3, -2, -0.5, 0, 1.1, 2.2, 3})) + Shapes.Cube;
        // Fourier  eagle // var shape = Polygons.FourierSeries(400, ((0.05, 0), 20), (Fourier.RotateN(1, 4), 1)).ToShape2().ToShape3().ToLines();
        // Rainbow // var shape = Surfaces.Plane(300, 30).Move(-150, -15, 0).Mult(0.0020).ApplyFn(null, v => -v.y - v.x * v.x, v=>0.005*Math.Sin(v.x*171 + v.y*750)).ToSpots3(0.05).ApplyColorGradientZ((x, y) => -x * x - y, Color.Red, Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.DarkBlue, Color.Purple, Color.Purple);
        // Barnsley Fern // var shape = IteratedFunctionSystem.BarnsleyFern(20000).Select(v => v.ToV3()).ToShape().ToTetrahedronSpots3().ApplyColor(Color.Blue);
        // Mandelbrot // var s = MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).Select(v => v.ToV3()).ToShape().ToCubeSpots3(0.2).ScaleZ(15).ApplyColor(Color.Blue) + Shapes.Ball.Mult(0.1).ApplyColor(Color.Red);
        // Maze 5 5 5 // var shape = Mazes.CreateNet3Maze(5, 5, 5).ToCubeMetaShape3(10, 10, Color.Blue, Color.Red);
        // Kershner8 Maze // var shape = Mazes.CrateKershner8Maze(0.03, 1.7, -1.09, 5).ToMetaShape3(0.2, 0.5, Color.Blue, Color.Red);
        // Fantastic Kershner8 Maze // var shape = Mazes.CrateKershner8Maze(0.01, 1.7, -1.09, 5).Mult(3).Transform(TransformFuncs3.Flower(0.3, 0.3, 5)).ToLines(0.2, Color.Green);
        // Kershner8 Wow Maze // var shape = Mazes.CrateKershner8Maze(0.03, 1.7, -1.09, 5).Mult(3).Transform(TransformFuncs3.Flower(0.5,0.5,5)).ToMetaShape3(0.2, 0.5, Color.Blue, Color.Red);
        // Kershner8 Wow Maze optimized // var shape = Mazes.CrateKershner8Maze(0.003, 1.7, -1.09, 5).Mult(3).Transform(TransformFuncs3.Flower(0.3, 0.3, 5)).ToTetrahedronMetaShape3(0.1, 0.3, Color.Blue, Color.Red);
        // Mobius Maze // var shape = Surfaces.MobiusStrip(128, 20).ToMaze(0, MazeType.SimpleRandom).ToLines(2).Rotate(Rotates.Z_Y).ApplyColor(Color.FromArgb(20, 20, 20));
        // Maze with path // var (maze, path) = Parquets.PentagonalKershner8(0.01, 1.7).Rotate(-1.09).ToShape3().Mult(3).Transform(TransformFuncs3.Flower(0.3, 0.3, 5)).ToMazeWithPath(1, new[] { (6, 7), (-6, -5) });            var shape = maze.ToLines(0.2, Color.Blue) + path.ToLines(0.2, Color.Red);
        // Imposible maze // var (maze, path) = Parquets.PentagonalKershner8(0.002, 1.7).ToShape3().Mult(4).ToMazeWithPath(1, MazeType.SimpleRandom, new[] { (6, 7), (-6, -5) });             var enter = Surfaces.Sphere(10, 10).Mult(0.005).Move(path.Points3[0]).ApplyColor(Color.Black);            var exit = Surfaces.Sphere(10, 10).Mult(0.005).Move(path.Points3[^1]).ApplyColor(Color.Green);            var shape = maze.ToLines(0.2, Color.Blue) + enter + exit + path.ToLines(0.2, Color.Red); //.Transform(TransformFuncs3.Torus(1.5))
        // Gravity maze // var (maze, path) = Parquets.Squares(50, 50, 0.04).ToShape3().ApplyZ(Funcs3Z.Paraboloid).ToMazeWithPath(1, MazeType.PowerGravity);            maze = maze.Rotate(Rotates.Z_Y);           path = path.Rotate(Rotates.Z_Y);            var enter = Surfaces.Sphere(10, 10).Mult(0.01).Move(path.Points3[0]).ApplyColor(Color.Black);            var exit = Surfaces.Sphere(10, 10).Mult(0.01).Move(path.Points3[^1]).ApplyColor(Color.Green);                  var shape = maze.ToLines(1, Color.Blue) + enter + exit + path.ToLines(0.3, Color.Red);
        // Not bad font // var shape = Texter.GetText("This is not a 3d font\r\nbut\r\nthis is already not bad", 50).ToCubeSpots3(50).ApplyColorGradientY(Color.Red, Color.Red, Color.White);
        // LNT // var shape = Vectorizer.GetText("ВОЙНА И МИР\r\nТОМ ПЕРВЫЙ\r\nЧАСТЬ ПЕРВАЯ\r\nI", 100, "Times New Roman").ToLines(300, Color.Red);
        // LNT2 // var shape = Vectorizer.GetText("— Eh bien, mon prince. Gênes et Lucques ne sont plus que des apanages, des\r\nпоместья, de la famille Buonaparte. Non, je vous préviens que si vous ne me dites pas\r\nque nous avons la guerre, si vous vous permettez encore de pallier toutes les infamies,\r\ntoutes les atrocités de cet Antichrist (ma parole, j'y crois) — je ne vous connais plus,\r\nvous n'êtes plus mon ami, vous n'êtes plus мой верный раб, comme vous dites.", 100, "Times New Roman").ToLines(300, Color.Red);
        // Bird // var shape = Vectorizer.GetContentShape(settings.GetContentFileName("bird1.jpg")).ApplyZ(Funcs3Z.Paraboloid).ToLines(0.5).ApplyColorGradientY(Color.Red, Color.Red, Color.White);
        // Iguana //var shape = Vectorizer.GetContentShape(settings.GetContentFileName("iguana1.jpg")).ApplyZ(Funcs3Z.Waves).ToLines(0.5).ApplyColorGradientZ(Color.Black, Color.Black, Color.DarkBlue);
        // Dragon // var shape = Vectorizer.GetContentShape(settings.GetContentFileName("dragon4.jpg")).ApplyZ(Funcs3Z.Waves).ToLines(0.2).ApplyColorGradientZ(Color.DarkRed, Color.Black, Color.DarkRed);
        // Fourier Man // var shape = FourierShapes.Series(new Fr[] { (-3, 1), (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }).ToSingleShape();
        // Fourier Woman // var shape = FourierShapes.Series(new Fr[] { (-7, 1), (-3, 2), (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }).ToSingleShape();
        // Fourier Athlete // var shape = FourierShapes.Series(new Fr[] { (1, 1), (2, -2), (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }).ToSingleShape();
        // Fourier Perfect Man // var shape = FourierShapes.Series(new Fr[] { (1, 1), (2, -2), (-11, 1), (-6, 2), (-9, 1), (4, 3), (-1, 12) }).ToSingleShape();
        // Fourier search humans // var shape = FourierShapes.SearchSeries(new Fr[] { (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }, 1, 2, -20, 20, -20, 20);
        // Fourier Kung Fu, let's start // var shape = FourierShapes.Series(new Fr[] { (-11, 1, 0.1), (-9,1),(-6,2,0.15),(-3,2),(-1,13),(1,1),(2,-2),(4,3),(9,-1) });
        // Fourier, kung fu best // (-41, 0.25), (-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 1.8), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1) //Transform(TransformFuncs3.RotateX(0.1, 10))
        // Fourier, elephant // (10, -1), (9, 3), (4, -7), (-3, -8), (3, 5), (-9, 3), (2, 5), (-18, 3), (1, 3), (-6, -1), (200, -1), (-16, 1), (11, -2), (16, 1), (13, -2), (-8, 5), (-37, 2), (1,2), (3,4), (-2,3), (-1,5), (5,1), (16, -3),
        // Fourier, for search // (-1, 10), (17, 1), (20, -2), (200, 0.2), (-1, 4), (2, 7),

        // compositions: .ApplyZ(Funcs3Z.SphereR(1.2)); Shapes.IcosahedronSp2.Mult(0.02).ApplyColor(Color.Red); Shapes.GolfBall.Move(0.7, 1.5, 2).ToLines(1, Color.Red)

        #region BlackHole

        public class BlachHoleOptions
        {
            public Shape Shape;
            public double? BlowRadius;
            public double? BlowFactor;
            public double? AngleSpeed;
            public double? GravityPower;
            public bool NoRotation;
            public bool? TryBeauty;
            public int? InterationsCount;
            public Color?[] Colors;
        }

        // bh&h
        //return BlackHole(new BlachHoleOptions()
        //{
        //    Shape = Surfaces.Heart(200, 100).Perfecto(3.5),
        //    NoRotation = false,
        //    BlowFactor = 0,
        //    BlowRadius = 0,
        //    InterationsCount = 40
        //});

        public Shape BlackHole(BlachHoleOptions options = null)
        {
            options ??= new BlachHoleOptions();

            var blowRadius = options.BlowRadius ?? 1.6;
            var blowFactor = options.BlowFactor ?? 0.4;
            var angleSpeed = options.AngleSpeed??0.5;
            var gravityPower = options.GravityPower??0.7;
            var gravityPoint = new Vector3(8, 0, 0);
            var useRotation = !options.NoRotation;
            var iterationsCount = options.InterationsCount??25;
            var tryBeauty = options.TryBeauty??true;
            var palette = options.Colors ?? new Color?[]
            {
                Color.DarkRed, Color.DarkRed, Color.Red, Color.Red, Color.DarkGoldenrod, Color.White
            };

            var r = new Random(0);
            var blowedShape = options.Shape ?? Shapes.Ball;

            Vector3 GetNativePowerPoint(double power, Vector3 to, Vector3 p, int count)
            {
                var v = (to - p).MultV(Vector3.YAxis).ToLen(angleSpeed);

                for (var i = 0; i < count; i++)
                {
                    var gForce = (to-p).ToLen(power / (to - p).Length2);
                    v += gForce;
                    p += v;
                }

                return p;
            }

            Vector3 GetBeautyPowerPoint(double power, Vector3 to, Vector3 p, int count)
            {
                double? w0 = null;

                for (var i = 0; i < count; i++)
                {
                    var offset = power / (to - p).Length2;
                    var w = Math.Sqrt(1 / offset);
                    if (!w0.HasValue)
                        w0 = w;

                    var wk = w / w0.Value;

                    p = p + offset * (to - p) + (to - p).MultV(Vector3.YAxis).ToLen(angleSpeed * wk);
                }

                return p;
            }

            Shape GetShape(Shape s)
            {
                var center = s.MassCenter;
                var dir = center.Normalize();
                var rot = new Vector3(r.NextDouble(), r.NextDouble(), r.NextDouble());
                var dist = blowRadius * (1 + blowFactor * (r.NextDouble()-1));

                var point = (1 + dist) * dir;
                var powerPoint = tryBeauty 
                    ? GetBeautyPowerPoint(gravityPower, gravityPoint, point, iterationsCount)
                    : GetNativePowerPoint(gravityPower, gravityPoint, point, iterationsCount);

                if (useRotation)
                    s = s.Move(-center).Rotate(Quaternion.FromEulerAngle(rot)).Move(center);

                return s.Move(powerPoint - dir);
            }

            var shape = new Shape[]
            {
                Shapes.Ball.Mult(0.3).ApplyColor(Color.Black).Move(gravityPoint),

                blowedShape
                    //.ApplyColorGradientX(palette)
                    .SplitByConvexes()
                    .Select(GetShape)
                    .ToSingleShape()
                    .ApplyColorSphereGradient(gravityPoint, palette.Reverse().ToArray()),

                //Shapes.CoodsWithText,
            }.ToSingleShape();

            return shape;
        }
        #endregion

        #region CubeGalaxiesIntersection

        class Particle : IAnimatorParticleItem
        {
            public int i;
            public Vector3 Position { get; set; }
            public Vector3 Speed { get; set; }
            public Color Color;
            public Func<Shape, Shape> ModifyFn;
        }

        public Shape CubeGalaxiesIntersection(double? netSize = 0.5, double gravityPower = 0.00001, int count = 10, double pSize = 0.1, double cubeStretch = 5, double sceneSize = 10)
        {
            var particleShape = Shapes.Cube.Perfecto(pSize);

            var data = new (Shape s, Vector3 shift, Func<Shape, Vector3> speed, Func<Shape, Shape> modifyFn, Color color)[]
            {
                (Shapes.Cube.SplitPlanes(0.1).ScaleY(cubeStretch), 
                    new Vector3(-2.5, 0, 0),
                    s => 0.5 * s.MassCenter.MultV(Vector3.YAxis), 
                    s=>s,
                    Color.Black),
                
                (Shapes.Cube.SplitPlanes(0.1).ScaleY(cubeStretch).Rotate(1, 1, 1), 
                    new Vector3(2.5, 0, 0),
                    s => 0.5 * s.MassCenter.MultV(Vector3.YAxis.Rotate(1, 1, 1)), 
                    s=>s.Rotate(1,1,1),
                    Color.Black),
            };

            var particles = data
                .SelectMany(s => s.s.SplitByConvexes()
                    .Select(ss => new Particle()
                    {
                        Position = ss.MassCenter + s.shift,
                        Speed = s.speed(ss),
                        Color = s.color,
                        ModifyFn = s.modifyFn
                    }))
                .Select((p, i) =>
                {
                    p.i = i;
                    return p;
                }).ToArray();

            var animator = new Animator(new AnimatorOptions()
            {
                UseParticleGravityAttraction = true,
                GravityAttractionPower = gravityPower,
                NetSize = netSize,
                NetFrom = new Vector3(-6, -6, -6),
                NetTo = new Vector3(6, 6, 6)
            });

            animator.AddItems(particles);
            animator.Animate(count);

            var shape = particles.Where(p => p.Position.Length < sceneSize).Select(p => p.ModifyFn(particleShape).Move(p.Position).ApplyColor(p.Color))
                .ToSingleShape()
                .ApplyColorSphereGradient(Color.White, Color.Black, Color.Black);

            //var fieldPoint = Shapes.Tetrahedron.Mult(0.1).ApplyColor(Color.Blue);
            //var field = animator.NetField.Select(p => fieldPoint.Move(p)).ToSingleShape();

            return shape/* + field + Shapes.CoodsWithText*/;
        }

        #endregion

        public Shape Lion()
        {
            var k = 0.27;
            var dk = 0.01;

            var s = vectorizer.GetContentShape("l5");
            var s1 = s.Where(v => v.Length < k).ApplyZ(Funcs3Z.SphereR(0.5)).AlignZ(0).ToLines()
                .ApplyColorGradientZ(Color.Blue, Color.Blue, Color.Blue, Color.Blue, Color.Blue, Color.Blue, Color.White);

            var s2 = s.Where(v => v.Length > k - dk).ToLines()
                .ApplyColor(Color.Blue);

            var l = vectorizer.GetContentShape("l6").ApplyZ(Funcs3Z.SphereRC(1.5)).Perfecto(0.35).Rotate(Rotates.Z_Y);
            var l1 = l.Rotate(0, -1, 10).Move(-0.2, -0.8, 0.2).ToLines()
                .ApplyColorGradientZ(Color.Blue, Color.White);
            var l2 = l.Rotate(0, -1, 1).Move(0.2, -0.6, -0.3).ToLines()
                .ApplyColorGradientZ(Color.Blue, Color.Blue);

            var shape = new Shape[]
            {
                s1, s2, l1, l2
            }.ToSingleShape();

            return shape;
        }

        public Shape Footprint()
        {
            var s = vectorizer.GetContentShape("w21").Perfecto(2).ApplyZ(Funcs3Z.Hyperboloid).MoveZ(2).ToLines(2)
                .ApplyColor(Color.Blue);

            var r = 10;
            var fnz = Funcs3Z.SphereR(r);

            var shape = new Shape[]
            {
                EnumerableV2.Wedge(10, true).Select(p=>2*p).Select(p => s.Move(p.ToV3(fnz(p.x,p.y)-r))).ToSingleShape(),
                Surfaces.Plane(50, 50).Perfecto(10).ApplyZ(fnz).Centered().ToLines(4).ApplyColor(Color.Red),
                Shapes.GolfBall.Mult(30).ToLines(20).Move(0, 45, 20).ApplyColor(Color.DarkOrange)
            }.ToSingleShape().Rotate(Rotates.Z_Y);

            return shape;
        }

        public Shape Polygons()
        {
            var fShape = new Fr[]
                {(-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1)};

            var mb = MandelbrotFractalSystem.GetPoints(2, 0.002, 1000);

            var mbFilled = mb.ToPolygon().PutInside(fShape.ToShapes(1000)[0].ToPolygon().Mult(0.6).MoveX(-0.15)).ToShape(0.01).ApplyColor(Color.Green);
            var mbOutLine = mb.ToShape().ToLines(0.2).ScaleZ(15).ApplyColor(Color.Blue);
            var frInsideLine = fShape.ToLineShape(1000, 0.2).ScaleZ(16 / 0.6).Mult(0.6).MoveX(-0.15).ApplyColor(Color.Blue);
            var cutNet = Surfaces.Plane(100, 100).Perfecto(3).MoveX(-0.5).Cut(mb.ToPolygon()).MoveZ(-0.1).ToLines(0.5).ApplyColor(Color.Blue);

            var shape = new[] { mbFilled, mbOutLine, frInsideLine, cutNet }.ToSingleShape();

            return shape;
        }

        public Shape Waterfall() =>
            WatterSystem.Waterfall(new WaterfallOptions()
            {
                SceneSize = new Vector3(15, 20, 15),
                SphereOffset = new Vector3(0, 0, 0),
                SphereRadius = 5,
                GutterRotation = new Vector3(0, 6, 1),
                GutterCurvature = 0.1,
                ParticleCount = 2000,
                SkipAnimations = 200,
                StepAnimations = 50,
                SceneSteps = (2, 3)
            });
    }
}