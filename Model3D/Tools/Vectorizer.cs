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
using Aspose.ThreeD.Entities;
using MathNet.Numerics;
using Meta.Model;
using Model.Graphs;
using Model.Interfaces;
using Model3D.Tools.Model;
using Shape = Model.Shape;

namespace Model3D.Tools
{
    public class Vectorizer
    {
        private readonly ContentFinder contentFinder;

        public Vectorizer(ContentFinder contentFinder)
        {
            this.contentFinder = contentFinder;

            outerFlagDirs = nearDirs.Select(d => (d, f: GetMpOuterDir(d))).ToArray();
            innerFlagDirs = nearDirs.Select(d => (d, f: GetMpInnerDir(d))).ToArray();
            flagDirs = outerFlagDirs.Concat(innerFlagDirs).ToArray();
            leftDirInds = leftDirs.Select((d, i) => (d, i)).ToDictionary(v => v.d, v => (v.i + 1) % 8);
            prevNearDirs = nearDirs.Select((d, i) => (d, i)).ToDictionary(v => v.d, v => nearDirs[(v.i - 1 + 4) % 4]);
        }

        [Flags]
        enum Mp : ushort
        {
            None = 0,
            Locked = 1,             // cannot visit this point anymore
            IsBlack = 2,            // black or white
            OuterLeft = 4,          // visited from left
            OuterTop = 8,           // visited from top
            OuterRight = 16,        // visited from right
            OuterBottom = 32,       // visisted from bottom
            InnerLeft = 64,         // visited from left
            InnerTop = 128,         // visited from top
            InnerRight = 256,       // visited from right
            InnerBottom = 512,      // visisted from bottom
            Level1 = 1024,          // perimeter point of level1, Level5, level9...
            Level2 = 2048,          // perimeter point of level2, Level6, level10...
            Level3 = 4096,          // perimeter point of level3, Level7, level11...
            Level4 = 8192,          // perimeter point of level4, Level8, level12...

            IsOuterPerimeter = Level1 | Level3,
            IsInnerPerimeter = Level2 | Level4,
            IsPerimeter = Level1 | Level2 | Level3 | Level4
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] private Mp GetMpOuterDir((int i, int j) dir) =>
            dir switch
            {
                (0, 0) => Mp.None,
                (-1, 0) => Mp.OuterLeft,
                (0, 1) => Mp.OuterTop,
                (1, 0) => Mp.OuterRight,
                (0, -1) => Mp.OuterBottom,
                _ => throw new ArgumentException(dir.ToString())
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)] private Mp GetMpInnerDir((int i, int j) dir) =>
            dir switch
            {
                (0, 0) => Mp.None,
                (-1, 0) => Mp.InnerLeft,
                (0, 1) => Mp.InnerTop,
                (1, 0) => Mp.InnerRight,
                (0, -1) => Mp.InnerBottom,
                _ => throw new ArgumentException(dir.ToString())
            };

        private (int i, int j)[] nearDirs = new[] { (-1, 0), (0, 1), (1, 0), (0, -1) };
        private (int i, int j)[] leftDirs = new[] { (1, 0), (1, 1), (0, 1), (-1, 1), (-1, 0), (-1, -1), (0, -1), (1, -1) };
        private ((int, int) d, Mp f)[] outerFlagDirs;
        private ((int, int) d, Mp f)[] innerFlagDirs;
        private ((int, int) d, Mp f)[] flagDirs;
        private Dictionary<(int,int), int> leftDirInds;
        private Dictionary<(int, int), (int i, int j)> prevNearDirs;

