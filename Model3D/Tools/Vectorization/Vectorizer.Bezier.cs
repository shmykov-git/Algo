using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Model;
using Model.Bezier;
using Model.Extensions;
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

        var resPs = options.SmoothingAlgoLevel == options.SmoothingResultLevel
            ? gps
            : (options.SmoothingResultLevel == 1 
                ? ps 
                : GetGps(options.SmoothingResultLevel));

        options.bps = nodes.Select(i=>resPs[i]).ToArray();
        options.ps = resPs;

        var bzs = nodes.OrderBy(i=>i).SelectCirclePair((i, j) => new Bz(resPs[i], resPs[j])).ToArray();

        return bzs;
    }
}
