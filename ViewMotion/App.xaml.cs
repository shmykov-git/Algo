using System.Windows;
using Aspose.ThreeD;
using Meta;
using Meta.Tools;
using Microsoft.Extensions.DependencyInjection;
using Model.Interfaces;
using Model3D.Tools;

namespace ViewMotion;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DI.Configure(services => services
            .AddSingleton<Viewer>(p => new Viewer() {DataContext = p.GetService<ViewerModel>()})
            .AddTransient<ViewerModel>()
            .AddTransient<MotionScene>()
            .AddTransient<Settings>()
            .AddSingleton<IDirSettings>(p => p.GetService<Settings>())
            .AddSingleton<ContentFinder>()
            .AddSingleton<Scene>()
            .AddSingleton<Vectorizer>()
            .AddSingleton<ThreadPool>());
        DI.Build();

        var viewer = DI.Get<Viewer>();
        viewer.Show();
    }
}