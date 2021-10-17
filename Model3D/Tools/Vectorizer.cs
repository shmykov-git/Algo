using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using Model.Graphs;

namespace Model3D.Tools
{
    public static class Vectorizer
    {
        private static Bitmap GetTextBitmap(string text, int fontSize = 50, string fontName = "Arial")
        {
            var lines = text.Split("\r\n").ToArray();

            var m = (int)(1.6 * fontSize * lines.Length) + 1;
            var n = (int)(1 * fontSize * lines.Max(l => l.Length)) + 1;

            Bitmap bitmap = new Bitmap(n, m, PixelFormat.Format32bppPArgb);
            using Graphics graphics = Graphics.FromImage(bitmap);
            //graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            graphics.Clear(Color.White);

            using Brush brush = new SolidBrush(Color.Black);
            //using Pen pen = new Pen(Color.Blue, 1);
            using Font arial = new Font(fontName, fontSize, FontStyle.Regular);

            Rectangle rectangle = new Rectangle(0, 0, n - 1, m - 1);
            //graphics.DrawRectangle(pen, rectangle);
            graphics.DrawString(text, arial, brush, rectangle);

            //bitmap.Save("DrawText.png");

            return bitmap;
        }

        private static bool[][] GetMapFromBitmap(Bitmap bitmap, int colorLevel = 200)
        {
            var n = bitmap.Width;
            var m = bitmap.Height;

            bool IsPoint((int i, int j) v)
            {
                var c = bitmap.GetPixel(v.j, v.i);
                return c.R < colorLevel && c.G < colorLevel && c.B < colorLevel;
            }

            var map = Ranges.Range(m).Select(i => Ranges.Range(n).Select(j => IsPoint((i, j))).ToArray()).ToArray();

            return map;
        }

        private static Shape GetShapeFromMap(bool[][] map)
        {
            var n = map[0].Length;
            var m = map.Length;

            (int i, int j) GetDir((int i, int j) a, (int i, int j) b) => (b.i - a.i, b.j - a.j);

            (int i, int j)[] insideDirs = new[] { (1, 0), (0, -1), (-1, 0), (0, 1) };
            var insidePoints = Ranges.Range(m, n).Where(v => map[v.i][v.j] && insideDirs.All(d => map[v.i + d.i][v.j + d.j])).ToArray();
            foreach (var p in insidePoints)
                map[p.i][p.j] = false;

            var nodes = Ranges.Range(m, n).Where(v => map[v.i][v.j]).SelectWithIndex((v, k) => new
            {
                k,
                v,
                p = new Vector2(v.j, m - 1 - v.i)
            }).ToArray();

            var dic = nodes.ToDictionary(v => v.v, v => v);

            (int i, int j)[] dirs = new[] { (1, 0), (1, -1), (0, -1), (-1, -1), (-1, 0), (-1, 1), (0, 1), (1, 1) };

            var edges = nodes.Select(n => n.v)
                .SelectMany(a => dirs.Select(d => (i: a.i + d.i, j: a.j + d.j)).Where(v => map[v.i][v.j]).Select(b => (i: dic[a].k, j: dic[b].k))
                .Select(v => v.i < v.j ? v : (i: v.j, j: v.i)))
                .Distinct()
                .ToList();

            var g = new Graph(edges);

            //foreach (var v in g.FullVisit())
            //    Debug.WriteLine(nodes[v.i].v);

            // todo: все еще есть точки для оптимизации
            // todo: 3d шрифт
            var delNodes1 = g.FullPathBiVisit().SelectCircleTriple((a, b, c) => (n: b, del: b.edges.Count == 2 && GetDir(nodes[a.i].v, nodes[b.i].v) == GetDir(nodes[b.i].v, nodes[c.i].v))).Where(v => v.del).Select(v => v.n).ToArray();
            var delNodes2 = g.FullPathBiVisit().SelectCircleGroup(5, g => (n: g[2], del: g[2].edges.Count == 2 && GetDir(nodes[g[0].i].v, nodes[g[2].i].v) == GetDir(nodes[g[2].i].v, nodes[g[4].i].v))).Where(v => v.del).Select(v => v.n).ToArray();
            var delNodes3 = g.FullPathBiVisit().SelectCircleGroup(7, g => (n: g[3], del: g[3].edges.Count == 2 && GetDir(nodes[g[0].i].v, nodes[g[3].i].v) == GetDir(nodes[g[3].i].v, nodes[g[6].i].v))).Where(v => v.del).Select(v => v.n).ToArray();
            var delNodes = delNodes1.Concat(delNodes2).Concat(delNodes3).Distinct().ToArray();
            foreach (var node in delNodes)
                g.TakeOutNode(node);

            //var delNodes2 = g.FullVisit().SelectCircleGroup(5, g => (ns: new[] { g[1], g[2], g[3] }, del: g[1].edges.Count == 2 && g[2].edges.Count == 2 && g[3].edges.Count == 2 && GetDir(nodes[g[0].i].v, nodes[g[2].i].v) == GetDir(nodes[g[2].i].v, nodes[g[4].i].v))).Where(v => v.del).SelectMany(v => v.ns).Distinct().ToArray();

            //var delNodes2 = g.FullVisit().SelectCircleGroup(5, g => (n: g[2], del: g[2].edges.Count == 2 && GetDir(nodes[g[0].i].v, nodes[g[2].i].v) == GetDir(nodes[g[2].i].v, nodes[g[4].i].v))).Where(v => v.del).Select(v => v.n).ToArray();
            //foreach (var node in delNodes2)
            //    g.TakeOutNode(node);

            //var delNodes = delNodes1.Concat(delNodes2).Distinct().ToArray();
            //foreach (var node in delNodes)
            //    g.TakeOutNode(node);

            var bi = g.GetBackIndices();

            return new Shape
            {
                Points2 = g.Nodes.Select(i => nodes[i].p).ToArray(),
                Convexes = g.Edges.Select(v => new int[] { bi[v.i], bi[v.j] }).ToArray()
            };
        }

        public static Shape GetText(string text, int fontSize = 50, string fontName = "Arial")
        {
            using var bitmap = GetTextBitmap(text, fontSize, fontName);
            var map = GetMapFromBitmap(bitmap);
            var shape = GetShapeFromMap(map);

            return shape;
        }

        public static Shape GetContentShape(string fileName)
        {
            using var bitmap = new Bitmap(fileName);
            var map = GetMapFromBitmap(bitmap);
            var shape = GetShapeFromMap(map);

            var radius = shape.GetRadius();

            return shape.Centered().Mult(0.5/ radius);
        }
    }
}
