using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Tools;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Aspose.ThreeD.Utilities;
using MathNet.Numerics;
using Model.Graphs;
using Model.Tools;
using View3D.Libraries;

namespace View3D
{
    class Scene
    {
        private readonly Settings settings;
        private readonly Vectorizer vectorizer;

        public Scene(Settings settings, Vectorizer vectorizer)
        {
            this.settings = settings;
            this.vectorizer = vectorizer;
        }

        public Shape GetShape()
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
            // Kersher8 Tube // var shape = Parquets.PentagonalKershner8ForTube(3, 10, 1.5).ToShape3().ToLines(40).AddVolumeZ(0.05).Transform(TransformFuncs3.CylinderWrapZ).Rotate(Rotates.Z_Y);
            // Last Construction day ever! // var shape = Parquets.PentagonalKershner8ForTube(3, 54, 1.5).ToShape3().ToLines(40).AddVolumeZ(0.05).Transform(TransformFuncs3.CylinderWrapZ).Scale(0.1, 0.1, 1).Move(0, 0, -5).CurveZ(Funcs3.Spiral4);
            // Inside sphere // var shape = Parquets.Squares(31, 52, 0.1).Scale((Math.PI/3.1, Math.PI / 3.1)).Move((Math.PI, -Math.PI/2)).ToShape3().ToLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.SphereWrapZ).Rotate(Rotates.Z_Y);
            // Under Construction // var shape = Parquets.Triangles(12, 40, 0.1).Scale((Math.PI/3.1, 3.0.Sqrt() / 1.7)).Move((Math.PI, -Math.PI/2)).ToShape3().ToLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.HeartWrapZ).Rotate(Rotates.Z_Y).Scale(1,1,0.7);
            // Still Kersher8 // var shape = Parquets.PentagonalKershner8(0.05, 0.3).ToShape3().ToMetaShape(0.5, 20).AddVolumeZ(0.01);
            // Kershner8 flower // var shape = Parquets.PentagonalKershner8ForTube(15, 15, 1.6).Move((0, 0)).ToShape3().ToLines(5).AddVolumeZ(0.005).Transform(TransformFuncs3.Flower(1.5, 3, 7)).ApplyZ(Funcs3Z.Paraboloid).Rotate(Rotates.Z_Y).Scale(1, 0.2, 1);
            // Alive Mistake // var shape = Parquets.PentagonalKershner8ForTube(15, 15, 1.6).Move((0, 0)).ToShape3().ToLines(5).AddVolumeZ(0.005).Transform(TransformFuncs3.Flower(1.5, 3, 5)).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).Scale(1, 0.2, 1).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y);            
            // Mistake Evolution // var shape = Parquets.PentagonalKershner8ForTube(30, 30, 1.6).ToShape3().ToLines(5).AddVolumeZ(0.005).Transform(TransformFuncs3.Flower(1.5, 3, 15)).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).Scale(1, 0.2, 1).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y);            
            // Print, Kershner8 // var shape = Parquets.PentagonalKershner8(0.075, 1.5).Rotate(-1.15).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
            // Print, Kershner8MugStand // var shape = Polygons.Elipse(1, 1, 50).PaveInside(Parquets.PentagonalKershner8(0.03, 1.5).Mult(1.5)).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
            // Print, SpiralMugStand // var shape = Polygons.Elipse(1, 1, 100).PutInside(Polygons.Spiral(25, 6000).Mult(1.25)).MakeShape().AddVolumeZ(0.01);
            // Print, Kershner8HeartMugStand // var shape = Polygons.Heart(1, 1, 100).PaveInside(Parquets.PentagonalKershner8(0.01, 1.9).Mult(1.5)).ToShape3().ToMetaShape(0.2,50).AddVolumeZ(0.01);
            // Shamrock // var shape = Surfaces.Shamrock(400, 30).ToLines3(4).Rotate(Rotates.Z_Y);
            // Kershner8 shamrock // var shape = Parquets.PentagonalKershner8ForTube(5, 75+5, 1.6).Scale((1, 0.4/3)).Move(0, -Math.PI/12).PullOnSurface90(SurfaceFuncs.Shamrock).ToLines3(4).Rotate(Rotates.Z_Y);
            // Shell // var shape = Parquets.PentagonalKershner8ForTube(10, 75, 1.6).Scale((1, 0.8/3)).PullOnSurface90(SurfaceFuncs.Shell).ToLines3(8).Rotate(Rotates.Z_Y);
            // See Shell // var shape = Parquets.PentagonalKershner8ForTube(10, 75, 1.6).Scale((1, 1.6/3)).PullOnSurface90(SurfaceFuncs.SeeShell).ToLines3(20).Rotate(Rotates.Z_Y);
            // Dini surface // var shape = Surfaces.DiniSurface(100, 50).ToLines3(2).Rotate(Rotates.Z_Y); // var shape = Surfaces.DiniSurface(120, 30).MassCentered().Normed().Move(0, 0, 1).ToLines3(0.2, Color.Blue)
            // Mobius Strip // var shape = Surfaces.MobiusStrip(62, 10).ToLines3(2).Rotate(Rotates.Z_Y);
            // Kershner try Mobius Strip // var shape = Parquets.PentagonalKershner8ForTube(31, 10, 1.6).Scale(0.98, 1).Move(Math.PI, -1 + 0.1).PullOnSurface(SurfaceFuncs.MobiusStrip).ToLines3(1).Rotate(Rotates.Z_Y);
            // Mobius is so ...ing spectial // var shape = Surfaces.MobiusStrip(124, 20).Rotate(Rotates.Z_Y).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).ApplyZ(Funcs3Z.Hyperboloid).ToLines3(2);
            // Fractal, Tree3 // var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10).Rotate(Rotates.Z_Y);
            // Never Mind // var shape = ShapeFractals.NeverMindTree(6).Rotate(Rotates.Z_Y) + Shapes.Cube;
            // Never Mind 3D // var shape = ShapeFractals.NeverMindTree3D(5).Rotate(Rotates.Z_Y) + Shapes.Cube;
            // Parabola Tree // var shape = ShapeFractals.ParabolaTree(5).Rotate(Rotates.Z_Y) + Shapes.Cube;
            // Normal Distribution // var shape = Surfaces.NormalDistribution(30, 30, 0.6, 0, 6).ToMetaShape3(3,3);
            // Normal Distribution gradient // var shape = Surfaces.NormalDistribution(55, 55, 0.5, 10, 4).ToMetaShape3(5, 5).Rotate(Rotates.Z_Y).ApplyColorGradientY(Color.DarkRed, Color.Red, Color.White);
            // Dark Heart //var shape = Parquets.Triangles(12, 40, 0.1).Scale((Math.PI / 3.1, 3.0.Sqrt() / 1.7)).Move((Math.PI, -Math.PI / 2)).ToShape3().ToLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.HeartWrapZ).Rotate(Rotates.Z_Y).Scale(1, 1, 0.7).Rotate(Rotates.Y_mZ).ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.FromArgb(20, 20, 20), Color.Red, Color.Red);
            // Green tree // var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10, true, Color.FromArgb(0, 10, 0), Color.FromArgb(0, 50, 0)).Rotate(Rotates.Z_Y);
            // Plinom // var shape = Surfaces.Cylinder(8, 61).MassCentered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.RootPolinomY(1.0/20, new[]{ -3, -2, -0.5, 0, 1.1, 2.2, 3})) + Shapes.Cube;
            // Fourier  eagle // var shape = Polygons.FourierSeries(400, ((0.05, 0), 20), (Fourier.RotateN(1, 4), 1)).ToShape2().ToShape3().ToLines3();
            // Rainbow // var shape = Surfaces.Plane(300, 30).Move(-150, -15, 0).Mult(0.0020).ApplyFn(null, v => -v.y - v.x * v.x, v=>0.005*Math.Sin(v.x*171 + v.y*750)).ToSpots3(0.05).ApplyColorGradientZ((x, y) => -x * x - y, Color.Red, Color.Red, Color.Orange, Color.Yellow, Color.Green, Color.Blue, Color.DarkBlue, Color.Purple, Color.Purple);
            // Barnsley Fern // var shape = IteratedFunctionSystem.BarnsleyFern(20000).Select(v => v.ToV3()).ToShape().ToTetrahedronSpots3().ApplyColor(Color.Blue);
            // Quick Mandelbrot // var shape = MandelbrotFractalSystem.GetPoints(0.001, 1000).Select(v => v.ToV3()).ToShape().ToCubeSpots3(0.1).ApplyColor(Color.Blue) + Surfaces.Sphere(20, 10).MassCentered().Mult(0.1).ApplyMaterial(new Material() { Color = Color.Red });
            // Maze 5 5 5 // var shape = Mazes.CreateNet3Maze(5, 5, 5).ToCubeMetaShape3(10, 10, Color.Blue, Color.Red);
            // Kershner8 Maze // var shape = Mazes.CrateKershner8Maze(0.03, 1.7, -1.09, 5).ToMetaShape3(0.2, 0.5, Color.Blue, Color.Red);
            // Fantastic Kershner8 Maze // var shape = Mazes.CrateKershner8Maze(0.01, 1.7, -1.09, 5).Mult(3).Transform(TransformFuncs3.Flower(0.3, 0.3, 5)).ToLines3(0.2, Color.Green);
            // Kershner8 Wow Maze // var shape = Mazes.CrateKershner8Maze(0.03, 1.7, -1.09, 5).Mult(3).Transform(TransformFuncs3.Flower(0.5,0.5,5)).ToMetaShape3(0.2, 0.5, Color.Blue, Color.Red);
            // Kershner8 Wow Maze optimized // var shape = Mazes.CrateKershner8Maze(0.003, 1.7, -1.09, 5).Mult(3).Transform(TransformFuncs3.Flower(0.3, 0.3, 5)).ToTetrahedronMetaShape3(0.1, 0.3, Color.Blue, Color.Red);
            // Mobius Maze // var shape = Surfaces.MobiusStrip(128, 20).ToMaze(0, MazeType.SimpleRandom).ToLines3(2).Rotate(Rotates.Z_Y).ApplyColor(Color.FromArgb(20, 20, 20));
            // Maze with path // var (maze, path) = Parquets.PentagonalKershner8(0.01, 1.7).Rotate(-1.09).ToShape3().Mult(3).Transform(TransformFuncs3.Flower(0.3, 0.3, 5)).ToMazeWithPath(1, new[] { (6, 7), (-6, -5) });            var shape = maze.ToLines3(0.2, Color.Blue) + path.ToLines3(0.2, Color.Red);
            // Imposible maze // var (maze, path) = Parquets.PentagonalKershner8(0.002, 1.7).ToShape3().Mult(4).ToMazeWithPath(1, MazeType.SimpleRandom, new[] { (6, 7), (-6, -5) });             var enter = Surfaces.Sphere(10, 10).Mult(0.005).Move(path.Points3[0]).ApplyColor(Color.Black);            var exit = Surfaces.Sphere(10, 10).Mult(0.005).Move(path.Points3[^1]).ApplyColor(Color.Green);            var shape = maze.ToLines3(0.2, Color.Blue) + enter + exit + path.ToLines3(0.2, Color.Red); //.Transform(TransformFuncs3.Torus(1.5))
            // Gravity maze // var (maze, path) = Parquets.Squares(50, 50, 0.04).ToShape3().ApplyZ(Funcs3Z.Paraboloid).ToMazeWithPath(1, MazeType.PowerGravity);            maze = maze.Rotate(Rotates.Z_Y);           path = path.Rotate(Rotates.Z_Y);            var enter = Surfaces.Sphere(10, 10).Mult(0.01).Move(path.Points3[0]).ApplyColor(Color.Black);            var exit = Surfaces.Sphere(10, 10).Mult(0.01).Move(path.Points3[^1]).ApplyColor(Color.Green);                  var shape = maze.ToLines3(1, Color.Blue) + enter + exit + path.ToLines3(0.3, Color.Red);
            // Not bad font // var shape = Texter.GetText("This is not a 3d font\r\nbut\r\nthis is already not bad", 50).ToCubeSpots3(50).ApplyColorGradientY(Color.Red, Color.Red, Color.White);
            // LNT // var shape = Vectorizer.GetText("ВОЙНА И МИР\r\nТОМ ПЕРВЫЙ\r\nЧАСТЬ ПЕРВАЯ\r\nI", 100, "Times New Roman").ToLines3(300, Color.Red);
            // LNT2 // var shape = Vectorizer.GetText("— Eh bien, mon prince. Gênes et Lucques ne sont plus que des apanages, des\r\nпоместья, de la famille Buonaparte. Non, je vous préviens que si vous ne me dites pas\r\nque nous avons la guerre, si vous vous permettez encore de pallier toutes les infamies,\r\ntoutes les atrocités de cet Antichrist (ma parole, j'y crois) — je ne vous connais plus,\r\nvous n'êtes plus mon ami, vous n'êtes plus мой верный раб, comme vous dites.", 100, "Times New Roman").ToLines3(300, Color.Red);
            // Bird // var shape = Vectorizer.GetContentShape(settings.GetContentFileName("bird1.jpg")).ApplyZ(Funcs3Z.Paraboloid).ToLines3(0.5).ApplyColorGradientY(Color.Red, Color.Red, Color.White);
            // Iguana //var shape = Vectorizer.GetContentShape(settings.GetContentFileName("iguana1.jpg")).ApplyZ(Funcs3Z.Waves).ToLines3(0.5).ApplyColorGradientZ(Color.Black, Color.Black, Color.DarkBlue);
            // Dragon // var shape = Vectorizer.GetContentShape(settings.GetContentFileName("dragon4.jpg")).ApplyZ(Funcs3Z.Waves).ToLines3(0.2).ApplyColorGradientZ(Color.DarkRed, Color.Black, Color.DarkRed);


