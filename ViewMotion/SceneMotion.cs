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
using Mapster;
using MapsterMapper;
using AI.Model;
using Shape = Model.Shape;
using AI.Libraries;
using AI.NBrain;
using AI.Extensions;
using ViewMotion.Platforms.AI;

namespace ViewMotion;

partial class SceneMotion
{


    public Task<Motion> Scene()
    {
        var s = new[]
        {
            Shapes.PlaneTorus(20, 50, 4).ToOx().Perfecto(0.9999),
        }.ToSingleShape();


        var b = s.GetBorders();
        var (from, to) = (Math.Min(b.min.x, b.min.y), Math.Max(b.max.x, b.max.y));


        var mode = P2NMode.Learn;
        var m = 0.75;
        var zN = 4;

        return new AIMotionPlatform().AI_Learn_2N(new AI2NOptions
        {
            m = m,
            trainN = 30,
            trainR = (from, to),
            modelN = 30,
            modelR = (from / m, to / m),

            frames = 1500,
            showTopology = false,
            showTopologyWeights = false,
            showError = true,
            showTime = false,
            mode = mode,
            learnShape = s, 
            zN = zN,
            AllowNullZ = false
        },
        new NOptions
        {
            Seed = 1,
            Topology = [2, 4, 5, 6, 8, zN],
            UpTopology = [2, 4, 5, 6, 8, zN],
            AllowGrowing = false,
            PowerWeight0 = (-0.05, 0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Act = NAct.SinB,
            DynamicW0Factor = 0.01,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 2,
            LinkFactor = 0.45,
            CrossLinkFactor = 0,
            EpochPerTrain = 200,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        });
    }    
}