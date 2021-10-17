using Aspose.ThreeD.Utilities;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Linq;
using Model.Graphs;

namespace Model3D.Tools
{

    public static class Mazerator
    {
        public static Shape MakeMaze(Shape shape, int seed = 0, MazeType type = MazeType.SimpleRandom, (int i, int j)[] exits = null, bool openExits = true)
        {
            return MakeMazeWithPath(shape, seed, type, exits, openExits).maze;
        }

        public static (Shape maze, Shape path) MakeMazeWithPath(Shape shape, int seed = 0, MazeType type = MazeType.SimpleRandom, (int i, int j)[] exits = null, bool openExits = true)
        {
            exits ??= new[] { (0, 1), (-2, -1) };

            var points2 = shape.Points2;
            var points = shape.Points3;

            (int i, int j)? GetBound((int i, int j)[] a, (int i, int j)[] b)
            {
                return a.SelectMany(v1 => b.Select(v2 => (v1, v2))).Where(v => v.v1 == v.v2).Select(v => ((int, int)?)v.v1).FirstOrDefault();
            };

            bool IsHole((int i, int j)[] holes, int i, int j)
            {
                return holes.Any(v => (v.i == i && v.j == j) || (v.i == j && v.j == i));
            }

            var items = shape.Convexes.Index().Select(i =>
            {
                var convex = shape.Convexes[i];
                var center2 = convex.Select(i => points2[i]).Center();
                var center3 = convex.Select(i => points[i]).Center();

                return new
                {
                    i,
                    convex = convex,
                    set = convex.SelectCirclePair((i, j) => i < j ? (i, j) : (i: j, j: i)).OrderBy(v => v).ToArray(),
                    center2 = center2,
                    center3 = center3,
                    radius2 = convex.Select(i => (points2[i] - center2).Len).Max()
                };
            }).ToArray();

            var net = new Net<Model.Vector2, int>(items.Select(v => (v.center2, v.i)), 6 * items.Max(v => v.radius2));

            var nodes = items.Select(a => new
            {
                a.i,
                a.convex,
                a.set,
                a.center2,
                a.center3,
                edges = net.SelectNeighbors(a.center2)
                            .Select(i => items[i])
                            .Where(b => b != a)
                            .Select(b => (b, bound: GetBound(a.set, b.set)))
                            .Where(v => v.bound.HasValue)
                            .Select(v => (e: a.i < v.b.i ? (i: a.i, j: v.b.i) : (i: v.b.i, j: a.i), bound: v.bound.Value))
                            .ToArray()
            }).ToArray();

            GraphVisitStrategy mazeStrategy;
            switch (type)
            {
                case MazeType.SimpleRandom:
                    mazeStrategy = GraphVisitStrateges.SimpleRandom(seed);
                    break;

                case MazeType.PowerDirection:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 1, 0, 1);
                    break;

                case MazeType.PowerDirection2:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 1, 0, 2);
                    break;

                case MazeType.PowerDirection4:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 1, 0, 4);
                    break;

                case MazeType.PowerGravity:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 0, 1, 1);
                    break;

                case MazeType.PowerGravity2:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 0, 1, 2);
                    break;

                case MazeType.PowerGravity4:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 0, 1, 4);
                    break;

                case MazeType.PowerDirectionAnd3Gravity:
                    mazeStrategy = MazeratorStrateges.DirectionAndGravityRandom(points, i => nodes[i].center2, i => nodes[i].convex, seed, 1, 3, 1);
                    break;

                default:
                    throw new NotImplementedException(type.ToString());
            }

            var g = new Graph(nodes.SelectMany(n => n.edges.Select(e => e.e)).Distinct());
            var holes = g.VisitEdges(seed, mazeStrategy).Select(e => e.e).ToArray();

            var bounds = holes.SelectMany(h => nodes[h.i].edges.Select(e => e.bound).Intersect(nodes[h.j].edges.Select(e => e.bound))).ToList();

            var n = shape.Points.Length;

            exits = exits?.Select(v => (v.i < 0 ? n + v.i : v.i, v.j < 0 ? n + v.j : v.j)).Select(v=>v.OrderedEdge()).ToArray();

            if (exits != null && openExits)
                bounds = bounds.Concat(exits).ToList();

            var mazeShape = new Shape()
            {
                Points = shape.Points,
                Convexes = nodes.SelectMany(n => n.set.Where(e => !bounds.Contains(e))).Distinct().Select(e => new[] { e.i, e.j }).ToArray()
            };

            if (exits == null)
                return (mazeShape, null);


            var exitNodes = exits.SelectMany(v => nodes.Where(n => n.set.Contains(v))).ToArray();

            var gHoles = new Graph(holes);
            var path = gHoles.FindPath(exitNodes[0].i, exitNodes[^1].i).Select(n => n.i).ToArray(); // todo: exits

            var pathShape = new Shape()
            {
                Points3 = path.Select(i => nodes[i].center3).ToArray(),
                Convexes = path.Index().SelectPair((i, j) => new[] { i, j }).ToArray()
            };

            return (mazeShape, pathShape);
        }
    }
}
