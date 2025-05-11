using System.Collections.Generic;
using System.Linq;
using Model;
using Model.Tools;
using Model3D.Extensions;
using Model.Extensions;
using System.Diagnostics;
using Model.Graphs;
using Model.Libraries;

namespace Model3D;

public class SupperShape
{
    private readonly Shape shape;

    public SupperShape(Shape shape)
    {
        this.shape = shape;
    }

    // normalized shape
    public (Shape skeletonShape, int n) GetSkeleton(double radius)
    {
        var center = shape.PointCenter;
        var ps = shape.Points3.Select(p => p - center).ToArray();

        Vector3 GetN(int[] c) => new Plane(ps[c[0]], ps[c[1]], ps[c[2]]).NOne;


        var nodes = shape.Convexes
            .SelectMany(c => c.Select(i => (i, c)))
            .GroupBy(v => v.i)
            .Select(gv => (i: gv.Key, cs: gv.Select(v => v.c).ToArray()))
            .Select(v=> new Node
            {
                i = v.i,
                p0 = ps[v.i],
                cs = v.cs,
                d = -v.cs.Select(GetN).Center().Normalize(),
            })            
            .OrderBy(v => v.i)
            .ToArray();

        var g = new Graph(shape.OrderedEdges);

        double GetVolume() => shape.Convexes.Select(c => nodes[c[0]].p.GetVolume0(nodes[c[1]].p, nodes[c[2]].p)).Sum().Abs();

        double GetShapeVolumeAfterNormalShift(double shift)
        {
            nodes.ForEach(n => n.p = n.p0 + n.d * shift);
            return GetVolume();
        }

        var delta = nodes.Select(v => (v.cs.SelectMany(c => c.Line2(v.i)).Distinct().Select(i => ps[i]).Center() - ps[v.i]).Length).Average();
        var precession = 0.01;
        var speed = 2;

        // базовое общее приближение нодов по общему объему
        var (shift, _) = Minimizer.Gold(0, delta, precession * delta, GetShapeVolumeAfterNormalShift, speed * delta).Last(); // skip animate

        nodes.ForEach(n => n.shift = shift);

        var radius2 = radius.Pow2();

        Vector3 GetRadiusPoint(Node a)=> nodes.Where(n => !n.isGrouped).Where(b => (a.p - b.p).Length2 < radius2).Select(b => b.p).Center();

        while (nodes.Any(n=>!n.isGrouped))
        {
            foreach(var n in nodes.Where(n=>!n.isGrouped)) 
            { 
                var p = GetRadiusPoint(n);
                
                if (n.p.EqualsV(p))
                    n.isGrouped = true;
                else
                    n.p = p;
            }
        }

        var skeleton = new Shape()
        {
            Points3 = nodes.Select(n => n.p).ToArray(),
            Convexes = shape.Convexes,
        };

        if (nodes.Any(n => n.isGrouped))
            skeleton = skeleton.ToLine2Shape().Normalize(true, true, true);

        var skPs = skeleton.Points3;

        nodes.ForEach(n => n.sI = skPs.Select((p, i) => (p, i)).First(v => n.p.EqualsV(v.p, Values.Epsilon5)).i);

        var psCount = shape.PointsCount;

        var skeletonShape = new Shape()
        {
            Points3 = shape.Points3
                .Concat(skPs.Select(p => p + center))
                .ToArray(),

            Convexes = shape.Convexes
                .Concat(skeleton.Convexes.Transform(i => i + psCount))
                .Concat(nodes.Select(n => new[] { n.i, psCount + n.sI }))
                .ToArray()
        };

        return (skeletonShape, skPs.Length);
    }

    #region Model

    class Node
    {
        public int i;
        public Vector3 p0;
        public int[][] cs;
        public Vector3 d;

        public Vector3 p;
        public double shift;
        public bool isGrouped;
        public int sI;
    }

    class Group
    {
        public List<Node> ns = new();
    }

    #endregion
}
