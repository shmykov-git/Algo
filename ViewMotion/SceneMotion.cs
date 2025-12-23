using AI.Extensions;
using AI.Images;
using AI.Libraries;
using AI.Model;
using AI.NBrain;
using Aspose.ThreeD;
using Aspose.ThreeD.Entities;
using Mapster;
using Mapster.Utils;
using MapsterMapper;
using Meta;
using Meta.Extensions;
using Model;
using Model.Bezier;
using Model.Extensions;
using Model.Fourier;
using Model.Graphs;
using Model.Libraries;
using Model.Tools;
using Model3D.Actives;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using Model3D.Tools.Model;
using Model3D.Tools.Vectorization;
using Model4D;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;
using View3D;
using View3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using ViewMotion.Platforms.AI;
using ViewMotion.Platforms.AI.Func.T2N;
using ViewMotion.Worlds;
using static Model.Graphs.Graph;
using static Model3D.Actives.ActiveWorld;
using static View3D.Scene;
using Color = System.Drawing.Color;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Line = Model3D.Line3;
using Plane = Model3D.Plane;
using Shape = Model.Shape;
using Vector3 = Model3D.Vector3;

namespace ViewMotion;

partial class SceneMotion
{

    public Task<Motion> Scene()
    {
        return MaterialActiveWorld();
    }
}