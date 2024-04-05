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

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        return TwoBallFallingToNetMotion();

        return new[] 
        { 
            Shapes.Cube.PutOn(0.5).ToActiveShape(),
            Shapes.Cube.Mult(0.5).PutOn(1.6).ToActiveShape(o =>
            {
                o.MaterialPower = 5;
                o.Skeleton.Power = 5;
            }),
        }.ToWorld(o =>
        {
            o.OverCalculationMult = 10;
        }).ToMotion();
    }
}