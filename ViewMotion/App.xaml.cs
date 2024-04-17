using System.Threading;
using System.Windows;
using Aspose.ThreeD;
using Mapster;
using Meta;
using Meta.Tools;
using Microsoft.Extensions.DependencyInjection;
using Model.Interfaces;
using Model3D.Tools;
using Model3D.Tools.Vectorization;
using View3D.Tools;

namespace ViewMotion;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

        DI.Configure(services => services
            .AddSingleton<Viewer>(p => new Viewer() { DataContext = p.GetService<ViewerModel>() })
            .AddTransient<ViewerModel>()
            .AddTransient<SceneMotion>()
            .AddTransient<MotionSettings>()
            .AddSingleton<IDirSettings>(p => p.GetService<MotionSettings>())
            .AddSingleton<ContentFinder>()
            .AddTransient<View3D.StaticSettings>()
            .AddSingleton<StaticSceneRender>()
            .AddSingleton<Scene>()
            .AddSingleton<Vectorizer>()
            .AddSingleton<Meta.Tools.ThreadPool>());
        DI.Build();

        var viewer = DI.Get<Viewer>();
        viewer.Show();
    }
}