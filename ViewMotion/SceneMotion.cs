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

namespace ViewMotion;

public enum NMode
{
    Topology,
    Model,
    Learn
}

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        // оформить growing
        // сделать удобным использование нейронки

        var m = 0.75f;
        var trainN = 20;
        (double from, double to) trainR = (-2, 2);
        var modelN = 50;
        (double from, double to) modelR = (-2/m, 2/m);

        var frames = 2000;
        var showTopology = true;
        var showTopologyWeights = true;
        var showError = true;
        var showTime = true;

        var mode = NMode.Learn;

        var options = new NOptions()
        {
            Seed = 0,
            Graph = N21Graphs.Mercury,
            //UpGraph = N21Graphs.TreeOnMercury,
            //Topology = [2, 6, 6, 1],
            UpTopology = [2, 6, 6, 6, 6, 6, 1],
            //UpTopology = [2, 6, 5, 4, 3, 1],
            AllowGrowing = true,
            AllowBelief = true,
            PowerWeight0 = (-0.05, 0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Act = NAct.Sin,
            DynamicW0Factor = 0.001,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 1,
            LinkFactor = 0.5,
            CrossLinkFactor = 0,
            EpochPerTrain = 200,
            EpochBeforeGrowing = 20_000,
            EpochAfterLevelGrowUp = 20_000,
            EpochAfterGrowUp = 1000
        };
        
        var topologyWeightHeight = options.Act switch { NAct.Sigmoid => 10, _ => 1 };
        var topologyNums = false;
        var topologyWeightNums = false;
        Func<int, bool> withTrain = k => k % 100 < 50;

        var boxScale = m * new Vector3(1 / (trainR.to - trainR.from), 1 / (trainR.to - trainR.from), 0.125);
        var boxCenter = new Vector3(0.5, 0.5, 0.5);

        //var TrainFn = SurfaceFuncs.Paraboloid.MoveZ(-4).Boxed(boxScale, boxCenter);
        //var TrainFn = SurfaceFuncs.Hyperboloid.Boxed(boxScale, boxCenter);

        var TrainFn = SurfaceFuncs.Wave(0, 4).Boxed(boxScale, boxCenter);
        //var TrainFn = SurfaceFuncs.WaveX(0, 4).Boxed(boxScale, boxCenter);                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             

        //var TrainFn = SurfaceFuncs.Polynom4.MoveZ(-4).Boxed(boxScale, boxCenter);


        //return (new Shape()
        //{
        //    Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
        //    Convexes = Convexes.SquaresBoth(trainN, trainN)
        //}.ToMeta() + Shapes.NativeCube.ToLines()).ToMotion();

        var trainData = (trainN, trainN)
            .SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, (x, y) => TrainFn(x, y))
            .Select((v, i) => (i, new double[] { v.x, v.y }, new double[] { v.z }))
            .ToArray();

        var trainer = new NTrainer(options.With(o => o.TrainData = trainData));
        trainer.Init();


        Shape GetTopologyShape()
        {
            var topMult = mode switch { NMode.Topology => 1, _ => 2 };
            var topology = trainer.model.GetTopology().Perfecto(3);

            return topologyNums
                ? topology.ToNumSpots3(0.5 * topMult).ApplyColor(Color.Black) + topology.ToMeta(Color.Red, Color.Blue, topMult, topMult)
                : topology.ToMeta(Color.Red, Color.Blue, topMult, topMult);
        }

        Shape GetTopologyWeightsShape()
        {
            var ss = topologyWeightNums
                ? trainer.model.GetTopologyWeights(topologyWeightHeight).ToMeta(Color.Red, Color.Blue, 0.5, 0.5) +
                    trainer.model.GetTopology().ToNumSpots3(0.15, Color.Black)
                : trainer.model.GetTopologyWeights(topologyWeightHeight).ToMeta(Color.Red, Color.Blue, 1, 1);

            return ss.ToOy().Mult(2).Move(2, 0, 1);
        }

        if (mode == NMode.Topology)
            return GetTopologyShape().ToMotion(3);

        TrainState state = new();
        state.model = trainer.model.Clone();

        Debug.WriteLine($"Topology: {state.model.TopologyInfo}");
        Debug.WriteLine($"Graph: {state.model.GetGraph().ToGraphString()}");

        Vector3 ModelFn(double xx, double yy)
        {
            var x = (m * xx - trainR.from) / (trainR.to - trainR.from);
            var y = (m * yy - trainR.from) / (trainR.to - trainR.from);
            var z = state.model!.Predict([x, y])[0];

            return new Vector3(x, y, z);
        }

        Shape GetErrorShape()
        {
            var len = state.bestError < 1 ? -Math.Log(state.bestError) - 5 : 0;
            var n = 10;
            var m = 50;
            var mult = 0.5;

            return Shapes.CylinderR(n, m: m).ToOx().Perfecto(mult).ScaleX(3 * len / (n * mult))
                .AlignX(0).MoveX(-1)
                .ApplyColorSphereRGradient(2, new Vector3(-1, 0, 0), Color.Black, Color.DarkRed, Color.DarkGreen, Color.Green, Color.Green, Color.LightGreen, Color.LightGreen);
        }

        Shape GetTimeShape()
        {
            var len = state.time.TotalHours * 5;
            var n = 10;
            var m = 50;
            var mult = 0.5;

            return Shapes.CylinderR(n, m: m).ToOx().Perfecto(mult).ScaleX(2 * len / (n * mult))
                .AlignX(0).MoveX(-1)
                .ApplyColor(Color.Blue);
        }

        Shape GetShape(bool withTrainModel) => new[]
        {
            showTime ? GetTimeShape().MoveY(-1.4) : Shape.Empty,
            showError ? GetErrorShape().MoveY(-1.2) : Shape.Empty,
            showTopology ? GetTopologyShape().Perfecto(1.8).MoveX(-2) : Shape.Empty,
            showTopologyWeights ? GetTopologyWeightsShape() : Shape.Empty,
            new Shape()
            {
                Points3 = (modelN, modelN).SelectInterval(modelR.from, modelR.to, modelR.from, modelR.to, ModelFn).ToArray(),
                Convexes = Convexes.Squares(modelN, modelN)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToPoints(Color.Red, 0.5),
            withTrainModel
                ? new Shape()
                {
                    Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
                    Convexes = Convexes.Squares(trainN, trainN)
                }.Move(-0.5, -0.5, -0.5).Mult(2).ToLines(Color.Blue)
                : Shape.Empty,
            Shapes.Cube.Mult(2).ToLines(Color.Black)
        }.ToSingleShape();

        async IAsyncEnumerable<Shape> Animate() 
        {
            yield return GetShape(withTrain(0));

            for (var k = 1; k < frames; k++)
            {
                state = await trainer.Train();

                if (state.errorChanged)
                {
                    if (state.bestErrorChanged)
                    {
                        Debug.WriteLine($"\r\n{state.BestModelInfo} {state.CountInfo} {state.bestModel.TopologyInfo}");
                        Debug.WriteLine($"BestState: {state.bestModel.GetState().ToStateString()}\r\n");
                    }
                    else
                        Debug.WriteLine($"{state.ModelInfo} {state.CountInfo} {state.model.TopologyInfo}");
                }

                if (state.isUpChanged)
                    Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().ToGraphString()}]");

                yield return GetShape(withTrain(k));
            }
        }

        switch (mode)
        {
            case NMode.Model:
                return GetShape(true).ToMotion(3);

            default:
                return Animate().ToMotion(3);
        }        
    }    
}