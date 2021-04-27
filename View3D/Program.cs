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
            var poligon = Poligons.Square.PutInside(Poligons.Sinus(3, 100));

            var settings = new Settings();
            var adapter = new SceneAdapter();

            var scene = adapter.CreateScene(poligon.Fill());

            scene.Save(settings.FbxFullFileName, FileFormat.FBX7500ASCII);

            var process = new Process();
            process.StartInfo.UseShellExecute = true;
            process.StartInfo.FileName = settings.FbxFullFileName;
            process.Start();
        }
    }
}
