using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Mapster;
using MathNet.Numerics;
using Meta.Model;
using Model;
using Model.Bezier;
using Model.Extensions;
using Model.Libraries;
using Model3D.Tools.Model;

namespace Model3D.Tools.Vectorization;

public partial class Vectorizer // Bezier
{
    public Bz[][] GetBeziers(string name, BezierOptions? options = null)
    {
        options ??= new BezierOptions();

        if (options.DebugProcess)
            Debug.WriteLine($"===== <[{name}]> =====");

        var sw = Stopwatch.StartNew();
        using var bitmap = new Bitmap(contentFinder.FindContentFileName(name));
        sw.Stop();

        if (options.DebugProcess)
            Debug.WriteLine($"Bitmap: {sw.Elapsed}");

        var bzs = GetBeziers(bitmap, options);

        if (options.DebugProcess)
            Debug.WriteLine($"===== <[{name}]/> =====");

        return bzs;
    }

    private Bz[][] GetBeziers(Bitmap bitmap, BezierOptions options)
    {
        var sw = Stopwatch.StartNew();
        var (polygons, map) = GetContentPolygons(bitmap, new PolygonOptions
        {
            PolygonPointStrategy = PolygonPointStrategy.Center,
            PolygonLevelStrategy = LevelStrategy.TopLevel,
            PolygonOptimizationLevel = 0,
        });
        sw.Stop();

        if (options.DebugProcess)
            Debug.WriteLine($"Polygons: {sw.Elapsed}");

        sw.Restart();
        var bzs = polygons.Select(p => GetPolygonBeziers(p, options)).ToArray();
        sw.Stop();

        if (options.DebugProcess)
            Debug.WriteLine($"Beziers: {sw.Elapsed}");

        return bzs;
    }

