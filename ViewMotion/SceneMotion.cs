using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using Mapster.Utils;
using Meta;
using Model;
using Model.Extensions;
using Model.Fourier;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using Model3D.Tools.Model;
using Model4D;
using View3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Model.Tools;
using System.Drawing.Text;
using System.Threading.Tasks.Sources;
using Model.Graphs;
using Model3D.Actives;
using Aspose.ThreeD.Entities;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics.Metrics;
using Aspose.ThreeD;
using Model.Bezier;
using System.Linq;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Mapster;
using MapsterMapper;
using AI.Model;
using Shape = Model.Shape;
using AI.Libraries;
using AI.NBrain;
using AI.Extensions;
using ViewMotion.Platforms.AI;
using AI.Images;
using System.Windows.Media;
using ViewMotion.Platforms.AI.Func.T2N;
using System.IO;
using Color = System.Drawing.Color;
using static Model3D.Actives.ActiveWorld;
using ViewMotion.Worlds;
using Meta.Extensions;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        return Ranges.Pyramid3(10).Select(v => new Vector3(1.25*v.x, 1.01 * v.z, 1.25 * v.y)).DebugComposeJs(rotate:false).ToMotion(30);


        int n = 20;
        double r = 15;
        var cube = (2, 2, 2);

        var polygon = new Fr[] { (1, 1) }.ToPolygon(n).Perfecto(1);
        //var polygon = new Fr[] { (-1, 10), (5, 5) }.ToPolygon(n).Perfecto();
        //var polygon = new Fr[] { (-7, 1), (-3, 2), (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }.ToPolygon(n).SmoothOut(10).Perfecto(1.71);


        var iterator = new IEnumerable<(int i, Vector3 p, Color cl)>[] {
            new Fr[] { (1, 1) }.ToPolygon(n).Perfecto(1).Select((p, i) => (i, new Vector3(p.x, cube.Item2 / 2.0, p.y), Color.Red)),
            //new Fr[] { (1, (1, Math.PI/2)) }.ToPolygon(n).Perfecto(1).Select((p, i) => (i, new Vector3(p.x, 3 * cube.Item2 / 2.0, p.y), Color.Blue)),
            //new Fr[] { (1, 1) }.ToPolygon(n).Perfecto(1).Select((p, i) => (i, new Vector3(p.x, 5 * cube.Item2 / 2.0, p.y), Color.Green)),
        }.ToSingle();

        ((int x, int y, int z) size, (double x, double y, double z) center, Quaternion q, Color c)[] data =
            iterator.SelectCircleTriple((a, b, c) => (a, b, c, b.cl))
                .Select(v => (v.b.i, v.b.p, l: v.c.p - v.a.p, v.b.cl))
                .Select(v => (cube, (r * v.p.x, v.p.y, r * v.p.z), Quaternion.FromRotation(Vector3.ZAxis, new Vector3(v.l.x, 0, v.l.z).Normalize()), v.cl)).ToArray();

        var s = data.Select(v =>
            Shapes.PerfectCube
                .Centered()
                .Scale(v.size.x, v.size.y, v.size.z)
                .Rotate(v.q)
                .Move(v.center.x, v.center.y, v.center.z)
                .ToLines(v.c, r)).ToSingleShape();

        var str = data.Select(v => $"[[{v.size.x}, {v.size.y}, {v.size.z}], [{v.center.x}, {v.center.y}, {v.center.z}], [{v.q.x}, {v.q.y}, {v.q.z}, {v.q.w}]]").SJoin(", ");
        Debug.WriteLine($"[{str}]");

        return s.ToMotion(50);
    }
}