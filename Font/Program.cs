using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
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
FontTable root = tablesJson.FromJson<FontTable>() ?? throw new ArgumentException("Configuration error");




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

FontTable CreateActiveTable(FontTable table, FontTable? activeParent)
{
    var activeTable = table.Clone();
    activeTable.ActiveTables = new();
    activeTable.ParentTable = activeParent;
    activeTable.Tables ??= new FontTable[0];
    activeTable.Fields ??= new FontField[0];    

    activeParent?.ActiveTables.Add(activeTable);

    activeTable.Fields.ForEach(f =>
    {
        f.Ft = typesFactory[f.Type].singleton.IsSingleton
            ? typesFactory[f.Type].singleton
            : typesFactory[f.Type].method();

        if (!f.Ft.IsSingleton)
        {
            f.Ft.table = activeTable;
            f.Ft.field = f;
        }
    });

    return activeTable;
}




using var stream = File.OpenRead(@"C:\\WINDOWS\\Fonts\\Alef-Bold.ttf");
using var reader = new BinaryReader(stream);


string Read(FontTable activeTable, int parentRowNumber)
{    
    var tableDebug = new StringBuilder();

    var level = activeTable.Level;
    var shift = new string(' ', 2 * activeTable.Level);
    tableDebug.AppendLine($"{shift}<{activeTable.FullName}_{parentRowNumber}> |{reader.BaseStream.Position}");

    var rowDebug = new StringBuilder();

    activeTable.Read(reader, parentRowNumber, rowDebug, rowNumber =>
    {
        foreach (var child in activeTable.Tables)
        {
            var activeChild = CreateActiveTable(child, activeTable);
            var rowText = Read(activeChild, rowNumber);

            if (rowText.HasText())
            {
                if (rowNumber > 0) 
                    rowDebug.AppendLine();

                rowDebug.Append(rowText);
            }
        }
    });

    tableDebug.Append(rowDebug);    
    activeTable.compactValues.Where(row => row.HasText()).ForEach(row => tableDebug.AppendLine($"{shift}  {row}"));
    tableDebug.AppendLine($"{shift}</{activeTable.FullName}_{parentRowNumber}> |{reader.BaseStream.Position}");

    var hasData = activeTable.compactValues.Length > 0 && !activeTable.Hide;

    return hasData ? tableDebug.ToString() : "";
}

var activeRoot = CreateActiveTable(root, null);

Debug.WriteLine("============");
var text = Read(activeRoot, 0);
Debug.Write(text);
Debug.WriteLine("============");

var stop = 1;

//var vectorizer = DI.Get<Vectorizer>();

//var options = new BezierTextOptions()
//{
//    FontSize = 200,
//    FontName = "Times New Roman"
//};

//new Mapper().Map(BezierValues.PerfectLetterOptions, options);

//var bzs = vectorizer.GetTextBeziers("a", options);



