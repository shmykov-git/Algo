using Model;
using Model.Extensions;
using Model3D.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Line = Model3D.Line3;

namespace Model3D.Tools;

public static class VoxelHelper
{
    private static bool debug = false;

    public static void AppendShape(HashSet<Voxel> vs, Shape shape, double thickness = 1)
    {
        shape.Lines3.ForEach(line => AppendLine(vs, line, thickness));
    }

    // VoxelHelper.AppendLine(vs, ((-3, -2, -6), (7, 5, 5)));
    public static void AppendLine(HashSet<Voxel> vs, Line line, double thickness = 1)
    {        
        if (thickness < 0 || 1 < thickness)
            throw new ArgumentException("thickness is from 0 to 1");

        var passVs = new HashSet<Voxel>();

        var dv = line.ab.Normalize();
        var s = dv.ToSignVoxel();
        var fromDirs = new Voxel[] { (s.i, 0, 0), (0, s.j, 0), (0, 0, s.k) }.Where(v => v != Voxel.zero).Distinct().ToArray();
        var toDirs = fromDirs.Select(d => -d).ToArray();

        var v0 = line.a;
        var v = v0;
        var prevVoxel = v.ToVoxel();
        var voxel = prevVoxel;
        passVs.Add(prevVoxel);
        Vector3 fromV = v;
        var len = line.Len;

        void AddVoxel(Voxel voxel, string msg)
        {
            if (debug)
                Debug.WriteLine($"{voxel} {msg}");

            vs.Add(voxel);
        }
        AddVoxel(prevVoxel, "main");

        for(var i=1; i<len; i++)
        {
            if (debug)
                Debug.WriteLine($"{(v - v0).Length} < {line.Len}");

            prevVoxel = voxel;
            fromV = v;
            v += dv;
            voxel = v.ToVoxel();

            if (passVs.Contains(voxel))
            {
                //v += dv;
                continue;
            }

            passVs.Add(voxel);
            AddVoxel(voxel, "main");

            if ((voxel-prevVoxel).len <= 1)
            {
                //v += dv;
                continue;
            }

            double GetTimeToPlane(Voxel d, Vector3 v)
            {
                if (d.i == 1)
                    return (GetForwadDistX(v.x) / dv.x).Abs();
                else if (d.j == 1)
                    return (GetForwadDistX(v.y) / dv.y).Abs();
                else if (d.k == 1)
                    return (GetForwadDistX(v.z) / dv.z).Abs();
                else if (d.i == -1)
                    return (GetBackwardDistX(v.x) / dv.x).Abs();
                else if (d.j == -1)
                    return (GetBackwardDistX(v.y) / dv.y).Abs();
                else if (d.k == -1)
                    return (GetBackwardDistX(v.z) / dv.z).Abs();

                throw new ArgumentException();
            }

            if (thickness == 1 || fromDirs.Length == 1)
            {
                var fromDir = fromDirs.MinBy(d => GetTimeToPlane(d, fromV));
                var toDir = toDirs.MinBy(d => GetTimeToPlane(d, v));

                AddVoxel(prevVoxel + fromDir, "from");
                AddVoxel(voxel + toDir, "to");
            }
            else
            {
                var fromWeights = fromDirs.Select(d => (d, t: GetTimeToPlane(d, fromV))).OrderBy(v => v.t).ToArray();
                var fromThickness = fromWeights[0].t / fromWeights[1].t;

                var toWeights = toDirs.Select(d => (d, t: GetTimeToPlane(d, v))).OrderBy(v => v.t).ToArray();
                var toThickness = toWeights[0].t / toWeights[1].t;

                var fromVoxel = prevVoxel + fromWeights[0].d;
                var toVoxel = voxel + toWeights[0].d;

                if (fromThickness < thickness)
                    AddVoxel(fromVoxel, $"from {fromThickness}");

                if (toThickness < thickness)
                    AddVoxel(toVoxel, $"to {toThickness}");
            }
        }

        AddVoxel(line.b.ToVoxel(), "main");
    }

    private static double GetForwadDistX(double x) => Math.Ceiling(x) - x;
    private static double GetBackwardDistX(double x) => x - Math.Floor(x);
}
