using Model3D.Extensions;
using AI.Model;
using Model.Libraries;
using Model3D.Libraries;
using System;
using System.Threading.Tasks;
using ViewMotion.Models;
using ViewMotion.Platforms.AI;
using ViewMotion.Platforms.AI.Func.T21;
using ViewMotion.Platforms.AI.Func.T2N;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> AI21_Example()
    {
        var m = 0.75;

        return new AIMotionPlatform().AI_Learn_21(new AI21Options
        {
            m = m,
            trainN = 20,
            trainR = (-2, 2),
            modelR = (-2 / m, 2 / m),
            modelN = 50,

            frames = 2000,
            showTopology = true,
            showTopologyWeights = true,
            showError = true,
            showTime = true,

            mode = P21Mode.Learn,
            //learnFunc = SurfaceFuncs.Paraboloid.MoveZ(-4),
            //learnFunc = SurfaceFuncs.Hyperboloid,
            learnFunc = SurfaceFuncs.Wave(0, 4),
            //learnFunc = SurfaceFuncs.WaveX(0, 4),
            //learnFunc = SurfaceFuncs.Polynom4.MoveZ(-4),

        },
        new NOptions
        {
            Seed = 0,
            //Graph = N21Graphs.Mercury,
            //UpGraph = N21Graphs.TreeOnMercury,
            Topology = [2, 3, 4, 1],
            UpTopology = [2, 3, 4, 5, 6, 1],
            //UpTopology = [2, 6, 5, 4, 3, 1],
            AllowGrowing = true,
            AllowBelief = false,
            PowerWeight0 = (-0.05, 0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Activator = NAct.SinB,
            DynamicW0Factor = 0.01,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 1,
            LinkFactor = 0.7,
            CrossLinkFactor = 0,
            EpochPerTrain = 200,
            EpochBeforeGrowing = 10000,
            EpochAfterLevelGrowUp = 10000,
            EpochAfterGrowUp = 1000,
            EpochUnwanted = 300,
        });
    }

    public Task<Motion> AI2N_Example()
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
            Activator = NAct.SinB,
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