            // .ApplyZ(Funcs3Z.SphereR(1.2))
            // Shapes.IcosahedronSp2.Mult(0.02).ApplyColor(Color.Red)
            // Shapes.GolfBall.Move(0.7, 1.5, 2).ToLines3(1, Color.Red)
            // Vectorizer.GetContentShape(settings.GetContentFileName("s8.jpg"), 200).Where(v=>v.y>-0.45).MassCentered().Normed().ApplyZ(Funcs3Z.Waves).ToLines3(1, Color.Blue),

            var fShape = new Fr[]
            {
                //(9, 0.1), (7, 0.2),
                //(-11, 0.1), (2, 0.2),
                //(-2, 0.1), (-12, 0.2),
                //(-15, 0.1), (-7, 0.2),
                //(-2, 0.1), (-4, 0.2),
                //(1, 0.1), 
                //(-12, 1), (5, 2),
                //(20, 1), (11, 2), // plane
                //(-10, 1), (-6, 2),
                //(-9, 1), (4, 2), 
                //(-6, 1), (3, 2),
                (-11, 1), (-3, 2),
                (-6, 1), (4, 2),
                (-1, 10)
            };

            //return FourierShapes.SearchSeries(fShape, 1, 2, -20, 20, -20, 20);
            //return FourierShapes.SearchSeries(fShape, 1, 2, -10, 10, -10, 10);