        private IEnumerable<(int i, int j)> GetLeftDirs((int i, int j) backDir)
        {
            var j = leftDirInds[backDir];

            return (8).SelectRange(i => leftDirs[(i + j) % 8]);
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

        private List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> GetPerimetersTreeFromMap(Mp[][] map, int perimeterMinimumPoints, Func<int, bool> filterLevelFn)
        {
            var n = map[0].Length;
            var m = map.Length;

            (int i, int j)[] GetOuterBounds((int i, int j) v) => outerFlagDirs.Where(fd => map[v.i][v.j].HasFlag(fd.f)).Select(fd => v.Add(fd.d)).ToArray();
            (int i, int j)[] GetInnerBounds((int i, int j) v) => innerFlagDirs.Where(fd => map[v.i][v.j].HasFlag(fd.f)).Select(fd => v.Add(fd.d)).ToArray();

            #region FindWildPerimeter

            var stack = new Stack<((int i, int j) from, (int i, int j) to)>(3 * (n + m));

            HashSet<(int i, int j)> FindWildPerimeter(int level, (int i, int j)[] startPoints) => level.IsEven()
                ? FindInnerWildPerimeter(GetPerimeterFlag(level), startPoints)
                : FindOuterWildPerimeter(GetPerimeterFlag(level), startPoints);

            HashSet<(int i, int j)> FindOuterWildPerimeter(Mp perimeterFlag, (int i, int j)[] startPoints)
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
                        map[p.i][p.j] |= perimeterFlag | GetMpOuterDir(prevP.Sub(p));
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

            HashSet<(int i, int j)> FindInnerWildPerimeter(Mp perimeterFlag, (int i, int j)[] startPoints)
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
                            map[prevP.i][prevP.j] |= GetMpInnerDir(p.Sub(prevP));
                        }
                        else
                        {
                            map[prevP.i][prevP.j] |= perimeterFlag | GetMpInnerDir(p.Sub(prevP));
                            perimeter.Add(prevP);
                        }
                    }
                }

                return perimeter;
            }

            #endregion

            #region TamePerimeter

            Mp GetPerimeterFlag(int level) => (level % 4) switch
            {
                1 => Mp.Level1,
                2 => Mp.Level2,
                3 => Mp.Level3,
                0 => Mp.Level4,
                _ => throw new ArgumentOutOfRangeException()
            };

            List<(int i, int j)> TamePerimeter(int level, ref HashSet<(int i, int j)> wildPerimeter)
            {
                var perimeterFlag = GetPerimeterFlag(level);
                Func<(int i, int j), (int i, int j)[]> getBounds = level.Odd() ? GetOuterBounds : GetInnerBounds;

                [MethodImpl(MethodImplOptions.AggressiveInlining)] bool IsPerimeter((int i, int j) v) => map[v.i][v.j].HasFlag(perimeterFlag);

                var perimeter = new List<(int i, int j)>();

                (int i, int j) FindNextPoint((int i, int j) prevX, (int i, int j) x)
                {
                    var nextP = GetLeftDirs(prevX.Sub(x)).Select(d => x.Add(d)).FirstOrDefault(IsPerimeter);

                    return nextP == default ? x : nextP;
                }

                int CountSiblings((int i, int j) x) => leftDirs.Select(d => x.Add(d)).Count(IsPerimeter);

                var (prevP, p) = wildPerimeter.Select(p => (p, c: CountSiblings(p), bs: getBounds(p)))
                    .Where(v => v.bs.Length > 0).OrderBy(v => v.c).ThenByDescending(v => v.bs.Length)
                    .Select(v => (v.bs[0], v.p)).FirstOrDefault();

                var p0 = p;

                var protectionCount = 1000000;

                do
                {
                    if (protectionCount-- == 0)
                        Debugger.Break();

                    perimeter.Add(p);
                    wildPerimeter.Remove(p);

                    var nextP = FindNextPoint(prevP, p);

                    prevP = p;
                    p = nextP;
                } while (p != p0);

                return p != p0 ? null : perimeter;
            }

            List<List<(int i, int j)>> FindPerimeters(int level, (int i, int j)[] startPoints)
            {
                //Debug.WriteLine($"Searching wild perimeter. Start points: {startPoints.Length} level={level}");
                //DebugMapPerimeter(map, startPoints);

                var wildPerimeter = FindWildPerimeter(level, startPoints);

                //Debug.WriteLine($"Found wild: {wildPerimeter.Count} level={level}");
                //DebugMapPerimeter(map, wildPerimeter);

                var result = new List<List<(int i, int j)>>();
                while (wildPerimeter.Count > 0)
                {
                    var perimeter = TamePerimeter(level, ref wildPerimeter);

                    //Debug.WriteLine($"Taming: {perimeter.Count} ({wildPerimeter.Count}) level={level}");
                    //DebugMapPerimeter(map, perimeter);

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

            List<(List<(int i, int j)> main, List<(int i, int j)> child, int childLevel)> levelPerimeters = new();
            perimeters.Where(p=>p.Count >= perimeterMinimumPoints).ForEach(p => levelPerimeters.Add((null, p, 1)));
            
            do
            {
                var processLevelPerimeters = levelPerimeters.ToArray(); 
                levelPerimeters = new();

                foreach (var perimeter in processLevelPerimeters)
                {
                    perimetersTree.Add(perimeter);

                    if (!filterLevelFn(perimeter.childLevel + 1))
                        continue;

                    var startPoints = perimeter.childLevel.IsEven()
                        ? perimeter.child.SelectMany(GetInnerBounds).Distinct().ToArray()
                        : perimeter.child.ToArray();

                    if (startPoints.Length == 0)
                        continue;

                    var newPerimeters = FindPerimeters(perimeter.childLevel + 1, startPoints);

                    newPerimeters
                        .Where(p => p.Count >= perimeterMinimumPoints)
                        .ForEach(p => levelPerimeters.Add((perimeter.child, p, perimeter.childLevel + 1)));
                }
            } while (levelPerimeters.Count > 0);

            return perimetersTree;
        }

        private List<Vector2> GetCenterPoints(int m, List<(int i, int j)> perimeter) =>
            perimeter.Select(v => new Vector2(v.j, m - 1 - v.i)).ToList();

        private List<Vector2> GetCirclePoints(double r, int m, List<(int i, int j)> perimeter)
        {
            Vector2 PosToPoint((int i, int j) v) => new Vector2(v.j, m - 1 - v.i);
            Vector2 DirToPoint((int i, int j) d) => r*new Vector2(d.j, -d.i);

            IEnumerable<(int i, int j)> GetDirs((int i, int j) from, (int i, int j) p, (int i, int j) to)
            {
                var isDiagonal = from.i.Abs() + from.j.Abs() == 2;
                var dir = from.Direct(to);
                var dirCase = (isDiagonal, dir.i, dir.j);

                return dirCase switch
                {
                    (false, -1, 1) => new (int,int)[0],
                    (false, 0, 1) => new (int, int)[0],
                    (false, 1, 1) => new[] { (0, 1) },
                    (false, 1, 0) => new[] { (0, 1) },
                    (false, 1, -1) => new[]  { (0, 1), (1, 0) },
                    (false, 0, -1) => new[] { (0, 1), (1, 0) },
                    (false, -1, -1) => new[] { (0, 1), (1, 0), (0, -1) },
                    (false, -1, 0) => new[] { (0, 1), (1, 0), (0, -1) },

                    (false, 0, 0) => new[] { (0, 1), (1, 0), (0, -1), (-1, 0) },
                    
                    (true, -1, 1) => new (int, int)[0],
                    (true, 0, 1) => new[] { (-1, 1) },
                    (true, 1, 1) => new[] { (-1, 1) },
                    (true, 1, 0) => new[] { (-1, 1), (1, 1) },
                    (true, 1, -1) => new[] { (-1, 1), (1, 1) },
                    (true, 0, -1) => new[] { (-1, 1), (1, 1), (1, -1) },
                    (true, -1, -1) => new[] { (-1, 1), (1, 1), (1, -1), (-1, -1) },
                    (true, -1, 0) => new[] { (-1, 1), (1, 1), (1, -1), (-1, -1) },

                    _ => throw new ArgumentOutOfRangeException(dirCase.ToString())
                };
            }

            return perimeter.SelectCircleTriple((a, b, c) => (from: b.Sub(a), b, to: c.Sub(b)))
                .SelectMany(v => GetDirs(v.from, v.b, v.to).Select(d => PosToPoint(v.b) + DirToPoint(v.from.Direct(d)))).ToList();
        }

        private List<Vector2> GetSquarePoints(double r, int m, List<(int i, int j)> perimeter)
        {
            Vector2 PosToPoint((int i, int j) v) => new Vector2(v.j, m - 1 - v.i);
            Vector2 DirToPoint((int i, int j) d) => r * new Vector2(d.j, -d.i);

            IEnumerable<(int i, int j)> GetDirs((int i, int j) from, (int i, int j) p, (int i, int j) to)
            {
                var isDiagonal = from.i.Abs() + from.j.Abs() == 2;
                var dir = from.Direct(to);
                var dirCase = (isDiagonal, dir.i, dir.j);

                return dirCase switch
                {
                    (false, -1, 1) => new (int, int)[0],
                    (false, 0, 1) => new[] { (-1, 1) },
                    (false, 1, 1) => new[] { (-1, 1) },
                    (false, 1, 0) => new[] { (-1, 1) },
                    (false, 1, -1) => new[] { (-1, 1), (1, 1) },
                    (false, 0, -1) => new[] { (-1, 1), (1, 1) },
                    (false, -1, -1) => new[] { (-1, 1), (1, 1), (1, -1) },
                    (false, -1, 0) => new[] { (-1, 1), (1, 1), (1, -1) },

                    (false, 0, 0) => new[] { (-1, 1), (1, 1), (1, -1), (-1, -1) },

                    (true, -1, 1) => new (int, int)[0],
                    (true, 0, 1) => new[] { (-1, 0) },
                    (true, 1, 1) => new[] { (-1, 0) },
                    (true, 1, 0) => new[] { (-1, 0), (0, 1) },
                    (true, 1, -1) => new[] { (-1, 0), (0, 1) },
                    (true, 0, -1) => new[] { (-1, 0), (0, 1), (1, 0) },
                    (true, -1, -1) => new[] { (-1, 0), (0, 1), (1, 0) },
                    (true, -1, 0) => new[] { (-1, 0), (0, 1), (1, 0), (0, -1) },

                    _ => throw new ArgumentOutOfRangeException(dirCase.ToString())
                };
            }

            return perimeter.SelectCircleTriple((a, b, c) => (from: b.Sub(a), b, to: c.Sub(b)))
                .SelectMany(v => GetDirs(v.from, v.b, v.to).Select(d => PosToPoint(v.b) + DirToPoint(v.from.Direct(d)))).ToList();
        }

        private Polygon GetPolygonFromPerimeter(PolygonOptions options, Mp[][] map, int level, List<(int i, int j)> perimeter, int optimizationLevel)
        {
            var m = map.Length;

            var points = options.PolygonPointStrategy switch
            {
                PolygonPointStrategy.Center => GetCenterPoints(m, perimeter),
                PolygonPointStrategy.Circle => GetCirclePoints(options.PolygonPointRadius, m, perimeter),
                PolygonPointStrategy.Square => GetSquarePoints(options.PolygonPointRadius, m, perimeter),
                _ => throw new ArgumentOutOfRangeException(nameof(options.PolygonPointStrategy), options.PolygonPointStrategy.ToString())
            };

            if (optimizationLevel == 0)
                return new Polygon()
                {
                    Points = points.ToArray()
                };


            //(int i, int j) GetDir((int i, int j) a, (int i, int j) b) => (b.i - a.i, b.j - a.j);

            // todo: все еще есть точки для оптимизации (3d шрифт)
            var deleted = points
                .SelectCircleGroup(7, g => new
                {
                    p = g[3],
                    del1 = optimizationLevel > 0 && (g[3] - g[2]) == (g[4] - g[3]),
                    del2 = optimizationLevel > 1 && (g[3] - g[1]) == (g[5] - g[3]),
                    del3 = optimizationLevel > 2 && (g[3] - g[0]) == (g[6] - g[3]),
                })
                .Where(v => v.del1 || v.del2 || v.del3)
                .Select(v => v.p)
                .ToArray();

            foreach (var point in deleted)
                points.Remove(point);

            return new Polygon()
            {
                Points = points.ToArray()
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

            if (options.DebugPerimeters)
                perimetersTree.Select((p, i) => (p, i)).OrderBy(v => v.p.child.Count).ForEach(v => Debug.WriteLine($"Perimeter {v.i}: len={v.p.child.Count} lvl={v.p.childLevel} children={perimetersTree.Count(p=>p.main == v.p.child)}"));

            var perimeters = perimetersTree.Select(v => v.child).ToList();

            if (options.DebugBitmap)
                DebugMapPerimeter(perimetersMap, perimeters.SelectMany(v => v));

            var composeMap = perimetersTree.Where(v => v.main != null)
                .Select(v => (perimeters.IndexOf(v.main), perimeters.IndexOf(v.child))).ToArray();

            var polygons = perimetersTree.Select(v => GetPolygonFromPerimeter(options, perimetersMap, v.childLevel, v.child, options.PolygonOptimizationLevel)).ToArray();
            
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

            var sw = Stopwatch.StartNew();
            var (polygons, map) = GetContentPolygons(bitmap, options);
            sw.Stop();

            if (options.DebugProcess)
                Debug.WriteLine($"Polygons: {sw.Elapsed}");

            if (options.SmoothOutLevel > 0)
            {
                sw.Restart();
                polygons = polygons.Select(p => p.SmoothOut(options.SmoothOutLevel, options.SmoothAngleScalar)).ToArray();
                sw.Stop();

                if (options.DebugProcess)
                    Debug.WriteLine($"SmoothOut: {sw.Elapsed}");
            }

            var composedPolygons = polygons;

            if (options.ComposePolygons)
            {
                sw.Restart();
                composedPolygons = polygons.Compose(map, true);
                sw.Stop();

                if (options.DebugProcess)
                    Debug.WriteLine($"Compose: {sw.Elapsed}");
            }

            sw.Restart();
            var shapes = composedPolygons.Select(p => p.ToShape(options)).ToArray();
            sw.Stop();

            if (options.DebugProcess)
                Debug.WriteLine($"Shapes: {sw.Elapsed}");

            Shape shape;

            sw.Restart();

            if (options.SpliteAllPolygonsDistance.HasValue)
                shapes = shapes.Select((s, i) => s.MoveZ(i * options.SpliteAllPolygonsDistance.Value)).ToArray();

            if (options.ToLinesSize.HasValue && options.SpliteLineLevelsDistance.HasValue && !options.ComposePolygons)
            {
                var getLevel = map.GetMapLevelFn();
                
                var shapeOdd = shapes.Where((_, i) => getLevel(i).Odd()).ToSingleShape();
                var shapeEven = shapes.Where((_, i) => getLevel(i).Even()).ToSingleShape().MoveZ(options.SpliteLineLevelsDistance.Value);

                var lineShapeOdd = shapeOdd.ToLines(options.ToLinesSize.Value).ApplyColor(options.LineColors.odd);
                var lineShapeEven = shapeEven.ToLines(options.ToLinesSize.Value).ApplyColor(options.LineColors.even);

                if (options.ToSpotNumSize.HasValue)
                {
                    var numShapeOdd = shapes.Where((_, i) => getLevel(i).Odd())
                        .Select(s =>
                            s.ToNumSpots3(options.ToSpotNumSize.Value).ApplyColor(options.NumColors.odd))
                        .ToSingleShape();
                    var numShapeEven = shapes.Where((_, i) => getLevel(i).Even())
                        .Select(s =>
                            s.ToNumSpots3(options.ToSpotNumSize.Value).ApplyColor(options.NumColors.even))
                        .ToSingleShape().MoveZ(options.SpliteLineLevelsDistance.Value);

                    shape = lineShapeEven + lineShapeOdd + numShapeOdd + numShapeEven;
                }
                else
                {
                    shape = lineShapeEven + lineShapeOdd;
                }
            }
            else
            {
                if (options.ToLinesSize.HasValue)
                {
                    var lineShape = shapes.ToSingleShape().ToLines(options.ToLinesSize.Value).ApplyColor(options.LineColors.odd);

                    if (options.ToSpotNumSize.HasValue)
                    {
                        var numShape = shapes.Select(s => s.ToNumSpots3(options.ToSpotNumSize.Value).ApplyColor(options.NumColors.odd)).ToSingleShape();

                        shape = lineShape + numShape;
                    }
                    else
                    {
                        shape = lineShape;
                    }
                }
                else
                {
                    shape = shapes.ToSingleShape();
                }
            }

            sw.Stop();

            if (options.DebugProcess)
                Debug.WriteLine($"SingleShape: {sw.Elapsed}");

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

        private void DebugMapPerimeter(Mp[][] map, IEnumerable<(int i, int j)> perimeter, int pointType = 0)
        {
            if (perimeter == null)
                return;

            int CountBits(Mp f)
            {
                var a = (ushort)f;
                var count = 0;
                
                while (a > 0)
                {
                    count += a & 1;
                    a >>= 1;
                }

                return count;
            }

            (int i, int j)[] GetBounds((int i, int j) v) => flagDirs.Where(fd => map[v.i][v.j].HasFlag(fd.f)).Select(fd => fd.d).Select(d => v.Add(d)).ToArray();
            var bounds = perimeter.SelectMany(GetBounds).Distinct().ToHashSet();

            var list = perimeter.ToList();
            var hashset = perimeter.ToHashSet();

            string GetStr(int i, int j)
            {
                var s = " ";

                var perimeterCount = CountBits(map[i][j] & Mp.IsPerimeter);

                if (perimeterCount > 0)
                {
                    var taken = hashset.Contains((i, j));
                    var perChar = perimeterCount >= 2
                        ? "▩" // ▩ ⬤ ✚ 🞮
                        : (map[i][j] & Mp.IsOuterPerimeter) > 0
                            ? "■"
                            : "▲";

                    s = pointType switch
                    {
                        0 => taken ? perChar : "‎▣", // ◆ ■ ◼ ◯ ⬤ ◉ ▰ ► ▦ ▲
                        1 => j.ToString(),
                        2 => list.IndexOf((i, j)).ToString(),
                        _ => "■"
                    };
                }
                else if (bounds.Contains((i, j)))
                {
                    s = "."; // * . • ◾ 
                }
                else if (hashset.Contains((i, j)))
                {
                    s = "!";
                }
                else if (!map[i][j].HasFlag(Mp.Locked))
                {
                    s = "?"; // □ ◌ ◯ 
                }
                else if (map[i][j].HasFlag(Mp.IsBlack))
                {
                    s = "●"; // ● ◎ ◍ ▢ □ ◌ ◯ 
                }
                else
                {
                    s = "◌"; // □ ◌ ◯ 
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
                    0 => $"{i.ToString().PadRight(3)}■  {line}",
                    1 => $"{i.ToString().PadLeft(3)}: {line}",
                    2 => $"{i.ToString().PadRight(3)}■  {line}",
                    _ => $"{i.ToString().PadRight(3)}■  {line}",
                });
            }
        }
    }
}
