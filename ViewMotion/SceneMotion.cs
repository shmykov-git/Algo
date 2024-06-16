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

        var nEpoch = 500000;
        var nEpochPart = 200;
        var growSpeedI = 5;
        var levelTrainI = 50;

        var showTopology = true;
        var showTopologyWeights = true;
        var showError = true;
        var showTime = true;

        var mode = NMode.Learn;

        var options = new NOptions()
        {
            Seed = 1,
            Graph = N21Graphs.Mercury,
            UpGraph = N21Graphs.TreeOnMercury,
            //Topology = [2, 6, 6, 1],
            UpTopology = [2, 6, 5, 4, 3, 1],
            AllowGrowing = false,
            PowerWeight0 = (0.1, -0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            Act = NAct.Sin,
            Nu = 0.1,
            Alfa = 0.5,
            Power = 2,
            LinkFactor = 0.5,
            CrossLinkFactor = 0
        };
        
        var topologyWeightHeight = options.Act switch { NAct.Sigmoid => 10, _ => 1 };
        var topologyNums = false;
        var topologyWeightNums = false;
        var growI = levelTrainI;
        Func<int, bool> showTrainDataFn = k => k % 100 < 50;

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

        var training = (trainN, trainN)
            .SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, (x, y) => TrainFn(x, y))
            .Select((v, i) => (i, new double[] { v.x, v.y }, new double[] { v.z }))
            .ToArray();

        var trainer = new NTrainer(options.With(o => o.Training = training));
        trainer.Init();

        var isUpReady = false;
        var isLevelUp = false;

        var topMult = mode switch { NMode.Topology => 1, _ => 2 };

        Shape GetTopologyShape()
        {
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

        NModel model = trainer.model.Clone();
        NModel bestModel = model;
        var size0 = model.size;
        Debug.WriteLine($"Brain: n={model.ns.Count()} e={model.es.Count()} ({model.input.Count}->{model.output.Count})");
        Debug.WriteLine($"Graph: {trainer.model.GetGraph().ToGraphString()}");

        Vector3 ModelFn(double xx, double yy)
        {
            var x = (m * xx - trainR.from) / (trainR.to - trainR.from);
            var y = (m * yy - trainR.from) / (trainR.to - trainR.from);
            var z = model!.Predict([x, y])[0];

            return new Vector3(x, y, z);
        }


        var bestErr = double.MaxValue;

        Shape GetErrorShape()
        {
            var len = bestErr < 1 ? -Math.Log(bestErr) - 5 : 0;
            var n = 10;
            var m = 50;
            var mult = 0.5;

            return Shapes.CylinderR(n, m: m).ToOx().Perfecto(mult).ScaleX(2 * len / (n * mult))
                .AlignX(0).MoveX(-1)
                .ApplyColorSphereRGradient(2, new Vector3(-1, 0, 0), Color.Black, Color.DarkRed, Color.DarkGreen, Color.Green, Color.Green, Color.LightGreen);
        }

        var t0 = DateTime.Now;

        Shape GetTimeShape()
        {
            var len = (DateTime.Now - t0).TotalHours * 5;
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

        async Task BlowUp() 
        {
            model.BlowUp();
        }

        async IAsyncEnumerable<Shape> Animate() 
        {
            yield return GetShape(showTrainDataFn(0));

            for (var k = 0; k < nEpoch / nEpochPart; k++)
            {
                var err = double.MaxValue;
                var errChanged = false;
                var bestErrChanged = false;

                if (options.AllowGrowing && !isUpReady && growI < k + 1)
                {
                    if (isLevelUp && options.AllowBelief)
                        trainer.MakeBelieved();

                    (var isUp, isLevelUp) = trainer.GrowUp();

                    growI += isLevelUp 
                        ? levelTrainI
                        : growSpeedI * size0 / model.size;

                    if (isUp != isUpReady)
                    {
                        Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().Select(es => $"[{es.Select(e => $"({e.i}, {e.j})").SJoin(", ")}]").SJoin(", ")}]");
                    }

                    isUpReady = isUp;
                }

                for(var ii = 0; ii < nEpochPart; ii++)
                {
                    var newErr = await trainer.Train();

                    if (newErr < err)
                    {
                        err = newErr;
                        errChanged = true;
                        model = trainer.model.Clone();

                        if (err < bestErr)
                        {
                            bestErr = err;
                            bestErrChanged = true;
                            bestModel = model;
                        }
                    }
                }

                if (errChanged)
                {
                    if (bestErrChanged)
                    {
                        Debug.WriteLine($"bestErr: {err} [{k + 3}]");
                        Debug.WriteLine($"BestState: {bestModel.GetState().ToStateString()}");
                    }
                    else
                        Debug.WriteLine($"err: {err}");

                    model.ShowDebugInfo();
                }

                yield return GetShape(showTrainDataFn(k));
            }
        }

        switch (mode)
        {
            case NMode.Model:
                return GetShape(true).ToMotion(3);

            default:
                return Animate().ToMotion(3, async (t, a) => { if (t == InteractType.MouseDblClick) await BlowUp(); });
        }
        
    }
    
}