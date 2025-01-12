using Aspose.ThreeD.Utilities;
using Model3D;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using ViewMotion.Models;
using Model3D.Extensions;
using ViewMotion.Extensions;

namespace ViewMotion.Worlds;

internal static class HtmlWorlds
{
    public static Task<Motion> CubeMazeWorld()
    {
        var n = 4;
        var nScale = 4;
        var (maze, holes, path) = Mazes.CreateNet3MazeBox(n, n, n, true, new[] { (n / 2 - 1, -1, n / 2 - 1), (n / 2, n, n / 2) }, 1);
        //var (maze, holes, path) = Mazes.CreateNet3MazeBox(n, n, n, true, new[] { (0, 0, n), (n - 2, n, n - 2) }, 1);

        var center = 0.5 * new Vector3(n - 1, n - 1, n - 1);
        var sMaze = maze.Move(-center).Mult(1.0 / nScale);
        var sHoles = holes.Move(-center).Mult(1.0 / nScale);
        var sPath = path.Move(-center).Mult(1.0 / nScale);

        Debug.WriteLine($"\r\n=== <js holes>\r\n{sHoles.Get_js_object_data()}\r\n=== </js holes>\r\n");
        Debug.WriteLine($"\r\n=== <js path>\r\n{sPath.Get_js_object_data()}\r\n=== </js path>\r\n");
        Debug.WriteLine($"\r\n=== <js>\r\n{sMaze.Get_js_object_data()}\r\n=== </js>\r\n");

        return new[]
        {
            sMaze.ApplyColor(Color.FromArgb(50, Color.Blue)),
            sHoles.ApplyColor(Color.Green),
            sPath.ApplyColor(Color.Red),
        }.ToSingleShape().ToMotion();

        return new[]
        {
            sMaze.ToLines(0.2, Color.Blue),
            sHoles.ApplyColor(Color.Green),
            sPath.ToLines(0.4, Color.Red)
        }.ToSingleShape().ToMotion();
    }
}
