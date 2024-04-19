using System;
using System.Collections.Concurrent;
using System.Drawing;
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

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        return (1).SelectInterval(5, 20, x =>
        {
            var options = new BezierOptions()
            {
                AllowedAngle = 0.75 * Math.PI,
                AngleSigma2 = 0.1,
                SmoothingResultLevel = 1,
                SmoothingAlgoLevel = 1,
                MinPointDistance = 5,
                MaxPointDistance = 16,
                AnglePointDistance = 4,

                DebugProcess = true,
            };

            var bzs = vectorizer.GetContentBeziers("la", options);

            var fpss = bzs.Select(b => { var fn = b.ToFn(); return (b.Length*20).SelectInterval(x => fn(x)); }).ToArray();

            return new[]
            {
                //options.cps.Select(p=>p.ToShape2().ToShape3().ToPoints(0.34, Color.Yellow)).ToSingleShape(),
                //options.aps.Select(p=>p.ToShape2().ToShape3().ToPoints(0.33, Color.Green)).ToSingleShape(),
                //options.ps.Select(p=>p.ToShape2().ToShape3().ToPoints(0.3, Color.Blue)).ToSingleShape(),

                //options.ps.Select(p=>p.ToShape2().ToShape3().ToNumSpots3(0.1, Color.Blue)).ToSingleShape(),
                
                // углы?


                options.lps.Select(p=>p.ToShape2().ToShape3().ToPoints(0.32, Color.Red)).ToSingleShape(),
                fpss.Select(fps => fps.ToShape2().ToShape3().ToPoints(0.1, Color.Red)).ToSingleShape(),
            }.ToSingleShape().Perfecto();
        }).ToMotion(1);
    }
}