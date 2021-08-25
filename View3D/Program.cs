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
                var views = Scene3.GetShapeViews();

                var scene = sceneManager.CreateScene(views);

                scene.Save(settings.FullFileName, settings.Format);

                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = settings.FullFileName;
                process.Start();
            }
            catch(PolygonFillException)
            {
                Console.WriteLine($"Incorrect construction. Use View (2D) to fix it!");
            }            
        }
    }
}
