using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MathNet.Numerics;
using Meta.Model;
using Model.Graphs;
using Model.Interfaces;

namespace Model3D.Tools
{
    public class Vectorizer
    {
        private readonly ContentFinder contentFinder;

        public Vectorizer(ContentFinder contentFinder)
        {
            this.contentFinder = contentFinder;
        }

        private Bitmap GetTextBitmap(string text, int fontSize = 50, string fontName = "Arial", double multY = 1, double multX = 1)
        {
            var lines = text.Split("\r\n").ToArray();

            var m = (int)(multY * 1.6 * fontSize * lines.Length) + 1;
            var n = (int)(multX * fontSize * lines.Max(l => l.Length)) + 1;

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

        [Flags]
        enum Mp : byte
        {
            None = 0,
            Locked = 1,     // cannot visit this point anymore
            IsBlack = 2,    // black or white
            Left = 4,       // visited from left
            Top = 8,        // visited from top
            Right = 16,     // visited from right
            Bottom = 32,    // visisted from bottom
            IsEven = 64,    // perimeter level is even
            IsPerimeter = 128, // point marked as perimeter
        }

        private Mp[][] GetPerimetersMapFromBitmap(Bitmap bitmap, int colorLevel = 200)
        {
            var n = bitmap.Width;
            var m = bitmap.Height;

            Mp GetPoint((int i, int j) v)
            {
                if (v.i < 0 || v.i >= m || v.j < 0 || v.j >= n)
                    return Mp.Locked;

                var c = bitmap.GetPixel(v.j, v.i);
                var isBlack = c.R < colorLevel && c.G < colorLevel && c.B < colorLevel;
                
                return isBlack ? Mp.IsBlack : Mp.None;
            }

            var map = Ranges.Range(m + 2).Select(i => Ranges.Range(n + 2).Select(j => GetPoint((i - 1, j - 1))).ToArray()).ToArray();

            return map;
        }


        private List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> GetPerimetersTreeFromMap(Mp[][] map)
        {
            var n = map[0].Length;
            var m = map.Length;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            Mp GetMpDir((int i, int j) dir) =>
                dir switch
                {
                    (0, 0) => Mp.None,
                    (-1, 0) => Mp.Left,
                    (0, 1) => Mp.Top,
                    (1, 0) => Mp.Right,
                    (0, -1) => Mp.Bottom,
                    _ => throw new ArgumentException(dir.ToString())
                };

            (int i, int j)[] nearDirs = new[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
            var nearFlagDirs = nearDirs.Select(d => (d, f: GetMpDir(d))).ToArray();
            (int i, int j)[] farDirs = new[] { (1, -1), (-1, -1), (-1, 1), (1, 1) };
            (int i, int j)[] dirs = new[] { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            (int i, int j)[] GetMpDirs(Mp v) => nearFlagDirs.Where(fd => v.HasFlag(fd.f)).Select(fd => fd.d).ToArray();
            
            (int i, int j)[] GetDirs((int i, int j) v) => GetMpDirs(map[v.i][v.j]);
            
            (int i, int j)[] GetBounds((int i, int j) v) => GetDirs(v).Select(d=>v.Add(d)).ToArray();

            bool IsNearClose((int i, int j) a, (int i, int j) b) => nearDirs.Any(d => a.Add(d) == b);
            bool IsClose((int i, int j) a, (int i, int j) b) => a == b || dirs.Any(d => a.Add(d) == b);

            var stack = new Stack<((int i, int j) from, (int i, int j) to)>(3 * (n + m));

            HashSet<(int i, int j)> FindPerimeter(int level, (int i, int j)[] startPoints) => level.IsEven()
                ? FindInnerPerimeter(startPoints)
                : FindOutterPerimeter(startPoints);

            HashSet<(int i, int j)> FindOutterPerimeter((int i, int j)[] startPoints)
            {
                startPoints.ForEach(p => stack.Push((p, p)));

                var perimeter = new HashSet<(int i, int j)>();

                while (stack.Count > 0)
                {
                    var (prevP, p) = stack.Pop();

                    if (map[p.i][p.j].HasFlag(Mp.Locked))
                        continue;

                    if (map[p.i][p.j].HasFlag(Mp.IsBlack))
                    {
                        map[p.i][p.j] |= Mp.IsPerimeter | GetMpDir(prevP.Sub(p));
                        perimeter.Add(p);
                    }
                    else
                    {
                        map[p.i][p.j] |= Mp.Locked;

                        foreach (var d in nearDirs)
                        {
                            var s = p.Add(d);

                            if (!map[s.i][s.j].HasFlag(Mp.Locked))
                                stack.Push((p, s));
                        }
                    }
                }

                return perimeter;
            }

            HashSet<(int i, int j)> FindInnerPerimeter((int i, int j)[] startPoints)
            {
                startPoints.ForEach(p => stack.Push((p,p)));

                var perimeter = new HashSet<(int i, int j)>();

                while (stack.Count > 0)
                {
                    var (prevP, p) = stack.Pop();

                    if (map[p.i][p.j].HasFlag(Mp.Locked))
                        continue;

                    if (map[p.i][p.j].HasFlag(Mp.IsBlack))
                    {
                        map[p.i][p.j] |= Mp.Locked;

                        foreach (var d in nearDirs)
                        {
                            var s = p.Add(d);

                            if (!map[s.i][s.j].HasFlag(Mp.Locked))
                                stack.Push((p, s));
                        }
                    }
                    else
                    {
                        if (!perimeter.Contains(prevP))
                        {
                            map[prevP.i][prevP.j] |= Mp.IsEven | Mp.IsPerimeter | GetMpDir(p.Sub(prevP));
                            perimeter.Add(prevP);
                        }
                    }
                }

                return perimeter;
            }

            //List<(int i, int j)> OrderPerimeter(int level, ref HashSet<(int i, int j)> perimeter)
            //{
            //    var isEven = level.IsEven() ? Mp.IsEven : Mp.None;

            //    [MethodImpl(MethodImplOptions.AggressiveInlining)]
            //    bool IsPerimeter((int i, int j) v) =>
            //        map[v.i][v.j].HasFlag(Mp.IsPerimeter) && (map[v.i][v.j] & Mp.IsEven) == isEven;

            //    var orderedPerimeter = new List<(int i, int j)>();

            //    (int i, int j) FindNextPoint((int i, int j) prevX, (int i, int j) x)
            //    {

            //    }
            //}

            List<(int i, int j)> OrderPerimeter1(int level, ref HashSet<(int i, int j)> perimeter)
            {
                var isEven = level.IsEven() ? Mp.IsEven : Mp.None;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool IsPerimeter((int i, int j) v) =>
                    map[v.i][v.j].HasFlag(Mp.IsPerimeter) && (map[v.i][v.j] & Mp.IsEven) == isEven;

                var orderedPerimeter = new List<(int i, int j)>();

                (int i, int j) FindNextPoint((int i, int j) prevPrevX, (int i, int j) prevX, (int i, int j) x)
                {
                    var xBounds = GetBounds(x);

                    // тут ошибка, перескок направления. Можно ввести проверку границы, но лучше рассмотреть вариант обхода по направлению
                    // todo: сделать левый обход!!

                    var sbNear = nearDirs.Select(d => x.Add(d)).Where(v => IsPerimeter(v) && v != prevX && v != prevPrevX)
                        .ToArray();

                    if (sbNear.Length > 0)
                    {
                        if (sbNear.Length == 1)
                            return sbNear[0];

                        var bounded = sbNear.FirstOrDefault(s => GetBounds(s).Any(a => xBounds.Any(b => IsClose(a, b))));
                        
                        if (bounded != default)
                            return bounded;

                        return sbNear[0];
                    }


                    var sbFar = farDirs.Select(d => x.Add(d)).Where(v => IsPerimeter(v) && v != prevX && v != prevPrevX && !IsNearClose(v, prevX))
                        .ToArray();

                    if (sbFar.Length > 0)
                    {
                        if (sbFar.Length == 1)
                            return sbFar[0];

                        var bounded = sbFar.FirstOrDefault(s => GetBounds(s).Any(a => xBounds.Any(b => IsClose(a, b))));

                        if (bounded != default)
                            return bounded;

                        return sbFar[0];
                    }
 

                    return default;
                }

                DebugMapPerimeter(map, Mp.IsPerimeter, perimeter);

                (int,int) prevPrevP = default;
                (int, int) prevP = default;
                var p0 = perimeter.FirstOrDefault(p => GetDirs(p).Length == 1); // todo: circle single point perimeter
                if (p0 == default)
                    p0 = perimeter.First();
                var p = p0;

                do
                {
                    if (orderedPerimeter.Contains(p))
                        Debugger.Break();

                    orderedPerimeter.Add(p);
                    perimeter.Remove(p);
                    
                    var nextP = FindNextPoint(prevPrevP, prevP, p);

                    if (nextP == default)
                        break;

                    prevPrevP = prevP;
                    prevP = p;
                    p = nextP;
                } while (p != p0);

                return p != p0 ? null : orderedPerimeter;
            }

            List<List<(int i, int j)>> FindOrderedPerimeters(int level, (int i, int j)[] startPoints)
            {
                var perimeter = FindPerimeter(level, startPoints);

                //DebugMap(map, -2);
                //DebugMap(map, level);

                var result = new List<List<(int i, int j)>>();
                while (perimeter.Count > 0)
                {
                    //DebugMapPerimeter(map, Mp.IsPerimeter, perimeter);

                    var orderedPerimeter = OrderPerimeter1(level, ref perimeter);

                    //DebugMapPerimeter(map, Mp.IsPerimeter, orderedPerimeter);

                    if (orderedPerimeter != null)
                        result.Add(orderedPerimeter);
                }

                return result;
            }

            var perimeters = new List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)>();

            var startPerimeterPoints =
                new[]
                {
                    (m - 2).SelectRange(i => (i + 1, 1)),
                    (m - 2).SelectRange(i => (i + 1, n - 2)),
                    (n - 2).SelectRange(j => (1, j + 1)),
                    (n - 2).SelectRange(j => (m - 2, j + 1)),
                }.ManyToArray();

            var orderedPerimeters = FindOrderedPerimeters(1, startPerimeterPoints);

            var perimeterStack = new Stack<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)>();
            orderedPerimeters.ForEach(p => perimeterStack.Push((null, p, 1)));

            while(perimeterStack.Count > 0)
            {
                //DebugMap(map, Mp.Locked);

                var perimeter = perimeterStack.Pop();
                perimeters.Add(perimeter);

                //var startPoints = perimeter.child.SelectMany(p => nearDirs.Select(d => p.Add(d)))
                //    .Where(p => map[p.i][p.j] < 0).Distinct().ToArray(); // todo: remove distinct for release

                var newOrderedPerimeters = FindOrderedPerimeters(perimeter.childLevel + 1, perimeter.child.ToArray());
                newOrderedPerimeters.ForEach(p => perimeterStack.Push((perimeter.child, p, perimeter.childLevel + 1)));
            }

            return perimeters;
        }

        private Polygon GetPolygonFromPerimeter(int m, List<(int i, int j)> perimeter)
        {
            (int i, int j) GetDir((int i, int j) a, (int i, int j) b) => (b.i - a.i, b.j - a.j);

            var nodes = perimeter.Select(v => new
            {
                v,
                p = new Vector2(v.j, m - 1 - v.i)
            }).ToList();

            // todo: все еще есть точки для оптимизации
            // todo: 3d шрифт

            var deletedNodesRule = nodes
                .SelectCircleGroup(7, g => new
                {
                    node = g[3],
                    del1 = GetDir(g[2].v, g[3].v) == GetDir(g[3].v, g[4].v),
                    del2 = GetDir(g[1].v, g[3].v) == GetDir(g[3].v, g[5].v),
                    del3 = GetDir(g[0].v, g[3].v) == GetDir(g[3].v, g[6].v),
                })
                .Where(v => v.del1 || v.del2 || v.del3)
                .Select(v => v.node)
                .ToArray();

            foreach (var node in deletedNodesRule)
                nodes.Remove(node);

            return new Polygon()
            {
                Points = nodes.Select(n=>n.p).ToArray()
            };
        }

        public (Polygon[] polygons, (int main, int child)[] map) GetContentPolygons(string name, int colorLevel = 200)
        {
            using var bitmap = new Bitmap(contentFinder.FindContentFileName(name));
            var perimetersMap = GetPerimetersMapFromBitmap(bitmap, colorLevel);

            //DebugMap(perimetersMap, Mp.IsBlack);

            var perimetersTree = GetPerimetersTreeFromMap(perimetersMap);


            var perimeters = perimetersTree.Select(v => v.child).ToList();
            var polygons = perimeters.Select(p => GetPolygonFromPerimeter(perimetersMap.Length, p)).ToArray();
            
            var composeMap = perimetersTree.Where(v => v.main != null)
                .Select(v => (perimeters.IndexOf(v.main), perimeters.IndexOf(v.child))).ToArray();

            var borders = polygons.Select(p => p.Border).ToArray();
            var ax = borders.Min(v => v.a.x);
            var ay = borders.Min(v => v.a.y);
            var bx = borders.Max(v => v.b.x);
            var by = borders.Max(v => v.b.y);

            var sizeX = bx - ax;
            var sizeY = by - ay;
            var shift = -new Vector2(ax + 0.5 * sizeX, ay + 0.5 * sizeY);
            var maxSize = Math.Max(sizeX, sizeY);

            var normedPolygons = polygons.Select(v => v.Transform(p => (p + shift) / maxSize).ToLeft()).ToArray();

            return (normedPolygons, composeMap);
        }

        public Shape GetContentShape1(string name, int colorLevel = 200)
        {
            var (polygons, _) = GetContentPolygons(name, colorLevel);

            return polygons.Select(p => p.ToShape()).ToSingleShape();
        }

        public Shape GetContentSolid(string name, int colorLevel = 200, double zVolume = 1, int smoothLevel = 0, bool trioStrategy = true)
        {
            var (polygons, map) = GetContentPolygons(name, colorLevel);

            return polygons.Select(p => p.SmoothOut(smoothLevel)).ToArray().Compose(map)
                .Select(p => p.ToShape(zVolume, trioStrategy: trioStrategy)).ToSingleShape();
        }

        private bool[][] GetMapFromBitmap(Bitmap bitmap, int colorLevel = 200)
        {
            var n = bitmap.Width;
            var m = bitmap.Height;

            bool IsPoint((int i, int j) v)
            {
                if (v.i < 0 || v.i >= m || v.j < 0 || v.j >= n)
                    return false;

                var c = bitmap.GetPixel(v.j, v.i);
                return c.R < colorLevel && c.G < colorLevel && c.B < colorLevel;
            }

            var map = Ranges.Range(m+2).Select(i => Ranges.Range(n+2).Select(j => IsPoint((i-1, j-1))).ToArray()).ToArray();

            return map;
        }

        private Shape GetShapeFromMap(bool[][] map)
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
                .Select(v => v.OrderedEdge()))
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
                Convexes = g.Edges.Where(v=> bi[v.i] != bi[v.j]).Select(v => new int[] { bi[v.i], bi[v.j] }).ToArray()
            };

