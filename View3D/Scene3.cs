using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
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
            // Arabica Heart // var shape = Polygons.Spiral(25, 4000).Mult(1.23).MakeShape().ApplyZ(Funcs3.Hyperboloid).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);
            // Snake Heart // var shape = Polygons.Spiral(25, 4000).Mult(1.23).Mult(2).MakeShape(true).ApplyZ(Funcs3.Hyperboloid).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);
            // Kershner Heart // var shape = Polygons.Square.PaveInside(Parquets.PentagonalKershner8(0.02, 1.5)).ToShape3().ToMetaShape(0.4, 40).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).Rotate(Rotates.Z_Y);
            // Saddle Net // var shape = Parquets.Triangles(0.1).ToShape3().Mult(2).ToMetaShape().ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
            // Saddle Hexagon Net // var shape = Parquets.Hexagon(0.1).ToShape3().Mult(2).ToMetaShape().ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
            // VualLy //var shape = Parquets.PentagonalKershner8(0.05, 1.5).ToShape3().ToMetaShape(0.5, 20).ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
            // NeedToCut // var polygon = Polygons.Heart(1, 1, 50).Move((0,-0.1)).Mult(1.2); var shape = Paver.Pave(polygon, Parquets.PentagonalKershner8(0.03, 1.5).Mult(1)).Join(polygon.ToShape2()).ToShape3().ToMetaShape(0.5, 20).ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
            // Print, Kershner8 // var shape = Parquets.PentagonalKershner8(0.075, 1.5).Rotate(-1.15).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
            // Print, Kershner8MugStand // var shape = Polygons.Elipse(1, 1, 50).PaveInside(Parquets.PentagonalKershner8(0.03, 1.5).Mult(1.5)).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
            // Print, SpiralMugStand // var shape = Polygons.Elipse(1, 1, 100).PutInside(Polygons.Spiral(25, 6000).Mult(1.25)).MakeShape().AddVolumeZ(0.01);
            // Print, Kershner8HeartMugStand // var shape = Polygons.Heart(1, 1, 100).PaveInside(Parquets.PentagonalKershner8(0.01, 1.9).Mult(1.5)).ToShape3().ToMetaShape(0.2,50).AddVolumeZ(0.01);

            //var shape = Polygons.Heart(1, 1, 50)
            //    .PutInside(Polygons.Spiral(10, 500).Mult(0.3).Move((0.13, 0.21)))
            //    .PutInside(Polygons.Spiral(10, 500).Mult(0.3).Move((-0.13, 0.21)))
            //    .MakeShape().ApplyZ(Funcs3.Paraboloid);

            //var shape = new Shape
            //{
            //    Points = Polygons.Polygon5.Points.Select(p => p.ToV4()).ToArray(),
            //    Convexes = Aspose.ThreeD.Entities.PolygonModifier.Triangulate(Polygons.Polygon5.Points.Select(p => p.ToV4()).ToArray())
            //}.ToMetaShape();

            //var shape = Polygons.Elipse(1, 0.4, 16).MakeTriangulatedShape(0.1).ToMetaShape();
            //var shape = Polygons.Heart(1, 1, 50).MakeTriangulatedShape(0.05).ToMetaShape();
            //var shape = Polygons.Square(1).MakeTriangulatedShape(0.6).ToMetaShape();

            //var shape = Parquets.PentagonalKershner8(0.05, 1).ToShape3().ToMetaShape(0.5, 20).Rotate(Rotates.Z_Y); //.ApplyZ(Funcs3.Paraboloid)
            //var shape = Polygons.Square(1).PutInside(Polygons.Spiral(10, 800).Mult(1)).MakeShape().Transform(Multiplications.Cube);

            //var polygon = Polygons.Heart(1, 1, 100).Move((0,-0.1)).Mult(1.2); // Parquets.PentagonalKershner8(0.03, 1.5) .ToMetaShape(0.5, 20); .Join(polygon.ToShape2())
            //var shape = Polygons.Heart(1, 1, 100).PaveExactInside(Parquets.Triangles(0.03)).ToShape3().Mult(4).ApplyZ(Funcs3.Hyperboloid);//.Rotate(Rotates.Z_Y);//.ToMetaShape(0.5, 20); //.ApplyZ(Funcs3.Waves).Rotate(Rotates.Z_Y);

            //var shape = Polygons.Elipse(1, 1, 50).PaveInside(Parquets.PentagonalKershner8(0.03, 1.5).Mult(1.5)).ToShape3().ToMetaShape(1, 20).AddVolumeZ(0.01);
            //var shape = Polygons.Elipse(1, 1, 100).PutInside(Polygons.Spiral(25, 6000).Mult(1.25)).MakeShape().AddVolumeZ(0.01);
            var shape = Polygons.Heart(1, 1, 100).PaveInside(Parquets.PentagonalKershner8(0.01, 1.9).Mult(1.5)).ToShape3().ToMetaShape(0.2,50).AddVolumeZ(0.01);

            return shape;
        }
    }
}
