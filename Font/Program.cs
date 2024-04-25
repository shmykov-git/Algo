using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using Font;
using Font.Model;
using Font.Model.Fts;
using Mapster;
using MapsterMapper;
using meta.Extensions;
using Meta;
using Meta.Extensions;
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
FontTable[] tables = tablesJson.FromJson<FontTable[]>() ?? throw new ArgumentException("Configuration error");

var family = new FontTableFamily();

var assembly  = Assembly.GetExecutingAssembly();

bool TypeCondition(Type? t)
{
    if (t == null)
        return false;

    return t == typeof(Ft) || TypeCondition(t.BaseType);
}

var typesFactory = assembly.GetTypes()
    .Where(t => !t.IsAbstract)
    .Where(TypeCondition)
    .Select(t => (Func<Ft>)(() => (Ft)Activator.CreateInstance(t)))
    .Select(m => (singleton: m(), method: m))
    .ToDictionary(v => v.singleton.Type, v => v);

void InitTable(FontTable t)
{
    family.Tables.Add(t);
    t.Family = family;
    t.Fields.ForEach(f =>
    {
        f.Ft = typesFactory[f.Type].singleton.IsSingleton
            ? typesFactory[f.Type].singleton
            : typesFactory[f.Type].method();

        if (!f.Ft.IsSingleton)
        {
            f.Ft.table = t;
            f.Ft.field = f;
        }
    });
}

//tables.ForEach(InitTable);


using var stream = File.OpenRead(@"C:\\WINDOWS\\Fonts\\Alef-Bold.ttf");
using var reader = new BinaryReader(stream);

void Read(int count, int tableIndex)
{
    if (tables.Length == tableIndex)
        return;

    for (var iterationNumber = 0; iterationNumber < count; iterationNumber++)
    {
        var t = tables[tableIndex].Clone();
        InitTable(t);
        t.TableIndex = iterationNumber;

        Debug.WriteLine("");
        Debug.WriteLine($"======= <{t.Name}_{iterationNumber}> =======");
        t.Read(reader);
        t.compactValues.ForEach(row => Debug.WriteLine(row));
        Debug.WriteLine($"======= </{t.Name}_{iterationNumber}> =======");

        if (t.ReadIterationCount > 1)
            Read(t.ReadIterationCount - 1, tableIndex + 1);
        else
            Read(1, tableIndex + 1);
    }
}

Read(1, 0);

//tables.ForEach(t =>
//{
//    Debug.WriteLine("");
//    Debug.WriteLine($"======= <{t.Name}> =======");
//    t.Read(reader);
//    t.compactValues.ForEach(row => Debug.WriteLine(row));
//    var iterationCount = t.ReadIterationCount;


//    Debug.WriteLine($"======= </{t.Name}> =======");
//});

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



