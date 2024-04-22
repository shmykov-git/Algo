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

        bool IsPolygonSupported(Polygon p)
        {
            var n = p.Points.Length;

            return  n > options.MinPointDistance && 
                    n > options.AnglePointDistance &&
                    n > options.SmoothingAlgoLevel && 
                    n > options.SmoothingResultLevel;
        }

        var bzs = polygons.Where(IsPolygonSupported)
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
        int FactorSideSigma(double sigma) => !double.IsRealNumber(sigma) || (int)(300 * sigma) > 100 ? 100 : (int)(300 * sigma);    // FactorSigma for one side direction
        int FactorSigma((double iSigma, double kSigma) s) => Math.Min(FactorSideSigma(s.iSigma), FactorSideSigma(s.kSigma));        // 0 - very perfect angle, 50 - a good one angle, 100 - this is curve (not angle at all)
        int FactorAngle(double a) => (int)(100 * a.Abs() / Math.PI);                                                                // 0 - very acute angle, 50 - right angle, 100 - straight angle
        int Factor(double a, (double, double) s) => Math.Max(FactorAngle(a), FactorSigma(s));                                       // 0 - very perfect acute angle, 50 - good right angle, 100 - some line points

        #region Bezier line points

        var basePoints = polygon.Points;
        var n = basePoints.Length;

        // factor angle - size of angle, factor sigma - the angle is correctly determined
        Vector2[] GetAnglePoints(int level) => basePoints.SelectCircleGroup(level, ps => ps.Center()).ToArray().CircleShift(level / 2);

        Vector2[] GetMoveCompensatedPoints(int level, Vector2[] ps0, Vector2[] ps1)
        {
            var n = ps1.Length;
            var moves = (n).Range().Select(i=> ps1[i] - ps0[i]).SelectCircleGroup(level, moves => moves.Center()).ToArray();

            return (n).Range().Select(i => ps1[i] - options.SmoothingMoveCompensationLength * moves[(i + level/2) % n]).ToArray();
        }

        var algoPoints = GetAnglePoints(options.SmoothingAlgoLevel);

        if (options.SmoothingMoveCompensationLength.Abs() > 0)
            algoPoints = GetMoveCompensatedPoints(options.SmoothingMoveCompensationLevel, basePoints, algoPoints);

        Vector2[] GetResultPoints() => options.SmoothingAlgoLevel == options.SmoothingResultLevel
            ? algoPoints
            : (options.SmoothingResultLevel == 1
                ? basePoints
                : GetAnglePoints(options.SmoothingResultLevel));

        var points = GetResultPoints();

        if (options.SmoothingMoveCompensationLength.Abs() > 0)
            points = GetMoveCompensatedPoints(options.SmoothingMoveCompensationLevel, basePoints, points);

        (double cornerAngle, (double iS, double kS) dirDisps, (double iA, double kA) dirAngs) GetCornerData(Vector2[] ps, int j)
        {
            double GetDirAngle(int i, int j, int k) => (ps[j] - ps[i]).FullAngle(ps[k] - ps[j]);
            double GetCornerAngle(int i, int j, int k) => GetDirAngle(j, i, k).Abs();

            (double s, double a) GetDirDispersionWithAvg(int i, int j, int[] ks) => ks.Select(k => GetDirAngle((i + n) % n, j, k)).DispersionWithAvg();

            ((double iS, double kS) ss, (double iAng, double kAng) angs) GetDirDispersionsAndAngles(int j)
            {
                var backKs = Ranges.CircleBack(n, j - 1, j - options.AnglePointDistance).ToArray();
                var forwardVs = Ranges.Circle(n, j + 1, j + options.AnglePointDistance).ToArray();

                var (iS, iAng) = GetDirDispersionWithAvg(j - 1, j, forwardVs);
                var (kS, kAng) = GetDirDispersionWithAvg(j + 1, j, backKs);

                return ((iS, kS), (iAng, kAng));
            }

            double GetCornerAvgAngle(int j) => Ranges.CircleBoth(n, j, options.AnglePointDistance).Select(v => GetCornerAngle(v.i, j, v.k)).Average();

            var a = GetCornerAvgAngle(j);
            var (ss, angs) = GetDirDispersionsAndAngles(j);

            return (a, ss, angs);
        }

        int[] GetLps()
        {
            var query = algoPoints.Index()
                .Select(j => GetCornerData(algoPoints, j))
                .Select((v, i) => (i, a: v.cornerAngle, v.dirDisps, factor: Factor(v.cornerAngle, v.dirDisps)))
                .OrderBy(v => v.factor);

            if (options.DebugBezier)
                query.ForEach(v =>
                {
                    Debug.WriteLine($"{v.i}: f={v.factor} a=({FactorAngle(v.a)}, {v.a}) s=({FactorSigma(v.dirDisps)}, {v.dirDisps})");
                });

            // выраженность угла и величина угла
            var angles = query
                .Select(v => (v.i, v.factor))
                .ToList();
            var angleFactors = angles.ToDictionary(v => v.i, v => v.factor);

            HashSet<int> nodes = new();
            HashSet<int> allows = (n).Range().ToHashSet();   // can be taken
            HashSet<int> musts = (n).Range().ToHashSet();    // must be taken

            void AddCheckSet(HashSet<int> checkSet, int j, int distance)
            {
                checkSet.Remove(j);

                Ranges.CircleBoth(n, j, distance).ForEach(v =>
                {
                    checkSet.Remove(v.i);
                    checkSet.Remove(v.k);
                });
            }

            void AddNode(int jA)
            {
                var (j, jFactor) = angles[jA];
                nodes.Add(j);
                AddCheckSet(allows, j, options.MinPointDistance);
                AddCheckSet(musts, j, options.MaxPointDistance);

                var rmAngles = angles.Where(a => !allows.Contains(a.i)).ToArray();
                rmAngles.ForEach(v => angles.Remove(v));
            }

            var sameFactorDistance = 5;
            var bestNodeDistance = 3;

            int GetBestNode(int jA, int minFactor)
            {
                bool IsSameGood(int i, int j) => allows.Contains(j) && angleFactors[j] <= minFactor && (angleFactors[i] - angleFactors[j]).Abs() <= sameFactorDistance;

                var (j, _) = angles[jA];

                var iN = Ranges.CircleBack(n, j - 1, j - bestNodeDistance).While(k => IsSameGood(j, k)).Count();
                var kN = Ranges.Circle(n, j + 1, j + bestNodeDistance).While(k => IsSameGood(j, k)).Count();

                var nn = iN + kN;
                var ind = jA;

                if (nn > 1)
                {
                    var l = Ranges.Circle(n, j - iN, j - iN + nn / 2).Last();
                    ind = l == j ? jA : angles.IndexOf((l, angleFactors[l]));
                }

                return ind;
            }

            var jA = 0;
            var nCircle = 0;
            (HashSet<int> set, int maxFactor)[] plan = [(musts, 75), (allows, 75), (musts, 95), (allows, 95), (musts, 100)];

            // todo: angles - только int, factor из словаря
            while (musts.Count > 0 || angles.Any(a=>a.factor <= options.PointIgnoreFactor))
            {
                var (j, jFactor) = angles[jA];

                var (checkSet, minFactor) = plan[nCircle];

                if (jFactor <= minFactor && checkSet.Contains(j))
                {
                    var bestA = GetBestNode(jA, minFactor);
                    AddNode(bestA);
                    nCircle = 0;
                    jA = 0;

                    continue;
                }

                jA++;   

                if (jA == angles.Count)
                {
                    jA = 0;

                    if (nCircle < plan.Length - 1)
                        nCircle++;
                }
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

            var (cornerAngle, dirDisps, dirAngs) = GetCornerData(points, j);

            if (cornerAngle < options.AllowedAngle)
            {
                if (FactorSideSigma(dirDisps.iS) < options.AllowedAngleFactor)
                    aa = aa.Rotate((dirAngs.iA.Sgn() * Math.PI - dirAngs.iA) / 2, b);

                if (FactorSideSigma(dirDisps.kS) < options.AllowedAngleFactor)
                    cc = cc.Rotate((-dirAngs.kA.Sgn() * Math.PI + dirAngs.kA) / 2, b);
            }

            return (aa, b, cc);
        }

        (Vector2 c, Vector2 d) GetOptimizedCps(int m, Vector2 a, Vector2 c, Vector2 d, Vector2 b)
        {
            var i = lps[m];
            var j = lps[(m + 1) % lps.Length];

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
            var dx = new double[] { 0.5, 0.5 };

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

        if (options.DebugFillPoints)
        {
            options.ps.Add(points);
            options.aps.Add(algoPoints);
            options.cps.Add(bps.SelectMany(v => new[] { v.c, v.d }).ToArray());
            options.lps.Add(bps.SelectMany(v => new[] { v.a, v.b }).ToArray());
        }

        var bzs = bps.Select(v => new Bz(v.a, v.c, v.d, v.b)).ToArray();

        // todo: объединить bzs до требуемого количества

        return bzs;
    }
}
