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
using System.Windows;

namespace ViewMotion.Platforms.AI;

internal class AIMotionPlatform
{
    public Task<Motion> AI_Learn_2N(AI2NOptions o, NOptions nOptions)
    {
        var boxScale = o.m * new Vector3(1 / (o.trainR.to - o.trainR.from), 1 / (o.trainR.to - o.trainR.from), 1 / (o.trainR.to - o.trainR.from));
        var boxCenter = new Vector3(0.5, 0.5, 0.5);

        var boxedShape = o.learnShape.Boxed(boxScale, boxCenter);
        var convexIntersectedFns = o.learnShape.Convexes.Index().Select(o.learnShape.IntersectConvexFn).ToArray();

        Vector3?[] TrainPoints(double x, double y)
        {
            var a = new Vector3(x, y, 0);
            var b = new Vector3(x, y, 1);

            var ps = convexIntersectedFns
                .Select(fn => fn(a, b))
                .Where(p => p.HasValue)
                .Select(p => p.Value)
                .OrderBy(p => p.z)
                .Select(p => p.Boxed(boxScale, boxCenter))
                .ToArray();

            if (ps.Length == 0)
                return [];
            
            return ps.Stretch(o.zN);
        }

        (int i, double[] input, double?[] expected)[] trainData = (o.trainN, o.trainN)
            .SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, TrainPoints)
            .Where(ps => ps.Length > 0)
            .Select((ps, i) => (i, 
                 input: new double[] { ps.Where(p => p.HasValue).First().Value.x, ps.Where(p => p.HasValue).First().Value.y }, 
                 expected: ps.Select(p => p == null ? (double?)null : p.Value.z).ToArray()))
            .Where(v => o.AllowNullZ || v.expected.All(z => z.HasValue))
            .ToArray();

        var trainer = new NTrainer(nOptions.With(o => o.TrainData = trainData));
        trainer.Init();

        if (o.mode == P2NMode.Topology)
            return GetTopologyShape(trainer, o, 1).ToMotion(3);

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

        if (o.mode == P2NMode.Shape)
            return new[]
            {
                //(o.trainN, o.trainN).SelectInterval(o.trainR.from, o.trainR.to, o.trainR.from, o.trainR.to, TrainPoints)
                //    .ToSingleArray().Where(p=>p.HasValue).Select(p=>p.Value).ToArray()
                //    .ToPointsShape().Move(1.05, 0, -1.05).ToPoints(Color.Blue, 0.5),
                //boxedShape.Move(1.05, 0, -1.05).ToLines(Color.Red, 0.5),
                //Shapes.NativeCube.Move(1.05, 0, -1.05).ToLines(),

                (o.zN).Range().Select(i =>
                        trainData.Where(v=>v.expected[i].HasValue).Select(v=>new Vector3(v.input[0], v.input[1], v.expected[i].Value))
                            .ToArray().ToPointsShape().ToPoints(o.colors[i%o.colors.Length], 0.5)
                    ).ToSingleShape(),
                Shapes.NativeCube.ToLines(),
                boxedShape.ToLines(Color.Blue, 0.5)
             }.ToSingleShape().Centered().ToMotion();

        Shape GetShape(int type = 0)
        {
            var pps = (o.modelN, o.modelN).SelectInterval(o.modelR.from, o.modelR.to, o.modelR.from, o.modelR.to, ModelPoints).ToArray();
            var point = Shapes.Tetrahedron.Perfecto(1.5);

            return new[]
            {
                o.showTime ? GetTimeShape(state).MoveY(-1.4) : Shape.Empty,
                o.showError ? GetErrorShape(state).MoveY(-1.2) : Shape.Empty,
                o.showTopology ? GetTopologyShape(trainer, o).Perfecto(1.8).MoveX(-2) : Shape.Empty,
                o.showTopologyWeights ? GetTopologyWeightsShape(trainer, o) : Shape.Empty,
                (o.zN).Range().Select(i => 
                    pps.Select(ps=>ps[i]).ToArray().ToPointsShape().ToPoints(o.colors[i%o.colors.Length], 0.5, point)
                ).ToSingleShape().Move(-0.5, -0.5, -0.5).Mult(2),
                (type switch 
                    {
                        0 => boxedShape.ToLines(0.5),
                        1 => (o.zN).Range().Select(i =>
                                trainData.Where(v=>v.expected[i].HasValue).Select(v=>new Vector3(v.input[0], v.input[1], v.expected[i].Value)).ToArray().ToPointsShape().ToPoints(o.colors[i%o.colors.Length], 0.5)
                              ).ToSingleShape(),
                        _ => Shape.Empty,
                    }                   
                ).Move(-0.5, -0.5, -0.5).Mult(2).ApplyColor(Color.Blue),
                Shapes.Cube.Mult(2).ToLines(Color.Black)
            }.ToSingleShape();
        }

