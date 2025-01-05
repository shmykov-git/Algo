using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meta;
using Model;
using View3D;
using Model3D.Extensions;

namespace ViewMotion.Extensions;

public static class HtmlExtensions
{
    public static void ShowShapedHtml(this IEnumerable<Shape> shapes)
    {
        var settings = DI.Get<StaticSettings>();
        var sceneHtmlFileName = Path.Combine(settings.InputHtmlDirectory, "ViewTemplate.html");
        shapes.CreateShapedHtml(sceneHtmlFileName, settings.FullFileNameHtml);
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
