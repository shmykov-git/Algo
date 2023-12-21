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
        var s = vectorizer.GetText("?", new TextShapeOptions 
        {
            FontSize = 100,            
            SmoothPointCount = 5,
            //ZVolume = null,
            //TriangulationStrategy = TriangulationStrategy.None,
        }).Normalize().Perfecto(2);

        //return s.AddSkeleton(1).ToLines(0.3, Color.Red).ToMotion();

        return (new[]
        {
            s.Rotate(1, new Vector3(1,3, 2).Normalize()).Move(1, 4, 3).ApplyColor(Color.Blue).ToActiveShape(o =>
            {
                o.RotationSpeedAngle = 0.001;
                o.Speed = 0.00035 * Vector3.YAxis.MultV(new Vector3(1, -4, 3));
                o.MaterialPower = 0.2;
                o.Skeleton.Type = ActiveShapeOptions.SkeletonType.CenterPoint;
            }),
            s.Rotate(2, new Vector3(1,-2, 3).Normalize()).Move(1, 2, -3).ApplyColor(Color.Red).ToActiveShape(o =>
            {
                o.RotationSpeedAngle = -0.0005;
                o.Speed = 0.00045 * Vector3.YAxis.MultV(new Vector3(1, 2, -3));
                o.MaterialPower = 0.2;
                o.Skeleton.Type = ActiveShapeOptions.SkeletonType.CenterPoint;
            })
        }, new[]
        {
            Shapes.IcosahedronSp2.ApplyColor(Color.Black).Perfecto(0.3)
        }).ToWorld(o =>
        {
            o.UseMassCenter = true;
            o.MassCenter.GravityPower = 10;
            o.UseGround = false;
        }).ToMotion(10);
    }
}