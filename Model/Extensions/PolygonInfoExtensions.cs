using Model.Tools;
using Model3D.Extensions;
using System.Linq;

namespace Model.Extensions
{
    public static class Shape2Extensions
    {
        private static FillEngine fillEngine = new FillEngine();

        public static Shape2 Triangulate(this Shape2 shape, double edgeLen)
        {
            Shape2 TriangulateFn(Shape2 s) => new Shape2
            {
                Points = s.Points,
                Convexes = fillEngine.Triangulate(s.Points, s.Convexes)
            };

            (double, double) GetMinMax(Shape2 s)
            {
                var lens = s.Lines.Select(line => line.AB.Len).ToArray();
                return (lens.Min(), lens.Max());
            }

            bool NeedSplit(Shape2 s, double edgeLen) => s.Lines.Select(line => line.AB.Len < edgeLen).Any();

            Shape2 Split(Shape2 s, double edgeLen)
            {
                var indicesOffset = s.Points.Length;

                ((int i, int j) edge, Vector2[] points, int[] indices, int[] backIndices) GetEdgeData((int i, int j) e)
                {
                    var a = s.Points[e.i];
                    var b = s.Points[e.j];
                    var oneAB = (b - a).Normed;
                    var n = (int)((b - a).Len / edgeLen) + 1;
                    var len = (b - a).Len / n;
                    var abNewPoints = Enumerable.Range(1, n - 1).Select(k => a + k * len * oneAB).ToArray();
                    var abAllIndices = new[] { e.i }.Concat(abNewPoints.Index().Select(k => k + indicesOffset)).ToArray();
                    var baAllIndices = new[] { e.j }.Concat(abNewPoints.Index().Reverse().Select(k => k + indicesOffset)).ToArray();

                    indicesOffset += abNewPoints.Length;

                    return (e, abNewPoints, abAllIndices, baAllIndices);
                }

                var edgeDatas = s.OrderedEdges.Select(GetEdgeData).ToArray();
                var edgesConvexes = edgeDatas.SelectMany(v => new[] { (v.edge, v.indices), (v.edge.ReversedEdge(), v.backIndices) }).ToDictionary(v => v.Item1, v => v.Item2);

                return new Shape2
                {
                    Points = s.Points.Concat(edgeDatas.SelectMany(e => e.points)).ToArray(),
                    Convexes = s.ConvexesIndices.Select(convex => convex.SelectMany(edge => edgesConvexes[edge]).ToArray()).ToArray()
                };
            }

            var s = shape;
            s = TriangulateFn(s);
            s = Split(s, edgeLen);
            s = TriangulateFn(s);
            return s;

            while (true)
            {
                var info = GetMinMax(shape);

                shape = TriangulateFn(shape);

                if (!NeedSplit(shape, edgeLen))
                    break;

                shape = Split(shape, edgeLen);
            }

            return shape;
        }
    }
}

