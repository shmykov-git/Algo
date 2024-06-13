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
        // smart neuron strategy (1-3(2,1))
        // learn any shape
        
        var m = 0.75f;
        //var external = 1.8f;
        var trainN = 20;
        (double from, double to) trainR = (-2, 2);
        var modelN = 50;
        (double from, double to) modelR = (-2/m, 2/m);

        //(double from, double to) trainRR = (0.2, 2);
        //(double from, double to) modelRR = (0.2, 2/m);

        var nEpoch = 500000;
        var nEpochPart = 200;
        var planSpeed = 5;
        var planI = 50;
        var showTopology = false;
        var topologyWeightHeight = 10;
        var topologyWeightNums = false;
        var showTopologyWeights = true;

        var mode = NMode.Learn;

        var options = new NOptions()
        {
            Seed = 1,
            //Graph = [[(0, 2), (0, 4), (0, 6), (0, 8), (0, 3), (0, 5), (1, 3), (1, 5), (1, 7), (1, 9)], [(2, 10), (2, 12), (3, 11), (3, 13), (3, 16), (4, 12), (4, 10), (4, 13), (5, 13), (6, 14), (6, 10), (6, 13), (7, 15), (8, 16), (9, 17), (9, 15), (9, 13)], [(10, 18), (11, 18), (12, 18), (13, 18), (14, 18), (15, 18), (16, 18), (17, 18)]],
            //Graph = [[(0, 2), (0, 3), (0, 4), (0, 6), (0, 8), (0, 9), (0, 10), (0, 12), (0, 14), (0, 16), (0, 18), (0, 20), (0, 22), (0, 26), (1, 2), (1, 3), (1, 5), (1, 7), (1, 8), (1, 9), (1, 11), (1, 13), (1, 15), (1, 17), (1, 19), (1, 21), (1, 24)], [(2, 23), (3, 24), (3, 25), (3, 28), (4, 25), (4, 27), (5, 24), (5, 26), (6, 24), (6, 27), (7, 23), (7, 25), (8, 24), (8, 27), (9, 25), (10, 24), (10, 26), (11, 24), (11, 27), (12, 23), (13, 24), (14, 23), (14, 24), (14, 25), (14, 26), (15, 23), (15, 26), (16, 23), (16, 26), (16, 28), (16, 27), (17, 23), (17, 26), (18, 24), (18, 26), (19, 25), (19, 27), (20, 26), (21, 23), (21, 25), (21, 27), (22, 23)], [(23, 28), (24, 28), (25, 28), (26, 28), (27, 28)]],
            //Model = N21Models.Socrates_Wave,
            Graph = N21Graphs.Mercury,
            //UpGraph = N21Graphs.Socrates,
            //UpGraph = [[(0, 2), (0, 4), (0, 6), (0, 8), (0, 3), (0, 5), (1, 3), (1, 5), (1, 7), (1, 9)], [(2, 10), (2, 12), (3, 11), (3, 13), (3, 16), (4, 12), (4, 10), (4, 13), (5, 13), (6, 14), (6, 10), (6, 13), (7, 15), (8, 16), (9, 17), (9, 15), (9, 13)], [(10, 18), (11, 18), (12, 18), (13, 18), (14, 18), (15, 18), (16, 18), (17, 18)]],
            Topology = [2, 19, 1],
            UpTopology = [2, 6, 6, 1],
            AllowGrowing = true,
            PowerWeight0 = (0.1, -0.05),
            ShaffleFactor = 0.01,
            SymmetryFactor = 0,
            //ParallelCount =1,
            Nu = 0.1,
            Alfa = 0.5,
            PowerFactor = 200,
            LinkFactor = 0.2,
            CrossLinkFactor = 0.1
        };

        var boxScale = m * new Vector3(1 / (trainR.to - trainR.from), 1 / (trainR.to - trainR.from), 0.125);
        var boxCenter = new Vector3(0.5, 0.5, 0.5);

        //var TrainFn = SurfaceFuncs.Paraboloid.MoveZ(-4).Boxed(boxScale, boxCenter);
        //var TrainFn = SurfaceFuncs.Hyperboloid.Boxed(boxScale, boxCenter);
        //var TrainFn = SurfaceFuncs.Wave(0, 4).Boxed(boxScale, boxCenter);
        //var TrainFn = SurfaceFuncs.Polynom4.MoveZ(-4).Boxed(boxScale, boxCenter);
        var TrainFn = SurfaceFuncs.WaveXY(0, 4).Boxed(boxScale, boxCenter);

        //var TrainFn = SurfaceFuncs.WaveFi(0, 4).Boxed(boxScale, boxCenter);


        //return (new Shape()
        //{
        //    Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
        //    Convexes = Convexes.SquaresBoth(trainN, trainN)
        //}.ToMeta() + Shapes.NativeCube.ToLines()).ToMotion();

        var training = (trainN, trainN)
            .SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, (x, y) => TrainFn(x, y))
            .Select(v => (new double[] { v.x, v.y }, new double[] { v.z }))
            .ToArray();

        //var training = (trainN, trainN)
        //    .SelectInterval(trainR.from, trainR.to, 0, 2*Math.PI, (r, fi) => TrainFn(r, fi))
        //    .Select(v => (new double[] { v.x, v.y }, new double[] { v.z }))
        //    .ToArray();

        var trainer = new NTrainer(options.With(o => o.Training = training));
        trainer.Init();

        var isGrowing = true;
        Shape lastTopology = null;

        var topMult = mode switch { NMode.Topology => 1, _ => 2 };

        Shape GetTopologyShape()
        {
            if (!isGrowing && lastTopology != null)
                return lastTopology;

            var topology = trainer.model.GetTopology().Perfecto(3);
            lastTopology = topology.ToNumSpots3(0.5 * topMult).ApplyColor(Color.Black) + topology.ToMeta(Color.Red, Color.Blue, topMult, topMult);
            return lastTopology;
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

        //Vector3 ModelFiFn(double r, double fi)
        //{
        //    var v = TrainFn(r, fi);
        //    var z = model!.Predict([r * m, fi])[0];

        //    return new Vector3(v.x, v.y, z);
        //}


        var bestErr = double.MaxValue;

        Shape GetShape(bool withTrainModel) => new[]
        {
            showTopology
            ? GetTopologyShape().Perfecto(1.8).MoveY(2)
            : Shape.Empty,
            showTopologyWeights
            ? GetTopologyWeightsShape() 
            : Shape.Empty,
            new Shape()
            {
                Points3 = (modelN, modelN).SelectInterval(modelR.from, modelR.to, modelR.from, modelR.to, ModelFn).ToArray(),
                //Points3 = (modelN, modelN).SelectInterval(modelRR.from, modelRR.to, 0, 2*Math.PI, ModelFiFn).ToArray(),
                Convexes = Convexes.Squares(modelN, modelN)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToPoints(Color.Red, 0.5),
            withTrainModel
                ? new Shape()
                {
                    Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
                    //Points3 = (trainN, trainN).SelectInterval(trainRR.from, trainRR.to, 0, 2*Math.PI, TrainFn).ToArray(),
                    Convexes = Convexes.Squares(trainN, trainN)
                }.Move(-0.5, -0.5, -0.5).Mult(2).ToLines(Color.Blue)
                : Shape.Empty,
            Shapes.Cube.Mult(2).ToLines(Color.Black)
        }.ToSingleShape();

        async IAsyncEnumerable<Shape> Animate() 
        {
            yield return GetShape(true);

            for (var k = 0; k < nEpoch / nEpochPart; k++)
            {
                var err = double.MaxValue;
                var errChanged = false;
                var bestErrChanged = false;

                if (options.AllowGrowing && isGrowing && planI < k + 1)
                {
                    var isStillGrowing = trainer.GrowUp();
                    planI += planSpeed * size0 / model.size;

                    if (isStillGrowing != isGrowing)
                    {
                        Debug.WriteLine($"UpGraph: [{trainer.model.GetGraph().Select(es => $"[{es.Select(e => $"({e.i}, {e.j})").SJoin(", ")}]").SJoin(", ")}]");
                    }

                    isGrowing = isStillGrowing;
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

                yield return GetShape(k % 100 < 50);
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