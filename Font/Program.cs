using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using Font;
using Font.Model;
using Mapster;
using MapsterMapper;
using meta.Extensions;
using Meta;
using Microsoft.Extensions.DependencyInjection;
using Model.Extensions;
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




var tablesJson = File.ReadAllText("tables.json");
var tables = tablesJson.FromJson<FontTable[]>();

var family = new FontTableFamily()
{
    Tables = tables
};

var assembly  = Assembly.GetExecutingAssembly();
var fTypes = assembly.GetTypes()
    .Where(t => !t.IsAbstract)
    .Where(t => t.BaseType == typeof(Ft) || t.BaseType?.BaseType == typeof(Ft) || t.BaseType?.BaseType?.BaseType == typeof(Ft))
    .Select(t => (Ft)Activator.CreateInstance(t))
    .ToDictionary(v => v.Type, v => v);

tables.ForEach(t =>
{
    t.Family = family;
    t.Fields.ForEach(f =>
    {
        f.Ft = fTypes[f.Type];
    });
});


using var stream = File.OpenRead(@"C:\\WINDOWS\\Fonts\\Alef-Bold.ttf");
using var reader = new BinaryReader(stream);

tables.ForEach(t => t.Read(reader));

tables.ForEach(t =>
{
    Debug.WriteLine("");
    Debug.WriteLine($"======= <{t.Name}> =======");
    t.compactValues.ForEach(row => Debug.WriteLine(row));
    Debug.WriteLine($"======= </{t.Name}> =======");
});

//cmap тут

var stop = 1;





//var vectorizer = DI.Get<Vectorizer>();

//var options = new BezierTextOptions()
//{
//    FontSize = 200,
//    FontName = "Times New Roman"
//};

//new Mapper().Map(BezierValues.PerfectLetterOptions, options);

//var bzs = vectorizer.GetTextBeziers("a", options);



