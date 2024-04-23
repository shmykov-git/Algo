using Font;
using Mapster;
using MapsterMapper;
using Meta;
using Microsoft.Extensions.DependencyInjection;
using Model.Interfaces;
using Model3D.Tools;
using Model3D.Tools.Model;
using Model3D.Tools.Vectorization;
using ThreadPool = Meta.Tools.ThreadPool;

DI.Configure(services => services
    .AddSingleton<FontSettings>()
    .AddSingleton<IDirSettings>(p => p.GetService<FontSettings>())
    .AddSingleton<ContentFinder>()
    .AddSingleton<Vectorizer>()
    .AddSingleton<ThreadPool>());

using var serviceProvider = DI.Build();

var vectorizer = DI.Get<Vectorizer>();



var options = new BezierTextOptions()
{
    FontSize = 200,
    FontName = "Times New Roman"
};

new Mapper().Map(BezierValues.PerfectLetterOptions, options);

var bzs = vectorizer.GetTextBeziers("a", options);

var stop = 1;

