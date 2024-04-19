using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks.Dataflow;
using Mapster;
using MathNet.Numerics;
using Meta.Model;
using Model;
using Model.Bezier;
using Model.Extensions;
using Model.Libraries;
using Model.Tools;
using Model3D.Tools.Model;

namespace Model3D.Tools.Vectorization;

public partial class Vectorizer // Bezier
{
    public Bz[][] GetContentBeziers(string name, BezierOptions? options = null)
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
        var (polygons, map) = GetContentPolygons(bitmap, options);
        sw.Stop();

        if (options.DebugProcess)
            Debug.WriteLine($"Polygons: {sw.Elapsed}");

        sw.Restart();

        var bzs = polygons.Where(p => p.Points.Length >= 2)
            .Select(p => GetPolygonBeziers(p, options))
            .Where(b => b.Length > 0)
            .ToArray();

        sw.Stop();

        if (options.DebugProcess)
            Debug.WriteLine($"Beziers: {sw.Elapsed}");

        return bzs;
    }

    private Bz[] GetPolygonBeziers(Polygon polygon, BezierOptions options)
    {
        #region Bezier line points

        var minNode = options.MinPointDistance;
        var maxNode = options.MaxPointDistance;
        var maxNode2 = maxNode / 2;

        var basePoints = polygon.Points;
        var n = basePoints.Length;

        Vector2[] GetAnglePoints(int level) => basePoints.SelectCircleGroup(level, aa => aa.Center()).ToArray().CircleShift(level / 2);

        var algoPoints = GetAnglePoints(options.SmoothingAlgoLevel);
        options.aps.Add(algoPoints);

        Vector2[] GetResultPoints() => options.SmoothingAlgoLevel == options.SmoothingResultLevel
            ? algoPoints
            : (options.SmoothingResultLevel == 1
                ? basePoints
                : GetAnglePoints(options.SmoothingResultLevel));

        var points = GetResultPoints();

        double getCornerAngle(int i, int j, int k) => (points[i] - points[j]).FullAngle(points[k] - points[j]);
        double getAlgoDirectionScalar(int i, int j, int k) => (algoPoints[j] - algoPoints[i]).ScalarAngle(algoPoints[k] - algoPoints[j]);

        (double s2, double avg) GetAngleDispersionPow2(int j)
        {
            var angles = Ranges.CircleBoth(n, j, options.AnglePointDistance).Select(v => getCornerAngle(v.i, j, v.k)).ToArray();
            var avg = angles.Average();
            var s2 = angles.DispersionPow2(avg);

            return (s2, avg);
        }

        int[] GetLps()
        {
            // веса углов
            // нельзя брать углы на малом расстоянии minNode
            // нужно закончить, если все ноды на расстоянии не больше maxNode
            //    - много лишних углов

            int WS2(double s2) => double.IsNaN(s2) || double.IsInfinity(s2) ? 100 : (int)(10 * s2);
            int WA(double a) => (int)(50 * (2 - a.Abs()));

            //algoPoints.Index()
            //    .SelectCircleTriple((i, j, k) => (a: getAlgoDirectionScalar(i, j, k), s2: GetAngleDispersionPow2(j)))
            //    .Select((v, i) => (i: (i + 1) % n, v.a, v.s2))
            //    .OrderBy(v => Math.Max(WS2(v.s2.s2), WA(v.a)))
            //    .ForEach(v =>
            //    {
            //        Debug.WriteLine($"{v.i}: {Math.Min(WS2(v.s2.s2), WA(v.a))} ({WS2(v.s2.s2)}, {WA(v.a)}) [{v.s2}, {v.a}]");
            //    });

            // выраженность угла и величина угла
            var angles = algoPoints.Index().SelectCircleTriple((i, j, k) => (a: getAlgoDirectionScalar(i, j, k), s2: GetAngleDispersionPow2(j)))
                .Select((v, i) => (i: (i+1) % n, v.a, v.s2))
                .OrderBy(v => Math.Max(WS2(v.s2.s2), WA(v.a)))
                .Select(v => v.i)
                .ToArray();

            HashSet<int> nodes = new();
            HashSet<int> check = new();

            void AddNode(int i)
            {
                nodes.Add(i);
                check.Add(i / maxNode2);
            }

            foreach (var i in angles)
            {
                if (!nodes.Contains(i) && !Ranges.CircleBoth(n, i, minNode).Any(v => nodes.Contains(v.i) || nodes.Contains(v.k)))
                    AddNode(i);

                if (check.Count == angles.Length / maxNode2)
                    break;
            }

            return nodes.OrderBy(i => i).ToArray();
        }

        var lps = GetLps();
        
        if (options.DebugProcess)
            Debug.WriteLine($"Bezier line points count: {lps.Length}");

        #endregion

        #region Bezier control points

        (int i, int j) GetIndCps(int i, int j)
        {
            var bzAngles = Ranges.Circle(n, i, j).Count() >= 5
                    ? Ranges.Circle(n, i + 2, j - 2).Select(k => (k, a: (points[i] - points[k]).ScalarAngle(points[j] - points[k]))).ToArray()
                    : Ranges.Circle(n, i + 1, j - 1).Select(k => (k, a: (points[i] - points[k]).ScalarAngle(points[j] - points[k]))).ToArray();

            var hasLeft = bzAngles.Where(a => a.a <= 0).Any();
            var hasRight = bzAngles.Where(a => a.a > 0).Any();

            if (hasLeft && hasRight)
            {
                var left = bzAngles.Where(a => a.a <= 0).MaxBy(v => v.a);
                var right = bzAngles.Where(a => a.a > 0).MinBy(v => v.a);

                return left.k < right.k ? (left.k, right.k) : (right.k, left.k);
            }
            else if (hasRight)
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

        (Vector2 a, Vector2 b, Vector2 c) GetAngleCps(int i, int j, int k)
        {
            var a = points[i];
            var b = points[j];
            var c = points[k];

            var l = (a - b).Normed + (b - c).Normed;
            var line = new Line2(b, b + l);

            var aa = line.ProjectionPoint(a);
            var cc = line.ProjectionPoint(c);

            var (s2, avgAng) = GetAngleDispersionPow2(j);
            //Debug.WriteLine($"s2={s2} {s2 < options.AngleSigma2}");

            if (avgAng.Abs() < options.AllowedAngle && s2 < options.AngleSigma2)
            {
                aa = aa.Rotate((avgAng.Sgn() * Math.PI - avgAng) / 2, b);
                cc = cc.Rotate((-avgAng.Sgn() * Math.PI + avgAng) / 2, b);
            }

            if (options.DebugBreak)
            {
                aa.BreakNan();
                b.BreakNan();
                cc.BreakNan();
            }

            return (aa, b, cc);
        }

        (Vector2 c, Vector2 d) GetOptimizedCps(int m, Vector2 a, Vector2 c, Vector2 d, Vector2 b)
        {
            var i = lps[m];
            var j = lps[(m + 1) % lps.Length];

            if (options.DebugBreak)
            {
                if (Ranges.Circle(n, i + 1, j - 1).Count() > options.MaxPointDistance)
                    Debugger.Break();
            }

            var ps = Ranges.Circle(n, i + 1, j - 1).Select(k => points[k]).ToArray();

            var pn = options.OptimizationAccuracy * ps.Length;

            Func2 GetBFn(double u, double v)
            {
                var cc = a + u * (c - a);
                var dd = b + v * (d - b);
                return new[] { new Bz(a, cc, dd, b) }.ToFn();
            }

            double Fn(double u, double v)
            {
                var bFn = GetBFn(u, v);
                var bFnPs = (pn).SelectInterval(t => bFn(t)).ToArray();

                return ps.Select(p1=>bFnPs.Min(p2=>(p2-p1).Len2)).Sum(); // todo: optimize
            }

            var x0 = new double[] { 1, 1 };
            var dx = new double[] { 0.1, 0.1 };

            var (xMin, _) = Minimizer.Gradient(x0, dx, options.OptimizationEpsilon, xi => Fn(xi[0], xi[1]), false).TopLast(options.OptimizationMaxCount);

            var u = xMin[0];
            var v = xMin[1];

            var cc = a + u * (c - a);
            var dd = b + v * (d - b);

            return (cc, dd);
        }

        var bps = lps.SelectCirclePair((i, j) => (i, j, cs: GetIndCps(i, j)))       // ((i, ii, jj, j))
            .SelectMany(v => new[] { v.i, v.cs.i, v.cs.j })                         // (i, ii, jj)
            .CircleArrayShift(1)                                                    // (jj, i, ii)
            .SelectCircleTriple((i, j, k) => (i, j, k)).Triples()                   // ((jj, i, ii), (jj, i, ii))
            .Select(v => GetAngleCps(v.i, v.j, v.k))                                // ((a, b, c)), jj -> a, i -> b, ii -> c
            .SelectMany(v => new[] { v.a, v.b, v.c })                               // (a, b, c)
            .CircleArrayShift(-1)                                                   // (b, c, a)
            .SelectCircleTriple((a, b, c) => (a, b, c)).Triples()                   // (b, c, a), (b, c, a)
            .SelectCirclePair((aa, bb) => (aa, bb))                                 // ((b, c, a), (b, c, a))
            .Select((vv, m) => (vv.aa.a, b: vv.bb.a, cps: GetOptimizedCps(m, vv.aa.a, vv.aa.b, vv.aa.c, vv.bb.a))) // optimize cps
            .Select(v => (v.a, v.cps.c, v.cps.d, v.b))                              // (b, c, a, bb), b -- i
            .ToArray();

        #endregion

        // tmp debug
        options.cps.Add(bps.SelectMany(v => new[] {v.c, v.d}).ToArray());
        options.lps.Add(bps.SelectMany(v => new[] { v.a, v.b }).ToArray());
        options.ps.Add(points);

        var bzs = bps.Select(v => new Bz(v.a, v.c, v.d, v.b)).ToArray();

        // сортировать углы по sigma2
        // объединить bzs до требуемого количества
        // что-то не то с max point
        // нужна дисперсия не углов, а линий от углов

        return bzs;
    }
}
