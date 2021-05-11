using Aspose.ThreeD;
using Model;
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
            var sceneManager = new SceneManager();

            try
            {
                var shape = Scene3.GetShape();

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
