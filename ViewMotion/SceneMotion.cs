using AI.Extensions;
using AI.Images;
using AI.Libraries;
using AI.Model;
using AI.NBrain;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Mapster;
using Mapster.Utils;
using MapsterMapper;
using Meta;
using Meta.Extensions;
using Model;
using Model.Bezier;
using Model.Extensions;
using Model.Fourier;
using Model.Graphs;
using Model.Libraries;
using Model.Tools;
using Model3D.Actives;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using Model3D.Tools.Model;
using Model3D.Tools.Vectorization;
using Model4D;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using View3D;
using View3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using ViewMotion.Platforms.AI;
using ViewMotion.Platforms.AI.Func.T2N;
using ViewMotion.Worlds;
using static Model.Graphs.Graph;
using static Model3D.Actives.ActiveWorld;
using static View3D.Scene;
using Color = System.Drawing.Color;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Line = Model3D.Line3;
using Plane = Model3D.Plane;
using Shape = Model.Shape;
using Vector3 = Model3D.Vector3;

namespace ViewMotion;

partial class SceneMotion
{

    public Task<Motion> Scene()
    {
        var s = Shapes.ChristmasTree().TriangulateByFour().ToOy().Perfecto().ApplyColor(Color.Green);
        var plane = new Plane(new Vector3(0, 0, 0), new Vector3(0.25, 1, 0), new Vector3(0.25, 0, 1));

        var ps = s.Points3;
        var planeFn = plane.Fn;
        var intersectionFn = plane.IntersectionFn;
        var pBi = ps.WhereBi(x => planeFn(x) < 0);
        var ssPs = pBi.items.ToArray();
        var ssConvexes = s.Convexes.Transform(i => pBi.bi[i]).CleanBi();

        var map = s.Points3.Select((p, i) => (c: (pBi.bi[i] != -1 ? 1 : 0), i)).ToDictionary(v => v.i, v => v.c);
        var cutPs = s.Points3.Where((_, i) => map[i] == 1).ToArray();

        // under condition, cut by condition, vertex info (index) or convex info (-1)
        (bool inside, bool cut, int[] convex) GetCutInfo(int[] convex)
        {
            var a = map[convex[0]];
            var b = map[convex[1]];
            var c = map[convex[2]];
            var s = a + b + c;

            if (s == 0)
                return (false, false, convex);

            if (s == 3)
                return (true, false, convex);

            if (s == 1)
            {
                if (a == 1)
                    return (true, true, convex);
                
                if (b == 1)
                    return (true, true, convex.ShiftConvex(convex[1]));

                return (true, true, convex.ShiftConvex(convex[2]));
            }
            else // s == 2
            {
                if (a == 0)
                    return (false, true, convex);

                if (b == 0)
                    return (false, true, convex.ShiftConvex(convex[1]));

                return (false, true, convex.ShiftConvex(convex[2]));
            }
        }

        var convexInfos = s.Convexes.Select(GetCutInfo).ToArray();
        var cutConvexInfos = convexInfos.Where(v => v.cut).ToArray();
        var newPs = new Dictionary<(int i, int j), (int i, Vector3 p)>();
        var newConvexes = new List<int[]>();
        var planeConvexes = new List<int[]>();

        Vector3 GetP((int i, int j) e)
        {
            return intersectionFn(ps[e.i], ps[e.j]) ?? throw new ArgumentNullException();
        }

        (int i, Vector3 p) GetEdgePair((int i, int j) e)
        {
            if (newPs.TryGetValue(e, out var pair))
                return pair;

            var p = GetP(e);
            var i = ssPs.Length + newPs.Count;
            pair = (i, p);
            newPs.Add(e, pair);

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
                newConvexes.Add(newC);
                int[] newPC = [-1, pairB.i, pairA.i];
                planeConvexes.Add(newPC);
            }
            else // outside
            {
                int[] newC1 = [pBi.bi[info.convex[2]], pairB.i, pairA.i];
                int[] newC2 = [pairA.i, pBi.bi[info.convex[1]], pBi.bi[info.convex[2]]];
                newConvexes.Add(newC1);
                newConvexes.Add(newC2);
                int[] newPC = [-1, pairA.i, pairB.i];
                planeConvexes.Add(newPC);
            }
        }

        var sssPs = ssPs.Concat(newPs.Values.Select(v => v.p)).ToArray();
        var planeEdges = planeConvexes.Select(c => (c, e: c.Where(i => i != -1).ToArray())).Select(a => (a.c, e: (a.e[0], a.e[1]).OrderedEdge())).ToArray();
        var planeGraph = new Graph(planeEdges.Select(p => p.e));
        var groups = planeGraph.FullVisit().Select((group, i) => (g: group.Select(g => g.i).ToHashSet(), i)).ToArray();
        var centers = groups.Select(g => g.g.Select(i => sssPs[i]).Center()).ToArray();

        planeEdges.ForEach(pe => pe.c.ForEach((c, i) => 
        {
            if (c == -1)
            {
                var g = groups.First(g => g.g.Contains(pe.e.i));
                pe.c[i] = sssPs.Length + g.i;
            }
        }));

        var ss = new Shape
        {
            Points3 = ssPs.Concat(newPs.Values.Select(v => v.p)).Concat(centers).ToArray(),
            Convexes = ssConvexes.Concat(newConvexes).Concat(planeConvexes).ToArray(),
            Materials = ssConvexes.Index().Select(i => s.Materials[0]).Concat(newConvexes.Index().Select(_ => s.Materials[0])).Concat(planeConvexes.Index().Select(_=>Materials.GetByColor(Color.Red))).ToArray()
        }; 

        return (/*ss.ToPoints(Color.Blue, 0.3) +*/ ss + s.ToLines(Color.Blue, 0.1)).ToMotion();
    }
}