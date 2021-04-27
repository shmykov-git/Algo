using Aspose.ThreeD;
using Model.Extensions;
using Model.Libraries;
using System;
using System.Diagnostics;
using View3D.Tools;

namespace View3D
{
    class Program
    {
        static void Main(string[] args)
        {
            //var poligon = Sinus(3, 50);
            //var poligon = Spiral(3, 60);
            //var poligon = Poligon3;
            //var poligon = Poligon5;
            //var poligon = Elipse(1, 0.3, 30);
            //var poligon = Elipse(0.4, 1, 10);
            //var poligon = Square.PutInside(Spiral(3, 60));
            //var poligon = Square.PutInside(Square.MultOne(0.9));
            //var poligon = Poligons.Square.PutInside(Poligons.Sinus(3, 100));
            var poligon = Poligons.Elipse(1, 1, 50).PutInside(Poligons.Spiral(15, 1000).Mult(1.23));

            var settings = new Settings();
            var adapter = new SceneAdapter();
            var poligonInfo = poligon.Fill();

            if (poligonInfo.IsValid)
            {
                var scene = adapter.CreateScene(poligonInfo);

                scene.Save(settings.FbxFullFileName, FileFormat.FBX7700Binary);

                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = settings.FbxFullFileName;
                process.Start();
            }
            else
            {
                Console.WriteLine($"Incorrect construction. Use View (2D) to fix it!");
            }
        }
    }
}
