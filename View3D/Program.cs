using Model;
using System;
using System.Diagnostics;
using Meta;
using Meta.Tools;
using Microsoft.Extensions.DependencyInjection;
using Model.Interfaces;
using Model3D.Tools;
using View3D.Tools;

namespace View3D
{
    class Program
    {
        static void Main(string[] args)
        {
            DI.Configure(services => services
                .AddSingleton<Settings>()
                .AddSingleton<IDirSettings>(p => p.GetService<Settings>())
                .AddSingleton<StaticSceneRender>()
                .AddSingleton<ContentFinder>()
                .AddSingleton<Scene>()
                .AddSingleton<Vectorizer>()
                .AddSingleton<ThreadPool>());
            
            using var serviceProvider = DI.Build();

            var settings = DI.Get<Settings>();
            var staticSceneViewer = DI.Get<StaticSceneRender>();
            var scene = DI.Get<Scene>();

            try
            {
                var sw = Stopwatch.StartNew();
                var shape = scene.GetShape();
                var meshedScene = staticSceneViewer.CreateScene(shape);
                sw.Stop();

                Console.WriteLine($"Scene generation time {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");

                meshedScene.Save(settings.FullFileName, settings.Format);

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
