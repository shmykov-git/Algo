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
        var options = new BezierOptions()
        {
            SmoothingResultLevel = 3,
            SmoothingAlgoLevel = 5,
        };
        var bzs = vectorizer.GetBeziers("hh3", options);
        
        var fn = bzs[0].ToBz();
        var fps = (1000).SelectInterval(x=>fn(x));

        return new[] 
        {
            options.cps.ToShape2().ToShape3().ToPoints(0.6, Color.Yellow),
            options.lps.ToShape2().ToShape3().ToPoints(0.5, Color.Red),
            options.ps.ToShape2().ToShape3().ToPoints(0.3, Color.Blue),
            fps.ToShape2().ToShape3().ToPoints(0.2, Color.Green),
        }.ToSingleShape().ToMotion(1);
    }
}