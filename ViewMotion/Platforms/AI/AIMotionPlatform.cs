using System;
using System.Drawing;
using System.Threading.Tasks;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using ViewMotion.Extensions;
using ViewMotion.Models;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using AI.Model;
using Shape = Model.Shape;
using AI.NBrain;
using AI.Extensions;
using Model3D;

namespace ViewMotion.Platforms.AI;

internal class AIMotionPlatform
{
    public Task<Motion> AI_Learn_2N(AI2NOptions o, NOptions nOptions)
    {
        var topologyWeightHeight = nOptions.Act switch { NAct.Sigmoid => 10, _ => 1 };
        var topologyNums = false;
        var topologyWeightNums = false;
        Func<int, bool> withTrain = k => k % 100 < 50;

        var boxScale = o.m * new Vector3(1 / (o.trainR.to - o.trainR.from), 1 / (o.trainR.to - o.trainR.from), 1 / (o.trainR.to - o.trainR.from));
        var boxCenter = new Vector3(0.5, 0.5, 0.5);

        var boxedShape = o.learnShape.Boxed(boxScale, boxCenter);
        var planes = o.learnShape.Planes.Select(p => new Plane(p[0], p[1], p[2])).ToArray();

        Vector3[] TrainPoints(double x, double y)
        {
            var a = new Vector3(x, y, 0);
            var b = new Vector3(x, y, 1);

            var ps = planes
                .Select(p => (p, point: p.IntersectionFn(a, b)))
                .Where(v => v.point.HasValue && v.p.IsPointInsideFn(v.point.Value))
                .Select(v => v.point.Value)
                .OrderBy(p => p.z)
                .ToArray();

            if (ps.Length == 0)
                return [];

            return [ps[0].Boxed(boxScale, boxCenter), ps[^1].Boxed(boxScale, boxCenter)];
        }

        if (o.mode == P2NMode.Shape)
            return (new Shape()
            {
                Points3 = (o.trainN, o.trainN).SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, TrainPoints).Where(v => v.Length > 1).ToSingleArray(),
            }.ToPoints(0.5).ApplyColor(Color.Blue) + Shapes.NativeCube.ToLines() + boxedShape.ToLines(Color.Red, 0.5)).Centered().ToMotion();