            // todo: тут есть ноды для этого shape, в них доп. информация о том какой полигон в каком находится
            // todo: перенести периметры на карту, с ее пощью определить дерево вложенности периметров
            // todo: по алгоритму заливки найти вложение периметров
        }

        public Shape GetText(string text, int fontSize = 50, string fontName = "Arial", double multY = 1, double multX = 1, bool adjust = true)
        {
            if (string.IsNullOrEmpty(text))
                return Shape.Empty;

            using var bitmap = GetTextBitmap(text, fontSize, fontName, multY, multX);
            var map = GetMapFromBitmap(bitmap);
            var shape = GetShapeFromMap(map);

            return adjust ? shape.Adjust() : shape;
        }

        public Shape GetContentShape(string name, int colorLevel = 200)
        {
            using var bitmap = new Bitmap(contentFinder.FindContentFileName(name));
            var map = GetMapFromBitmap(bitmap, colorLevel);
            var shape = GetShapeFromMap(map);

            return shape.Perfecto();
        }

        private void DebugMap(Mp[][] map, Mp flag)
        {
            for (var i = 0; i < map.Length; i++)
            {
                var line = (map[0].Length).SelectRange(j => (map[i][j].HasFlag(flag) ? "x" : " ").PadLeft(2, ' ')).SJoin(" ");
                Debug.WriteLine(line);
            }
        }

        private void DebugMapPerimeter(Mp[][] map, Mp flag, IEnumerable<(int i, int j)> perimeter, bool useNumber = false)
        {
            if (perimeter == null)
                return;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            Mp GetMpDir((int i, int j) dir) =>
                dir switch
                {
                    (0, 0) => Mp.None,
                    (-1, 0) => Mp.Left,
                    (0, 1) => Mp.Top,
                    (1, 0) => Mp.Right,
                    (0, -1) => Mp.Bottom,
                    _ => throw new ArgumentException(dir.ToString())
                };

            (int i, int j)[] nearDirs = new[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
            var nearFlagDirs = nearDirs.Select(d => (d, f: GetMpDir(d))).ToArray();
            (int i, int j)[] farDirs = new[] { (1, -1), (-1, -1), (-1, 1), (1, 1) };
            (int i, int j)[] dirs = new[] { (1, 0), (0, -1), (-1, 0), (0, 1), (1, -1), (-1, -1), (-1, 1), (1, 1) };

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            (int i, int j)[] GetMpDirs(Mp v) => nearFlagDirs.Where(fd => v.HasFlag(fd.f)).Select(fd => fd.d).ToArray();

            (int i, int j)[] GetDirs((int i, int j) v) => GetMpDirs(map[v.i][v.j]);

            (int i, int j)[] GetBounds((int i, int j) v) => GetDirs(v).Select(d => v.Add(d)).ToArray();

            var pl = perimeter.ToList();
            var bounds = perimeter.SelectMany(p => GetBounds(p)).Distinct().ToHashSet();

            string GetStr(int i, int j)
            {
                var s = " ";

                if (map[i][j].HasFlag(flag))
                {
                    s = useNumber ? pl.IndexOf((i, j)).ToString() : j.ToString();
                }
                else if (bounds.Contains((i,j)))
                {
                    s = ".";
                }
                else if (map[i][j].HasFlag(Mp.Locked))
                {
                    s = "□";
                }

                return useNumber ? s.PadLeft(3) : s.PadRight(2);
            }

            for (var i = 0; i < map.Length; i++)
            {
                var line = (map[0].Length).SelectRange(j => GetStr(i,j)).SJoin(" ");
                Debug.WriteLine($"{i}: {line}");
            }
        }
    }
}
