using Model;
using Model.Extensions;
using Model.Graphs;
using Model.Tools;
using Model3D.Extensions;
using Model3D.Libraries;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Model3D.Tools;

public static class Cutter
{
    public static Shape PlaneCut(Shape shape, Plane plane, Color? cutColor = null)
    {
        var s = (shape.Materials?.Length ?? 0) > 0 ? shape : shape.ApplyColor(Color.Black);
        s = s.TriangulateByFour();

        // Debug args
        Color? borderColor = null; // Color.Blue;

        Material? cutColorM = cutColor == null ? null : Materials.GetByColor(cutColor.Value);
        Material? borderColorM = borderColor == null ? null : Materials.GetByColor(borderColor.Value);
        var ps = s.Points3;
        var planeFn = plane.Fn;
        var intersectionFn = plane.IntersectionFn;
        var pBi = ps.WhereBi(x => planeFn(x) < 0);
        var ssPs = pBi.items.ToArray();
        var stConvexes = s.Convexes.Transform(i => pBi.bi[i]);
        var ssBi = stConvexes.WhereBi(cc => cc.All(i => i != -1));
        var ssConvexes = ssBi.items.ToArray();

        var map = s.Points3.Select((p, i) => (c: (pBi.bi[i] != -1 ? 1 : 0), i)).ToDictionary(v => v.i, v => v.c);

        // under condition, cut by condition, vertex info (index) or convex info (-1)
        (bool inside, bool cut, int[] convex, int i) GetCutInfo(int[] convex, int i)
        {
            var a = map[convex[0]];
            var b = map[convex[1]];
            var c = map[convex[2]];
            var s = a + b + c;

            if (s == 0)
                return (false, false, convex, i);

            if (s == 3)
                return (true, false, convex, i);

            if (s == 1)
            {
                if (a == 1)
                    return (true, true, convex, i);

                if (b == 1)
                    return (true, true, convex.ShiftConvex(convex[1]), i);

                return (true, true, convex.ShiftConvex(convex[2]), i);
            }
            else // s == 2
            {
                if (a == 0)
                    return (false, true, convex, i);

                if (b == 0)
                    return (false, true, convex.ShiftConvex(convex[1]), i);

                return (false, true, convex.ShiftConvex(convex[2]), i);
            }
        }

        var convexInfos = s.Convexes.Select(GetCutInfo).ToArray();
        var cutConvexInfos = convexInfos.Where(v => v.cut).ToArray();
        var cutPs = new Dictionary<(int i, int j), (int i, Vector3 p)>();
        var cutConvexes = new List<(int[] c, int i)>();
        var planeConvexes = new List<(int[] c, int i)>();

        Vector3 GetP((int i, int j) e)
        {
            return intersectionFn(ps[e.i], ps[e.j]) ?? throw new ArgumentNullException();
        }

        (int i, Vector3 p) GetEdgePair((int i, int j) e)
        {
            if (cutPs.TryGetValue(e, out var pair))
                return pair;

            var p = GetP(e);
            var i = ssPs.Length + cutPs.Count;
            pair = (i, p);
            cutPs.Add(e, pair);

            return pair;
        }

        foreach (var info in cutConvexInfos)
        {
            var edgeA = (info.convex[0], info.convex[1]).OrderedEdge();
            var edgeB = (info.convex[0], info.convex[2]).OrderedEdge();
            var pairA = GetEdgePair(edgeA);
            var pairB = GetEdgePair(edgeB);

            if (info.inside)
            {
                int[] newC = [pBi.bi[info.convex[0]], pairA.i, pairB.i];
                cutConvexes.Add((newC, info.i));
                int[] newPC = [-1, pairB.i, pairA.i];
                planeConvexes.Add((newPC, info.i));
            }
            else // outside
            {
                int[] newC1 = [pBi.bi[info.convex[2]], pairB.i, pairA.i];
                int[] newC2 = [pairA.i, pBi.bi[info.convex[1]], pBi.bi[info.convex[2]]];
                cutConvexes.Add((newC1, info.i));
                cutConvexes.Add((newC2, info.i));
                int[] newPC = [-1, pairA.i, pairB.i];
                planeConvexes.Add((newPC, info.i));
            }
        }

        var sssPs = ssPs.Concat(cutPs.Values.Select(v => v.p)).ToArray();
        var planeEdges = planeConvexes.Select(c => (c, e: c.c.Where(i => i != -1).ToArray(), c.i)).Select(a => (a.c, e: (a.e[0], a.e[1]).OrderedEdge(), a.i)).ToArray();
        var planeGraph = new Graph(planeEdges.Select(p => p.e));
        var groups = planeGraph.FullVisit().Where(g => g.Length >= 3).Select((group, i) => (g: group.Select(g => g.i).ToHashSet(), i)).ToArray();
        var centers = groups.Select(g => g.g.Select(i => sssPs[i]).Center()).ToArray();

        planeEdges.ForEach(pe => pe.c.c.ForEach((c, i) =>
        {
            if (c == -1)
            {
                var g = groups.First(g => g.g.Contains(pe.e.i));
                pe.c.c[i] = sssPs.Length + g.i;
            }
        }));

        var ss = new Shape
        {
            Points3 = ssPs.Concat(cutPs.Values.Select(v => v.p)).Concat(centers).ToArray(),
            Convexes = ssConvexes.Concat(cutConvexes.Select(v => v.c)).Concat(planeConvexes.Select(v => v.c)).ToArray(),
            Materials = s.Materials.ApplyBi(ssBi.bi).Concat(cutConvexes.Select(c => borderColorM ?? s.Materials[c.i])).Concat(planeConvexes.Select(c => cutColorM ?? s.Materials[c.i])).ToArray()
        };

        if (ss.Convexes.Any(c => c.Any(i => i == -1)))
            Debugger.Break();

        // todo: splite group by convexes

        return ss;
    }
}
