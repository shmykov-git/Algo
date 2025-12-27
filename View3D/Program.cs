using Meta;
using Meta.Tools;
using Microsoft.Extensions.DependencyInjection;
using Model;
using Model.Interfaces;
using Model3D.Tools;
using Model3D.Tools.Vectorization;
using System;
using System.Diagnostics;
using View3D.Tools;

namespace View3D
{
    class Program
    {
        static void Main(string[] args)
        {
            DI.Configure(services => services
                .AddSingleton<StaticSettings>()
                .AddSingleton<IDirSettings>(p => p.GetService<StaticSettings>())
                .AddSingleton<StaticSceneRender>()
                .AddSingleton<ContentFinder>()
                .AddSingleton<Scene>()
                .AddSingleton<Vectorizer>()
                .AddSingleton<ThreadPool>());

            using var serviceProvider = DI.Build();

            var settings = DI.Get<StaticSettings>();
            var staticSceneViewer = DI.Get<StaticSceneRender>();
            var scene = DI.Get<Scene>();

            try
            {
                var sw = Stopwatch.StartNew();
                var shape = scene.GetShape();
                var meshedScene = staticSceneViewer.CreateScene(shape);
                sw.Stop();

                Console.WriteLine($"Scene generation time {TimeSpan.FromMilliseconds(sw.ElapsedMilliseconds)}");

                meshedScene.Save(settings.FullFileName3D, settings.Format);

                var process = new Process();
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.FileName = settings.FullFileName3D;
                process.Start();
            }
            catch (PolygonFillException)
            {
                Console.WriteLine($"Incorrect construction. Use View (2D) to fix it!");
            }
        }
    }
}
