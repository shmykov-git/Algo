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

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        return Shapes.PitterSphere(19, 23).Mult(0.97).ToLines().ApplyColor(Color.Aqua).ToMotion();

        var n = 10;
        var nCurve = 10;    // nCurve > 0
        var aFrom = 0;// Math.PI / 5;
        var aTo = 2*Math.PI;  // Math.PI * 2 - Math.PI / 15;// Math.PI;

        var l = 100;
        var curve = (nCurve-1) * Math.PI / (n*(aTo - aFrom));

        return Shapes.PitterSphere(nCurve, n, aFrom, aTo).Mult(0.97).ToLines().ApplyColor(Color.Aqua).ToMotion();

        return new[]{
            (n).SelectClosedInterval(2*Math.PI, a => new[]
            {
                (nCurve).SelectInterval(aFrom, aTo, t =>Shapes.Icosahedron.Perfecto(0.02).Move(Funcs3.SphereSpiral(curve, a-aFrom*curve)(t))).ToSingleShape().ApplyColor(Color.Red),

                (l).SelectInterval(aFrom, aTo, t => Funcs3.SphereSpiral(curve, a-aFrom*curve)(t)).ToShape(false).ToLines(0.5, Color.Blue),
                (l).SelectInterval(aFrom, aTo, t => Funcs3.SphereSpiral(-curve, a+aFrom*curve)(t)).ToShape(false).ToLines(0.5, Color.Green),
            }.ToSingleShape())
            .ToSingleShape(),
            Shapes.PitterSphere(nCurve, n, aFrom, aTo, Convexes.ShiftedSquares).Mult(0.97).ToLines().ApplyColor(Color.Aqua)
            //Shapes.PlaneSphere(20,20).ToLines(0.3, Color.Yellow),
            //Shapes.Coods
        }.ToSingleShape().ToMotion();
    }
}