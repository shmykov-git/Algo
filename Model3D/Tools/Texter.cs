using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;

namespace Model3D.Tools
{
    public static class Texter
    {
        public static Shape GetText(string text, int fontSize = 50, string fontName = "Arial")
        {
            var lines = text.Split("\r\n").ToArray();

            var m = (int)(1.6 * fontSize * lines.Length) + 1;
            var n = (int)(1 * fontSize * lines.Max(l => l.Length)) + 1;

            using Bitmap bitmap = new Bitmap(n, m, PixelFormat.Format32bppPArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            //graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.Clear(Color.White);

            using Brush brush = new SolidBrush(Color.Black);
            //using Pen pen = new Pen(Color.Blue, 1);
            using Font arial = new Font(fontName, fontSize, FontStyle.Regular);

            Rectangle rectangle = new Rectangle(0, 0, n-1, m-1);
            //graphics.DrawRectangle(pen, rectangle);
            graphics.DrawString(text, arial, brush, rectangle);

            //bitmap.Save("DrawText.png");

            var colorLevel = 200;

            bool IsPoint((int i, int j) v)
            {
                var c = bitmap.GetPixel(v.j, v.i);
                return c.R < colorLevel && c.G < colorLevel && c.B < colorLevel;
            }

            var map = Ranges.Range(m).Select(i => Ranges.Range(n).Select(j => IsPoint((i, j))).ToArray()).ToArray();

            (int i, int j)[] insideDirs = new[] { (1, 0), (0, -1), (-1, 0), (0, 1) };
            var insidePoints = Ranges.Range(m, n).Where(v => map[v.i][v.j] && insideDirs.All(d => map[v.i + d.i][v.j + d.j])).ToArray();
            foreach (var p in insidePoints)
                map[p.i][p.j] = false;

            var nodes = Ranges.Range(m, n).Where(v=>map[v.i][v.j]).SelectWithIndex((v, k) => new
            {
                k,
                v,
                p = new Vector2(v.j, 2 * fontSize - v.i)
            }).ToArray();

            //var dic = nodes.ToDictionary(v => v.v, v => v);

            //(int i, int j)[] dirs = new[] { (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1) };

            //var edges = nodes.Select(n => n.v)
            //    .SelectMany(a => dirs.Select(d => (i: a.i + d.i, j: a.j + d.j)).Where(v => map[v.i][v.j]).Select(b => (i: dic[a].k, j: dic[b].k))
            //    .Select(v => v.i < v.j ? v : (i: v.j, j: v.i)))
            //    .Distinct()
            //    .ToArray();

            return new Shape
            {
                Points2 = nodes.Select(n => n.p).ToArray(),
                //Convexes = edges.Select(v => new int[] { v.i, v.j }).ToArray()
            };
        }
    }
}
