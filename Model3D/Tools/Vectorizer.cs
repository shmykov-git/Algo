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
using Model3D.Tools.Model;

namespace Model3D.Tools
{
    public class Vectorizer
    {
        private readonly ContentFinder contentFinder;

        public Vectorizer(ContentFinder contentFinder)
        {
            this.contentFinder = contentFinder;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)] private Mp GetMpDir((int i, int j) dir) =>
            dir switch
            {
                (0, 0) => Mp.None,
                (-1, 0) => Mp.Left,
                (0, 1) => Mp.Top,
                (1, 0) => Mp.Right,
                (0, -1) => Mp.Bottom,
                _ => throw new ArgumentException(dir.ToString())
            };

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

        private List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> GetPerimetersTreeFromMap(Mp[][] map, int perimeterMinimumPoints, Func<int, bool> filterLevelFn)
        {
            var n = map[0].Length;
            var m = map.Length;

            #region Help methods

            (int i, int j)[] nearDirs = new[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
            var nearFlagDirs = nearDirs.Select(d => (d, f: GetMpDir(d))).ToArray();
            (int i, int j)[] leftDirs = new[] { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };
            var leftDirInds = leftDirs.Select((d, i) => (d, i)).ToDictionary(v => v.d, v => (v.i + 1) % 8);

            IEnumerable<(int i, int j)> GetLeftDirs((int i, int j) backDir)
            {
                var j = leftDirInds[backDir];

                return (8).SelectRange(i => leftDirs[(i + j) % 8]);
            }

            //var rightDirs = leftDirs.Reverse().ToArray();
            //var rightDirInds = rightDirs.Select((d, i) => (d, i)).ToDictionary(v => v.d, v => (v.i + 1) % 8);

            //IEnumerable<(int i, int j)> GetRightDirs((int i, int j) backDir)
            //{
            //    var j = rightDirInds[backDir];

            //    return (8).SelectRange(i => rightDirs[(i + j) % 8]);
            //}

            (int i, int j)[] GetBoundDirs((int i, int j) v) => nearFlagDirs.Where(fd => map[v.i][v.j].HasFlag(fd.f)).Select(fd => fd.d).ToArray();
            (int i, int j)[] GetBounds((int i, int j) v) => GetBoundDirs(v).Select(d=>v.Add(d)).ToArray();

            #endregion

            #region FindWildPerimeter

            var stack = new Stack<((int i, int j) from, (int i, int j) to)>(3 * (n + m));

            HashSet<(int i, int j)> FindWildPerimeter(int level, (int i, int j)[] startPoints) => level.IsEven()
                ? FindInnerWildPerimeter(startPoints)
                : FindOutterWildPerimeter(startPoints);

            HashSet<(int i, int j)> FindOutterWildPerimeter((int i, int j)[] startPoints)
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

            HashSet<(int i, int j)> FindInnerWildPerimeter((int i, int j)[] startPoints)
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
                        if (perimeter.Contains(prevP))
                        {
                            map[prevP.i][prevP.j] |= GetMpDir(p.Sub(prevP));
                        }
                        else
                        {
                            map[prevP.i][prevP.j] |= Mp.IsEven | Mp.IsPerimeter | GetMpDir(p.Sub(prevP));
                            perimeter.Add(prevP);
                        }
                    }
                }

