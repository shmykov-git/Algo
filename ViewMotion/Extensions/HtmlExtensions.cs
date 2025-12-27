using Meta;
using Model;
using Model3D.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using View3D;

namespace ViewMotion.Extensions;

public static class HtmlExtensions
{
    public static void ShowShapedHtml(this IEnumerable<Shape> shapes)
    {
        var settings = DI.Get<StaticSettings>();
        var sceneHtmlFileName = Path.Combine(settings.InputHtmlDirectory, "ViewTemplate.html");
        shapes.CreateHtml(sceneHtmlFileName, settings.FullFileNameHtml);
        ShowStaticScene(settings.FullFileNameHtml);
    }

    public static void ShowShapedHtml(this Shape shape) => new[] { shape }.ShowShapedHtml();

    private static void ShowStaticScene(string fullFileName)
    {
        var process = new Process();
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = fullFileName;
        process.Start();
    }
}
