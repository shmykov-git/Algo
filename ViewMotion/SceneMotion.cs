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
        var ar = Math.PI / 15;
        var n = 10;
        var l = 300;

        return new[]{
            (n).SelectClosedInterval(2*Math.PI, a => new[]
            {
                (l).SelectInterval(ar, Math.PI, t => Funcs3.SphereSpiral(2, a+ar)(t)).ToShape(false).ToLines(0.5, Color.Blue),
                (l).SelectInterval(ar, Math.PI, t => Funcs3.SphereSpiral(-2, a-ar)(t)).ToShape(false).ToLines(0.5, Color.Green),
            }.ToSingleShape())
            .ToSingleShape(),
            //Shapes.PlaneSphere(20,20).ToLines(0.3, Color.Yellow),
            //Shapes.Coods
        }.ToSingleShape().ToMotion();
    }
}