        async IAsyncEnumerable<Shape> Animate()
        {
            yield return GetShape();

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

                yield return GetShape(o.shapeType(k));
            }
        }

        switch (o.mode)
        {
            case P2NMode.Model:
                return GetShape().ToMotion(3);

            default:
                return Animate().ToMotion(3);
        }
    }

    public Task<Motion> AI_Learn_21(AI21Options o, NOptions nOptions)
    {
        var topologyWeightHeight = nOptions.Act switch { NAct.Sigmoid => 10, _ => 1 };

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
            .Select((v, i) => (i, new double[] { v.x, v.y }, new double?[] { v.z }))
            .ToArray();

        var trainer = new NTrainer(nOptions.With(o => o.TrainData = trainData));
        trainer.Init();

        if (o.mode == P21Mode.Topology)
            return GetTopologyShape(trainer, o, 1).ToMotion(3);

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

        Shape GetShape(bool withTrainModel = true) => new[]
        {
            o.showTime ? GetTimeShape(state).MoveY(-1.4) : Shape.Empty,
            o.showError ? GetErrorShape(state).MoveY(-1.2) : Shape.Empty,
            o.showTopology ? GetTopologyShape(trainer, o).Perfecto(1.8).MoveX(-2) : Shape.Empty,
            o.showTopologyWeights ? GetTopologyWeightsShape(trainer, o) : Shape.Empty,
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
            yield return GetShape();

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

                yield return GetShape(o.withTrain(k));
            }
        }

        switch (o.mode)
        {
            case P21Mode.Model:
                return GetShape().ToMotion(3);

            default:
                return Animate().ToMotion(3);
        }
    }

    private Shape GetTimeShape(TrainState state)
    {
        var len = state.time.TotalHours * 5;
        var n = 10;
        var m = 50;
        var mult = 0.5;

        return Shapes.CylinderR(n, m: m).ToOx().Perfecto(mult).ScaleX(2 * len / (n * mult))
            .AlignX(0).MoveX(-1)
            .ApplyColor(Color.Blue);
    }

    private Shape GetErrorShape(TrainState state)
    {
        var len = state.bestError < 0.006 ? -Math.Log(state.bestError) - 5 : 0;
        var n = 10;
        var m = 50;
        var mult = 0.5;

        return Shapes.CylinderR(n, m: m).ToOx().Perfecto(mult).ScaleX(3 * len / (n * mult))
            .AlignX(0).MoveX(-1)
            .ApplyColorSphereRGradient(2, new Vector3(-1, 0, 0), Color.Black, Color.DarkRed, Color.DarkGreen, Color.Green, Color.Green, Color.LightGreen, Color.LightGreen);
    }

    private Shape GetTopologyShape(NTrainer trainer, AIShowOptions o, double mult = 2)
    {
        var topology = trainer.model.GetTopology().Perfecto(3);

        return o.topologyNums
            ? topology.ToNumSpots3(0.5 * mult).ApplyColor(Color.Black) + topology.ToMeta(Color.Red, Color.Blue, mult, mult)
            : topology.ToMeta(Color.Red, Color.Blue, mult, mult);
    }

    private Shape GetTopologyWeightsShape(NTrainer trainer, AIShowOptions o)
    {
        var topologyWeightHeight = 1;

        var ss = o.topologyWeightNums
            ? trainer.model.GetTopologyWeights(topologyWeightHeight).ToMeta(Color.Red, Color.Blue, 0.5, 0.5) +
                trainer.model.GetTopology().ToNumSpots3(0.15, Color.Black)
            : trainer.model.GetTopologyWeights(topologyWeightHeight).ToMeta(Color.Red, Color.Blue, 1, 1);

        return ss.ToOy().Mult(2).Move(2, 0, 1);
    }
}
