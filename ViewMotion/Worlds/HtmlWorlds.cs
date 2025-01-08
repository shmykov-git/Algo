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
        var (maze, holes, path) = Mazes.CreateNet3MazeBox(n, n, n, true, new[] { (1, -1, 1), (n - 2, n, n - 2) }, 1);
        //var (maze, holes, path) = Mazes.CreateNet3MazeBox(n, n, n, true, new[] { (0, 0, n), (n - 2, n, n - 2) }, 1);
        var center = 0.5 * new Vector3(n - 1, n - 1, n - 1);
        var s = maze.Move(-center).Mult(1.0 / n).ApplyColor(Color.FromArgb(5, Color.Blue));
        var sH = holes.Move(-center).Mult(1.0 / n).ApplyColor(Color.FromArgb(255, Color.Green));
        var sP = path.Move(-center).Mult(1.0 / n).ApplyColor(Color.FromArgb(255, Color.Red));

        Debug.WriteLine($"\r\n=== <js>\r\n{s.Get_js_object_data()}\r\n=== </js>\r\n");
        //var g = s.EdgeGraph;

        return (s.ToLines(0.2, Color.Blue) + sH + sP.ToLines(0.4, Color.Red)).ToMotion();
    }
}
