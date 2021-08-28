using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using View3D.Libraries;

namespace View3D
{
    static class Scene3
    {
        public static Shape GetShape()
        {
            //var polygon = Sinus(3, 50);
            //var polygon = Spiral(3, 60);
            //var polygon = Elipse(1, 0.3, 30);
            //var polygon = Elipse(0.4, 1, 10);
            //var polygon = Square.PutInside(Spiral(3, 60));
            //var polygon = Square.PutInside(Square.MultOne(0.9));
            //var polygon = Polygons.Square.PutInside(Polygons.Sinus(3, 100));
            //var shape = Polygons.Square(1).PutInside(Polygons.Spiral(10, 800).Mult(1)).MakeShape().Transform(Multiplications.Cube);
            //var polygon = Polygons.Square.PutInside(Polygons.Sinus(1.7, 1.2, 3, 300));
            // var polygon = Polygons.Elipse(1, 1, 50).PutInside(Polygons.Sinus(1.7, 1.2, 3, 300).Mult(0.8));
            //var shape = Polygons.Square(1).PutInside(Polygons.Elipse(1, 1, 50).Mult(0.7)).Fill().ToShape().Transform(Transformations.Plane);
            //var shape = Shapes.Chesss(25).Mult(2).AddZVolume(1.0 / 25).ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);

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
            // Dini surface // var shape = Surfaces.DiniSurface(100, 50).ToLines3(2).Rotate(Rotates.Z_Y);
            // Mobius Strip // var shape = Surfaces.MobiusStrip(62, 10).ToLines3(2).Rotate(Rotates.Z_Y);
            // Kershner try Mobius Strip // var shape = Parquets.PentagonalKershner8ForTube(31, 10, 1.6).Scale(0.98, 1).Move(Math.PI, -1 + 0.1).PullOnSurface(SurfaceFuncs.MobiusStrip).ToLines3(1).Rotate(Rotates.Z_Y);
            // Mobius is so ...ing spectial // var shape = Surfaces.MobiusStrip(124, 20).Rotate(Rotates.Z_Y).ApplyZ(Funcs3Z.Hyperboloid).Rotate(Rotates.Z_Y).ApplyZ(Funcs3Z.Hyperboloid).ToLines3(2);

            // Fractal, Tree3 // var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10).Rotate(Rotates.Z_Y);
            // Never Mind // var shape = ShapeFractals.NeverMindTree(6).Rotate(Rotates.Z_Y) + Shapes.Cube;
            // Never Mind 3D // var shape = ShapeFractals.NeverMindTree3D(5).Rotate(Rotates.Z_Y) + Shapes.Cube;
            // Parabola Tree // var shape = ShapeFractals.ParabolaTree(5).Rotate(Rotates.Z_Y) + Shapes.Cube;
            // Normal Distribution // var shape = Surfaces.NormalDistribution(30, 30, 0.6, 0, 6).ToMetaShape3(3,3);
            // Dark Heart //var shape = Parquets.Triangles(12, 40, 0.1).Scale((Math.PI / 3.1, 3.0.Sqrt() / 1.7)).Move((Math.PI, -Math.PI / 2)).ToShape3().ToLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.HeartWrapZ).Rotate(Rotates.Z_Y).Scale(1, 1, 0.7).Rotate(Rotates.Y_mZ).ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.FromArgb(20, 20, 20), Color.Red, Color.Red);
            // Green tree // var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10, true, Color.FromArgb(0, 10, 0), Color.FromArgb(0, 50, 0)).Rotate(Rotates.Z_Y);
            // Plinom // var shape = Surfaces.Cylinder(8, 61).Centered().Scale(0.1, 0.1, 0.1).CurveZ(Funcs3.RootPolinomY(1.0/20, new[]{ -3, -2, -0.5, 0, 1.1, 2.2, 3})) + Shapes.Cube;
            // Fourier  eagle // var shape = Polygons.FourierSeries(400, ((0.05, 0), 20), (Fourier.RotateN(1, 4), 1)).ToShape2().ToShape3().ToLines3();

            // var shape = Parquets.PentagonalKershner8(0.05, 1.5).Rotate(-1.15).ToShape3().ToLines(40).AddVolumeZ(0.01);

            //var shape = Surfaces.NormalDistribution(50, 50, 0.6, 10, 3).Rotate(Rotates.Z_Y).CenteredXZ().ToMetaShape3(5, 5, Color.Red, Color.Green).ApplyColorGradientY(null, null, null, Color.White);
            //var shape2 = Surfaces.NormalDistribution(50, 50, 0.6, 10, 2).Rotate(Rotates.Z_Y).CenteredXZ().ToMetaShape3(5, 5).Move(0,0,30).ApplyMaterialGradientY(Color.Green, Color.White);

            //var shape = shape1 + shape2;

            //var shape = Parquets.Triangles(12, 40, 0.1).Scale((Math.PI / 3.1, 3.0.Sqrt() / 1.7)).Move((Math.PI, -Math.PI / 2)).ToShape3().ToLines(20).AddVolumeZ(0.1).Transform(TransformFuncs3.HeartWrapZ).Rotate(Rotates.Z_Y).Scale(1, 1, 0.7).Rotate(Rotates.Y_mZ).ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.FromArgb(20, 20, 20), Color.Red, Color.Red);

            var shape = LineFractals.Tree3.CreateFractal(6).ToShape(10, true, Color.FromArgb(0, 10, 0), Color.FromArgb(0, 50, 0)).Rotate(Rotates.Z_Y);

            //var shape = Shapes.Cube.ToMetaShape3(1, 1, Color.Red, Color.Green);

            //var poligon = Polygons.FourierSeries(400, ((0.2, 0), -6), (Fourier.RotateN(1, 4), -1));
            //var shape = poligon.PaveInside(Parquets.PentagonalKershner8(0.02, 1.5).Mult(3)).ToShape3().ToLines3()
            //    + poligon.ToShape2().ToShape3().ToLines3();

            //shape = shape + Shapes.Cube.Mult(0.2);

            return shape + Shapes.Cube.Mult(0.2).ApplyMaterial(new Material() { Color = Color.Black });
        }
    }
}
