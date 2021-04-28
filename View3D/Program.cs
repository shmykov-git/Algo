using Aspose.ThreeD;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Diagnostics;
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
                //var poligon = Sinus(3, 50);
                //var poligon = Spiral(3, 60);
                //var poligon = Elipse(1, 0.3, 30);
                //var poligon = Elipse(0.4, 1, 10);
                //var poligon = Square.PutInside(Spiral(3, 60));
                //var poligon = Square.PutInside(Square.MultOne(0.9));
                //var poligon = Poligons.Square.PutInside(Poligons.Sinus(3, 100));
                //var poligon = Poligons.Square.PutInside(Poligons.Spiral(15, 1000).Mult(1.23));
                //var poligon = Poligons.Square.PutInside(Poligons.Sinus(1.7, 1.2, 3, 300));
                // var poligon = Poligons.Elipse(1, 1, 50).PutInside(Poligons.Sinus(1.7, 1.2, 3, 300).Mult(0.8));
                //var shape = Poligons.Square(1).PutInside(Poligons.Elipse(1, 1, 50).Mult(0.7)).Fill().ToShape().Transform(Transformations.Plane);
                //var shape = Shapes.Chesss(25).Mult(2).AddZVolume(1.0 / 25).ApplyZ(Funcs3.Hyperboloid).Rotate(Rotates.Z_Y);
                var shape = Poligons.Elipse(1, 1, 50).PutInside(Poligons.Spiral(15, 1000).Mult(1.23)).MakeShape().Transform(Transformations.Cube).ToSphere();

                var scene = sceneManager.CreateScene(shape);

                scene.Save(settings.FbxFullFileName, FileFormat.FBX7700Binary);

                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = settings.FbxFullFileName;
                process.Start();
            }
            catch(PologonFillException)
            {
                Console.WriteLine($"Incorrect construction. Use View (2D) to fix it!");
            }            
        }
    }
}