                return perimeter;
            }

            #endregion

            #region TamePerimeter

            List<(int i, int j)> TamePerimeter(int level, ref HashSet<(int i, int j)> perimeter)
            {
                var isEven = level.IsEven() ? Mp.IsEven : Mp.None;
                //var isRightPerimeter = (isEven & Mp.IsEven) == Mp.IsEven;

                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                bool IsPerimeter((int i, int j) v) =>
                    map[v.i][v.j].HasFlag(Mp.IsPerimeter) && (map[v.i][v.j] & Mp.IsEven) == isEven;

                var orderedPerimeter = new List<(int i, int j)>();

                (int i, int j) FindNextPoint((int i, int j) prevX, (int i, int j) x)
                {
                    var nextP = GetLeftDirs(prevX.Sub(x)).Select(d => x.Add(d)).FirstOrDefault(IsPerimeter);

                    return nextP == default ? x : nextP;
                }

                var (prevP, p) = perimeter.Select(p=>(p, bs:GetBounds(p))).Where(v=>v.bs.Length > 0).Select(v=>(v.bs[0], v.p)).FirstOrDefault();

                if (prevP == default)
                    Debugger.Break();

                var p0 = p;

                var protectionCount = 1000000;

                do
                {
                    if (protectionCount-- == 0)
                        Debugger.Break();

                    orderedPerimeter.Add(p);
                    perimeter.Remove(p);

                    var nextP = FindNextPoint(prevP, p);

                    prevP = p;
                    p = nextP;
                } while (p != p0);

                return p != p0 ? null : orderedPerimeter;
            }

            List<List<(int i, int j)>> FindPerimeters(int level, (int i, int j)[] startPoints)
            {
                var wildPerimeter = FindWildPerimeter(level, startPoints);

                var result = new List<List<(int i, int j)>>();
                while (wildPerimeter.Count > 0)
                {
                    var perimeter = TamePerimeter(level, ref wildPerimeter);

                    if (perimeter != null)
                        result.Add(perimeter);
                }

                return result;
            }

            #endregion

            var perimetersTree = new List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)>();

            var startPerimeterPoints =
                new[]
                {
                    (m - 2).SelectRange(i => (i + 1, 1)),
                    (m - 2).SelectRange(i => (i + 1, n - 2)),
                    (n - 2).SelectRange(j => (1, j + 1)),
                    (n - 2).SelectRange(j => (m - 2, j + 1)),
                }.ManyToArray();

            var perimeters = FindPerimeters(1, startPerimeterPoints);

            var perimeterStack = new Stack<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)>();
            perimeters.Where(p=>p.Count >= perimeterMinimumPoints).ForEach(p => perimeterStack.Push((null, p, 1)));

            while(perimeterStack.Count > 0)
            {
                var perimeter = perimeterStack.Pop();
                perimetersTree.Add(perimeter);

                if (!filterLevelFn(perimeter.childLevel + 1)) 
                    continue;

                var startPoints = perimeter.childLevel.IsEven()
                    ? perimeter.child.SelectMany(GetBounds).Distinct().ToArray()
                    : perimeter.child.ToArray();

                var newOrderedPerimeters = FindPerimeters(perimeter.childLevel + 1, startPoints);
                newOrderedPerimeters.Where(p => p.Count >= perimeterMinimumPoints).ForEach(p => perimeterStack.Push((perimeter.child, p, perimeter.childLevel + 1)));
            }

            return perimetersTree;
        }

        private Polygon GetPolygonFromPerimeter(int m, List<(int i, int j)> perimeter, int optimizationLevel)
        {
            (int i, int j) GetDir((int i, int j) a, (int i, int j) b) => (b.i - a.i, b.j - a.j);

            var nodes = perimeter.Select(v => new
            {
                v,
                p = new Vector2(v.j, m - 1 - v.i)
            }).ToList();

            if (optimizationLevel == 0)
                return new Polygon()
                {
                    Points = nodes.Select(n => n.p).ToArray()
                };

            // todo: все еще есть точки для оптимизации (3d шрифт)
            var deletedNodesRule = nodes
                .SelectCircleGroup(7, g => new
                {
                    node = g[3],
                    del1 = optimizationLevel > 0 && GetDir(g[2].v, g[3].v) == GetDir(g[3].v, g[4].v),
                    del2 = optimizationLevel > 1 && GetDir(g[1].v, g[3].v) == GetDir(g[3].v, g[5].v),
                    del3 = optimizationLevel > 2 && GetDir(g[0].v, g[3].v) == GetDir(g[3].v, g[6].v),
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

        private List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> FilterTreeAsOdd(
            List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> tree)
        {
            return tree.Where(v => v.childLevel.IsOdd()).Select(v =>
                (
                    v.main == null ? null : tree.FirstOrDefault(vv => vv.child == v.main).main,
                    (v.childLevel % 4 == 3) ? v.child.ToArray().Reverse().ToList() : v.child,
                    (v.childLevel + 1) / 2
                )
            ).ToList();
        }

        private List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> FilterTree(
            List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> tree,
            LevelStrategy levelStrategy) =>
            levelStrategy switch
            {
                LevelStrategy.OddLevel => FilterTreeAsOdd(tree),
                _ => tree
            };

        private Func<int, bool> GetLevelStrategyFn(LevelStrategy levelStrategy) => levelStrategy switch
        {
            LevelStrategy.All => level => true,
            LevelStrategy.TopLevel => level => level == 1,
            LevelStrategy.OddLevel => level => true,
            _ => throw new ArgumentOutOfRangeException(levelStrategy.ToString())
        };

        public (Polygon[] polygons, (int main, int child)[] map) GetContentPolygons(string name, PolygonOptions options = null)
        {
            using var bitmap = new Bitmap(contentFinder.FindContentFileName(name));
            
            return GetContentPolygons(bitmap, options);
        }

        private (Polygon[] polygons, (int main, int child)[] map) GetContentPolygons(Bitmap bitmap, PolygonOptions options)
        {
            options ??= new PolygonOptions();

            var perimetersMap = GetPerimetersMapFromBitmap(bitmap, options.ColorLevel);
            var perimetersTree = GetPerimetersTreeFromMap(perimetersMap, options.MinimumPolygonPointsCount, GetLevelStrategyFn(options.LevelStrategy));
            perimetersTree = FilterTree(perimetersTree, options.LevelStrategy);

            var perimeters = perimetersTree.Select(v => v.child).ToList();

            var polygons = perimeters.Select(p => GetPolygonFromPerimeter(perimetersMap.Length, p, options.PolygonOptimizationLevel)).ToArray();
            
            var composeMap = perimetersTree.Where(v => v.main != null)
                .Select(v => (perimeters.IndexOf(v.main), perimeters.IndexOf(v.child))).ToArray();

            if (options.DebugPerimeterLength)
                perimeters.Select((p, i) => (p, i)).OrderBy(v => v.p.Count).ForEach(v => Debug.WriteLine($"len {v.i}: {v.p.Count}"));

            if (options.DebugBitmap)
                DebugMapPerimeter(perimetersMap, Mp.IsPerimeter, perimeters.SelectMany(v=>v), 0);

            if (options.NormalizeAlign || options.NormalizeScale)
            {
                var borders = polygons.Select(p => p.Border).ToArray();
                var ax = borders.Min(v => v.a.x);
                var ay = borders.Min(v => v.a.y);
                var bx = borders.Max(v => v.b.x);
                var by = borders.Max(v => v.b.y);

                var sizeX = bx - ax;
                var sizeY = by - ay;
                var maxSize = Math.Max(sizeX, sizeY);

                var shift = options.NormalizeAlign ? - new Vector2(ax + 0.5 * sizeX, ay + 0.5 * sizeY) : Vector2.Zero;
                
                Func<Vector2, Vector2> transformFn = options.NormalizeScale
                    ? p => (p + shift) / maxSize
                    : p => p + shift;

                var normedPolygons = polygons.Select(v => v.Transform(transformFn)).ToArray();

                return (normedPolygons, composeMap);
            }
            else
            {
                return (polygons, composeMap);
            }
        }

        public Shape GetContentShape(string name, ShapeOptions options = null)
        {
            if (options.DebugProcess)
                Debug.WriteLine($"===== <[{name}]> =====");

            var sw = Stopwatch.StartNew();
            using var bitmap = new Bitmap(contentFinder.FindContentFileName(name));
            sw.Stop();

            if (options.DebugProcess)
                Debug.WriteLine($"Bitmap: {sw.Elapsed}");

            var shape = GetContentShape(bitmap, options);

            if (options.DebugProcess)
                Debug.WriteLine($"===== <[{name}]/> =====");

            return shape;
        }

        private Shape GetContentShape(Bitmap bitmap, ShapeOptions options)
        {
            options ??= new ShapeOptions();

            var trioStrategy = options.TriangulationStrategy == TriangulationStrategy.Trio;
            var needTriangulation = options.TriangulationStrategy != TriangulationStrategy.None;

            var sw = Stopwatch.StartNew();
            var (polygons, map) = GetContentPolygons(bitmap, options);
            sw.Stop();

            if (options.DebugProcess)
                Debug.WriteLine($"Polygons: {sw.Elapsed}");

            sw.Restart();

            if (options.SmoothOutLevel > 0)
                polygons = polygons.Select(p => p.SmoothOut(options.SmoothOutLevel, options.SmoothAngleScalar)).ToArray();

            if (options.ComposePolygons)
                polygons = polygons.Compose(map, true);

            var shapes = polygons.Select(p => p.ToShape(options.ZVolume, needTriangulation, options.TriangulationFixFactor, trioStrategy)).ToArray();
            Shape shape;
            
            if (options.ToLinesSize.HasValue && options.SpliteLineLevelsDistance.HasValue && !options.ComposePolygons)
            {
                var getLevel = map.GetMapLevelFn();
                shape = shapes.Where((_, i) => getLevel(i).Odd()).ToSingleShape()
                            .ToLines(options.ToLinesSize.Value)
                            .ApplyColor(options.SpliteLineColors.odd) +
                        shapes.Where((_, i) => getLevel(i).Even()).ToSingleShape()
                            .MoveZ(options.SpliteLineLevelsDistance.Value).ToLines(options.ToLinesSize.Value)
                            .ApplyColor(options.SpliteLineColors.even);
            }
            else
            {
                shape = shapes.ToSingleShape();

                if (options.ToLinesSize.HasValue)
                    shape = shape.ToLines(options.ToLinesSize.Value).ApplyColor(options.SpliteLineColors.odd);
            }

            sw.Stop();

            if (options.DebugProcess)
                Debug.WriteLine($"Shape: {sw.Elapsed} (shmooth, volume, triangle, adjust, lines)");

            return shape;
        }

        public Shape GetText(string text, int fontSize = 50, string fontName = "Arial", double zVolume = 0.1, double multY = 1,
            double multX = 1, bool aline = false, bool scale = true) =>
            GetText(text, new TextShapeOptions()
            {
                FontSize = fontSize,
                FontName = fontName,
                MultX = multX,
                MultY = multY,
                ZVolume = zVolume,
                NormalizeAlign = aline,
                NormalizeScale = scale,
            });

        public Shape GetText(string text, TextShapeOptions options)
        {
            if (string.IsNullOrEmpty(text))
                return Shape.Empty;

            using var bitmap = GetTextBitmap(text, options.FontSize, options.FontName, options.MultY, options.MultX);

            return GetContentShape(bitmap, options);
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

        public Shape GetTextObsolet(string text, int fontSize = 50, string fontName = "Arial", double multY = 1, double multX = 1, bool adjust = true)
        {
            if (string.IsNullOrEmpty(text))
                return Shape.Empty;

            using var bitmap = GetTextBitmap(text, fontSize, fontName, multY, multX);
            var map = GetMapFromBitmap(bitmap);
            var shape = GetShapeFromMap(map);

            return adjust ? shape.Adjust() : shape;
        }

        public Shape GetContentShapeObsolet(string name, int colorLevel = 200)
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

        private void DebugMapPerimeter(Mp[][] map, Mp flag, IEnumerable<(int i, int j)> perimeter, int pointType = 1)
        {
            if (perimeter == null)
                return;

            (int i, int j)[] nearDirs = new[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
            var nearFlagDirs = nearDirs.Select(d => (d, f: GetMpDir(d))).ToArray();
            (int i, int j)[] GetBoundDirs((int i, int j) v) => nearFlagDirs.Where(fd => map[v.i][v.j].HasFlag(fd.f)).Select(fd => fd.d).ToArray();
            (int i, int j)[] GetBounds((int i, int j) v) => GetBoundDirs(v).Select(d => v.Add(d)).ToArray();
            var bounds = perimeter.SelectMany(GetBounds).Distinct().ToHashSet();

            var list = perimeter.ToList();
            var hashset = perimeter.ToHashSet();

            string GetStr(int i, int j)
            {
                var s = " ";

                if (map[i][j].HasFlag(flag))
                {
                    var taken = hashset.Contains((i, j));

                    s = pointType switch
                    {
                        0 => taken ? "■" : "‎▣",
                        1 => j.ToString(),
                        2 => list.IndexOf((i, j)).ToString(),
                        _ => "■"
                    };
                }
                else if (bounds.Contains((i, j)))
                {
                    s = ".";
                }
                else if (map[i][j].HasFlag(Mp.Locked))
                {
                    s = "□";
                }

                return pointType switch
                {
                    0 => s,
                    1 => s.PadRight(2),
                    2 => s.PadLeft(3),
                    _ => s
                };
            }

            for (var i = 0; i < map.Length; i++)
            {
                var line = (map[0].Length).SelectRange(j => GetStr(i,j)).SJoin(" ");

                Debug.WriteLine(pointType switch
                {
                    0 => line,
                    1 => $"{i.ToString().PadLeft(3)}: {line}",
                    2 => line,
                    _ => line
                });
            }
        }
    }
}
