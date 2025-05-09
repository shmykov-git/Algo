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
using Quaternion = Model3D.AsposeModel.Quaternion;
using Vector2 = Model.Vector2;
using Vector3 = Model3D.AsposeModel.Vector3;
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
using View3D;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        //return Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.3).ToMotion();

        //return Surfaces.Cylinder(5, 2, Convexes.Triangles).Perfecto().ToMeta().ToMotion();

        //var s = new Shape
        //{
        //    Points3 = [new Vector3(1, 2, 3), new Vector3(-3, -2, -1), new Vector3(-1, 1, 1)],
        //    Convexes = [[0, 1, 2], [0, 2, 1]]
        //}

        //var s = Shapes.Cube.TriangulateByFour().Perfecto().ApplyColor(Color.Red);

        //return (100).SelectInterval(0, 2*Math.PI, f=>s.Rotate(f, new Vector3(1, 2, 3))).ToMotion();

        //var c = Shapes.Icosahedron.Points3.Center();

        //var s = Shapes.Plane(2, 2).Centered().Where(v => v.x < 0 || v.y < 0, false, false).ToLines(30);//.ToMeta(multLines:30, multPoint:10);

        //return (s.ToSpots(0.5, Color.Blue) + s.ToMetaPointsShape(1.5) + Shapes.Coods2WithText()).ToMotion();

        //return s.ToNumSpots(0.3).ToMotion();

        return WorldInteractionMotion();
    }
}