        var trainData = (o.trainN, o.trainN)
            .SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, TrainPoints)
            .Where(v => v.Length > 1)
            .Select((v, i) => (i, new double[] { v[0].x, v[0].y }, new double[] { v[0].z, v[1].z }))
            .ToArray();

        var trainer = new NTrainer(nOptions.With(o => o.TrainData = trainData));
        trainer.Init();

        Shape GetTopologyShape()
        {
            var topMult = o.mode switch { P2NMode.Topology => 1, _ => 2 };
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

        if (o.mode == P2NMode.Topology)
            return GetTopologyShape().ToMotion(3);

        TrainState state = new();
        state.model = trainer.model.Clone();

        Debug.WriteLine($"Topology: {state.model.TopologyInfo}");
        Debug.WriteLine($"Graph: {state.model.GetGraph().ToGraphString()}");

        Vector3[] ModelPoints(double xx, double yy)
        {
            var x = (o.m * xx - o.trainR.from) / (o.trainR.to - o.trainR.from);
            var y = (o.m * yy - o.trainR.from) / (o.trainR.to - o.trainR.from);
            var zs = state.model!.Predict([x, y]);

            return zs.Select(z => new Vector3(x, y, z)).ToArray();
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
            o.showTime ? GetTimeShape().MoveY(-1.4) : Shape.Empty,
            o.showError ? GetErrorShape().MoveY(-1.2) : Shape.Empty,
            o.showTopology ? GetTopologyShape().Perfecto(1.8).MoveX(-2) : Shape.Empty,
            o.showTopologyWeights ? GetTopologyWeightsShape() : Shape.Empty,
            new Shape()
            {
                Points3 = (o.modelN, o.modelN).SelectInterval(o.modelR.from, o.modelR.to, o.modelR.from, o.modelR.to, ModelPoints).ToSingleArray(),
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToPoints(Color.Red, 0.5),
            withTrainModel
                ? boxedShape.Move(-0.5, -0.5, -0.5).Mult(2).ToLines(Color.Blue)
                : Shape.Empty,
            Shapes.Cube.Mult(2).ToLines(Color.Black)
        }.ToSingleShape();

        async IAsyncEnumerable<Shape> Animate()
        {
            yield return GetShape(withTrain(0));

            for (var k = 1; k < o.frames; k++)
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

        switch (o.mode)
        {
            case P2NMode.Model:
                return GetShape(true).ToMotion(3);

            default:
                return Animate().ToMotion(3);
        }
    }

    public Task<Motion> AI_Learn_21(AI21Options o, NOptions nOptions)
    {
        var topologyWeightHeight = nOptions.Act switch { NAct.Sigmoid => 10, _ => 1 };
        var topologyNums = false;
        var topologyWeightNums = false;
        Func<int, bool> withTrain = k => k % 100 < 50;

        var boxScale = o.m * new Vector3(1 / (o.trainR.to - o.trainR.from), 1 / (o.trainR.to - o.trainR.from), 0.125);
        var boxCenter = new Vector3(0.5, 0.5, 0.5);

        var TrainFn = o.learnFunc.Boxed(boxScale, boxCenter);

        if (o.mode == P21Mode.Func)
            return (new Shape()
            {
                Points3 = (o.trainN, o.trainN).SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, TrainFn).ToArray(),
                Convexes = Convexes.SquaresBoth(o.trainN, o.trainN)
            }.ToMeta() + Shapes.NativeCube.ToLines()).ToMotion();

        var trainData = (o.trainN, o.trainN)
            .SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, (x, y) => TrainFn(x, y))
            .Select((v, i) => (i, new double[] { v.x, v.y }, new double[] { v.z }))
            .ToArray();

        var trainer = new NTrainer(nOptions.With(o => o.TrainData = trainData));
        trainer.Init();

        Shape GetTopologyShape()
        {
            var topMult = o.mode switch { P21Mode.Topology => 1, _ => 2 };
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

        if (o.mode == P21Mode.Topology)
            return GetTopologyShape().ToMotion(3);

        TrainState state = new();
        state.model = trainer.model.Clone();

        Debug.WriteLine($"Topology: {state.model.TopologyInfo}");
        Debug.WriteLine($"Graph: {state.model.GetGraph().ToGraphString()}");

        Vector3 ModelFn(double xx, double yy)
        {
            var x = (o.m * xx - o.trainR.from) / (o.trainR.to - o.trainR.from);
            var y = (o.m * yy - o.trainR.from) / (o.trainR.to - o.trainR.from);
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
            o.showTime ? GetTimeShape().MoveY(-1.4) : Shape.Empty,
            o.showError ? GetErrorShape().MoveY(-1.2) : Shape.Empty,
            o.showTopology ? GetTopologyShape().Perfecto(1.8).MoveX(-2) : Shape.Empty,
            o.showTopologyWeights ? GetTopologyWeightsShape() : Shape.Empty,
            new Shape()
            {
                Points3 = (o.modelN, o.modelN).SelectInterval(o.modelR.from, o.modelR.to, o.modelR.from, o.modelR.to, ModelFn).ToArray(),
                Convexes = Convexes.Squares(o.modelN, o.modelN)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToPoints(Color.Red, 0.5),
            withTrainModel
                ? new Shape()
                {
                    Points3 = (o.trainN, o.trainN).SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, TrainFn).ToArray(),
                    Convexes = Convexes.Squares(o.trainN, o.trainN)
                }.Move(-0.5, -0.5, -0.5).Mult(2).ToLines(Color.Blue)
                : Shape.Empty,
            Shapes.Cube.Mult(2).ToLines(Color.Black)
        }.ToSingleShape();

        async IAsyncEnumerable<Shape> Animate()
        {
            yield return GetShape(withTrain(0));

            for (var k = 1; k < o.frames; k++)
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

        switch (o.mode)
        {
            case P21Mode.Model:
                return GetShape(true).ToMotion(3);

            default:
                return Animate().ToMotion(3);
        }
    }

}
