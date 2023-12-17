using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Navigation;
using Mapster.Utils;
using MathNet.Numerics;
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
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;
using Model.Tools;
using System.Drawing.Text;
using System.Threading.Tasks.Sources;
using Model.Graphs;
using Model3D.Actives;
using Aspose.ThreeD.Entities;
using Shape = Model.Shape;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics.Metrics;
using Aspose.ThreeD;

namespace ViewMotion;

/// <summary>
/// - docks?
/// </summary>
partial class SceneMotion
{
    #region ctor

    private readonly Vectorizer vectorizer;
    private readonly Random rnd;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
        this.rnd = new Random(0);
    }

    #endregion

   
    public Task<Motion> Scene()
    {
        var model = Shapes.PlaneCylinder(10, 10).Perfecto(2)/*.SplitPlanes(0.1)*/;
        var modelRotateAxis = new Vector3(1, 3, -2).Normalize();
        var useLine = true;
        var showModel = false;
        var lineSplitNum = 6;

        var planeN = 50;
        var planeSize = 3.0;
        var plane = Shapes.Plane(planeN, planeN, Convexes.Squares).SplitByConvexes(false).ToSingleShape().Perfecto(planeSize)/*.ApplyColor(Color.White)*/;
        var ps = plane.Points3;
        var ccs = plane.Planes.Select(p => p.Center()).ToArray();
        var fns = plane.Convexes.Index().Select(DistanceFn).ToArray();
        
        Vector3[] modelPoints = GetShapePoints(model);
        var net = new Net3<Net3Item<int>>(modelPoints.Select((p, i)=> new Net3Item<int>(i, () => modelPoints[i].SetZ(0))), 0.6 * planeSize / (planeN - 1));

        Vector3[] GetShapePoints(Shape s) => useLine
            ? s.Lines3.SelectMany(l => (lineSplitNum).SelectInterval(x => l.a + x.v * l.ab)).ToArray()
            : s.Planes.Select(v=>v.Center()).Concat(s.Points3).ToArray();

        Func<Vector3, double> DistanceFn(int i)             
        {
            var prFn = plane.ProjectionFn(i);
            var insFn = plane.IsInsideConvexFn(i);
            var disFn = plane.ConvexDistanceFn(i);

            return p => insFn(prFn(p)) ? Math.Min(0, disFn(p)) : 0;
        };

        Shape GetShape(double v)
        {
            var dynPlane = plane.Copy();
            var shape = model.Rotate(2 * Math.PI * v, modelRotateAxis);

            modelPoints = GetShapePoints(shape);
            net.Update();

            dynPlane.Convexes.ForEach((c, iC) =>
            {
                var inds = net.SelectNeighbors(ccs[iC]).ToArray();

                if (inds.Length > 0)
                {
                    var z = inds.Select(v => modelPoints[v.Item]).Min(p => fns[iC](p));
                    c.ForEach(i => dynPlane.Points[i] = ps[i].SetZ(z).ToV4());
                }
            });

            return new[]
            {
                dynPlane.FilterConvexPlanes((ps, _)=> !ps.Any(p=>p.z<0)).AddNormalVolume(-planeSize/(planeN-1)).ApplyColor(Color.Black),
                dynPlane.FilterConvexPlanes((ps, _)=> ps.Any(p=>p.z<0)).AddNormalVolume(-planeSize/(planeN-1)).ApplyColor(Color.Red),
                showModel ? shape/*.Move(0, 0, 1.1)*/.ToLines(5, Color.Yellow) : Shape.Empty
            }.ToSingleShape();
        }

        return (1000).SelectInterval(v => GetShape(v.v)).ToMotion();
    }
}