            var sp = FourierShapes.Series(fShape, 0.05, 256);
            var f = FourierShapes.SeriesFunc(fShape);
            //[0].ToLines3(1, Color.Blue); //.ApplyColor(Color.Blue);//.ToLines3(1, Color.Blue);//.ApplyColor(Color.Blue);//.ToLines3(1, Color.Blue);


            //var p = Polygons.FourierSeries5(-9, 4, -7, -6, 0.1, 0.2, 0.1, 0.2, 1000, 0.1, -0.2);

            //var b = sp[5].BorderY;

            var shape = new Shape[]
            {
                sp.ToSingleShape()/*.ToLines3(1)*/.ApplyColor(Color.Blue),
                f.Perfecto(1.5).Scale(0.6, 1, 1).Move(0, 0.13, 0.025).ApplyColor(Color.Red),
                //sp[5].Move(0,-b.a,0).Rotate(Quaternion.FromRotation(Vector3.YAxis, new Vector3(0,1,1).Normalize())).Move(0,b.a,0).ApplyColor(Color.Red)
                
                //Shapes.CoodsNet
            }.ToSingleShape();//.ToLines3(1, Color.Blue);//.ApplyColor(Color.Blue);


            //var shape = FourierShapes.SearchSeries3(-10, 10, -10, 10, 0.1, 0.2, 0.1, -0.2);

            return shape;//.Rotate(Rotates.Z_Y);
        }
    }
}
