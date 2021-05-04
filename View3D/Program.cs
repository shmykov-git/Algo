using Aspose.ThreeD;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Diagnostics;
using System.Linq;
using View3D.Libraries;
using View3D.Tools;

namespace View3D
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = new Settings();
            var sceneManager = new SceneManger();

            try
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
                //var shape = Polygons.Elipse(1, 1, 50).PutInside(Polygons.Spiral(15, 1000).Mult(1.23)).MakeShape().Transform(Multiplications.Cube).ToSphere();
                // Sphere Heart //var shape = Polygons.Spiral(25, 4000).Mult(1.23).MakeShape().Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);
                // Arabica Heart // var shape = Polygons.Spiral(25, 4000).Mult(1.23).MakeShape().ApplyZ(Funcs3.Hyperboloid).Transform(Multiplications.MoveY).Scale(1, 0.5, 1).Transform(TransformFuncs3.Heart()).Scale(0.7, 1, 1).AddVolume(0.001, 0, 0).Rotate(Rotates.Z_Y);

                //var shape = Polygons.Heart(1, 1, 50).MakeShape();
                //.PutInside(Polygons.Spiral(10, 500).Mult(0.3).Move((0.13, 0.21)))
                //.PutInside(Polygons.Spiral(10, 500).Mult(0.3).Move((-0.13, 0.21))).MakeShape(0.1).ApplyZ(Funcs3.Paraboloid)

                //var shape = new Shape
                //{
                //    Points = Polygons.Polygon5.Points.Select(p => p.ToV4()).ToArray(),
                //    Convexes = Aspose.ThreeD.Entities.PolygonModifier.Triangulate(Polygons.Polygon5.Points.Select(p => p.ToV4()).ToArray())
                //}.ToMetaShape();

                //var shape = Polygons.Elipse(1, 0.4, 16).MakeTriangulatedShape(0.1).ToMetaShape();
                //var shape = Polygons.Heart(1, 1, 50).MakeTriangulatedShape(0.05).ToMetaShape();
                //var shape = Polygons.Square(1).MakeTriangulatedShape(0.6).ToMetaShape();


                var shape = Parquets.Triangles(0.25).ToShape3().ToMetaShape();

                var scene = sceneManager.CreateScene(shape);

                scene.Save(settings.FbxFullFileName, FileFormat.FBX7700Binary);

                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = settings.FbxFullFileName;
                process.Start();
            }
            catch(PolygonFillException)
            {
                Console.WriteLine($"Incorrect construction. Use View (2D) to fix it!");
            }            
        }
    }
}
