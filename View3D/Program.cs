using Aspose.ThreeD;
using Model;
using Model3D.Tools;
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
                var sw = Stopwatch.StartNew();
                var shape = Scene3.GetShape(settings);
                var scene = sceneManager.CreateScene(shape);
                sw.Stop();

                Console.WriteLine($"Scene generation time {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");

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