    private Bz[] GetPolygonBeziers(Polygon polygon, BezierOptions options)
    {
        var minNode = options.MinPointDistance;
        var maxNode = options.MaxPointDistance;
        var maxNode2 = maxNode / 2;

        var ps = polygon.Points;
        var n = ps.Length;

        Vector2[] GetGps(int level) => ps.SelectCircleGroup(level, aa => aa.Center()).ToArray().CircleShift(level / 2);

        var gps = GetGps(options.SmoothingAlgoLevel);            

        double getScalarAngle(int i, int j, int k) => (gps[j] - gps[i]).ScalarAngle(gps[k] - gps[j]);

        var angles = gps.Index().SelectCircleTriple(getScalarAngle).ToArray().CircleShift(1)
            .Select((v, i) => (i, v)).OrderByDescending(v => v.v.Abs()).Select(v => v.i).ToArray();

        HashSet<int> nodes = new();
        HashSet<int> check = new();

        void AddNode(int i)
        {
            nodes.Add(i);
            check.Add(i / maxNode2);
        }

        foreach (var i in angles)
        {
            if (!nodes.Contains(i) && !(minNode).Range(1).Any(j => nodes.Contains(i + j) || nodes.Contains(i - j)))
                AddNode(i);

            if (check.Count == angles.Length / maxNode2)
                break;
        }

        var lps = nodes.OrderBy(i => i).ToArray();

        var resPs = options.SmoothingAlgoLevel == options.SmoothingResultLevel
            ? gps
            : (options.SmoothingResultLevel == 1 
                ? ps 
                : GetGps(options.SmoothingResultLevel));

        (int i, int j) GetCpInds(int i, int j)
        {
            var bzAngles = ((j - i + n) % n) >= 5
                    ? Ranges.Circle(i + 2, j - 2, n).Select(k => (k, a: (resPs[i] - resPs[k]).ScalarAngle(resPs[j] - resPs[k]))).ToArray()
                    : Ranges.Circle(i + 1, j - 1, n).Select(k => (k, a: (resPs[i] - resPs[k]).ScalarAngle(resPs[j] - resPs[k]))).ToArray();

            var hasLeft = bzAngles.Where(a => a.a <= 0).Any();
            var hasRight = bzAngles.Where(a => a.a > 0).Any();

            if (hasLeft && hasRight)
            {
                var left = bzAngles.Where(a => a.a <= 0).MaxBy(v => v.a);
                var right = bzAngles.Where(a => a.a > 0).MinBy(v => v.a);

                return left.k < right.k ? (left.k, right.k) : (right.k, left.k);
            } else if (hasRight)
            {
                var right = bzAngles.Where(a => a.a > 0).MinBy(v => v.a);

                return (right.k, right.k);
            }
            else if (hasLeft)
            {
                var left = bzAngles.Where(a => a.a <= 0).MaxBy(v => v.a);

                return (left.k, left.k);
            }

            throw new AlgorithmException();
        }

        // сохранить углы
        (Vector2 a, Vector2 b, Vector2 c) GetAngCps(int i, int j, int k)
        {
            var a = resPs[i];
            var b = resPs[j];
            var c = resPs[k];
            var ba = (a - b).Normed;
            var bc = (c - b).Normed;
            var n = ba + bc;

            var l = n == Vector2.Zero ? -ba + bc : n.Normal;
            var line = new Line2(b, b + l);

            var aa = line.ProjectionPoint(a);
            var cc = line.ProjectionPoint(c);

            //aa.BreakNan();
            //b.BreakNan();
            //cc.BreakNan();

            return (aa, b, cc);
        }

        (Vector2 c, Vector2 d) GetOptimized(int m, Vector2 a, Vector2 c, Vector2 d, Vector2 b)
        {
            var i = lps[m];
            var j = lps[(m + 1) % lps.Length];
            var ps = Ranges.Circle(i + 1, j - 1, n).Select(k => resPs[k]).ToArray();

            var u = 1;
            var v = 0.5;

            var cc = a + u * (c - a);
            var dd = b + v * (d - b);

            return (cc, dd);
        }

        var bps = lps.SelectCirclePair((i, j) => (i, j, cs: GetCpInds(i, j)))       // i, j -> i, ii, jj, j
            .SelectMany(v => new[] { v.i, v.cs.i, v.cs.j })                         // (i, ii, jj) -> arr
            .CircleArrayShift(1)                                                    // arr: (jj, i, ii)
            .SelectCircleTriple((i, j, k) => (i, j, k)).Triples()                   // arr: (jj, i, ii), (jj, i, ii)
            .Select(v => GetAngCps(v.i, v.j, v.k))                                  // arr: ((a, b, c))
            .SelectMany(v => new[] { v.a, v.b, v.c })                               // arr: (a, b, c)
            .CircleArrayShift(-1)                                                   // arr: (b, c, a)
            .SelectCircleTriple((a, b, c) => (a, b, c)).Triples()                   // arr (b, c, a), (b, c, a)
            .SelectCirclePair((aa, bb) => (aa, bb)).Select((vv, m) => (vv.aa.a, b: vv.bb.a, ops: GetOptimized(m, vv.aa.a, vv.aa.b, vv.aa.c, vv.bb.a))) // optimize
            .Select(v => (v.a, v.ops.c, v.ops.d, v.b))                              // (b, c, a, bb), b -- i
            .ToArray();                

        options.cps = bps.SelectMany(v => new[] {v.c, v.d}).ToArray();
        options.lps = bps.SelectMany(v => new[] { v.a, v.b }).ToArray();
        options.ps = resPs;

        var bzs = bps.Select(v => new Bz(v.a, v.c, v.d, v.b)).ToArray();

        return bzs;
    }

    //var bz = new Bz((1, 1)).Join(new Bz((2, 2)), BzJoinType.PowerTwo);
    //Vector2 a = (1.4, 1.01);
    //Bz[] bzs = [bz];
    //var bFn = bzs.ToBz();

    //var t0 = (bz.a - a).Len / ((bz.a - a).Len + (bz.la - a).Len);
    //var minFn = (double t) => (bFn(t) - a).Len2;

    //var (tMin, _) = Minimizer.Gold(t0, 0.05, 0.001, minFn, 0.1, debug: true).Last();

    //return new[]
    //{
    //    (100).SelectInterval(x => bFn(x)).ToShape2().ToShape3().ToLines(Color.Blue),
    //    Shapes.IcosahedronSp2.Perfecto(0.1).Move(a.x, a.y, 0).ApplyColor(Color.Blue),
    //    Shapes.IcosahedronSp2.Perfecto(0.1).Move(bFn(tMin).ToV3()).ApplyColor(Color.Red),
    //    Shapes.Coods2WithText(3, Color.Black, Color.Gray)
    //}.ToSingleShape().ToMotion();
}
