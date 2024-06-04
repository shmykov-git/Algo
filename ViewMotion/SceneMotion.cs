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

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        // оптимизация сети DuplicateFactor = 2
        // положить куб в куб
        
        var m = 0.9f;
        //var external = 1.8f;
        var trainN = 10;
        (double from, double to) trainR = (-2, 2);
        var modelN = 30;
        (double from, double to) modelR = (-2/m, 2/m);

        var nEpoch = 500000;
        var nEpochPart = 200;

        var options = new NOptions()
        {
            Seed = 2,
            NInput = 2,
            NHidden = (20, 1),
            NOutput = 1,
            DuplicatorsCount = 3,
            Weight0 = (0.001, -0.0005),
            ShaffleFactor = 0.01,
            Nu = 0.1,
            PowerFactor = 100,
            //FillFactor = 0.5,
            LinkFactor = 0.4,
        };

        Func<double, double, Vector3> Boxed(SurfaceFunc fn, Vector3 move, Vector3 scale) => (u, v) => (fn(u, v) + move).MultC(scale) + new Vector3(0.5, 0.5, 0.5);

        var TrainFn = Boxed(SurfaceFuncs.Wave(0, 40), new Vector3(0, 0, 0), m * new Vector3(1 / (trainR.to - trainR.from), 1 / (trainR.to - trainR.from), 0.25));
        //var TrainFn = Boxed(SurfaceFuncs.Hyperboloid, new Vector3(0, 0, 0), m * new Vector3(1 / (trainR.to - trainR.from), 1 / (trainR.to - trainR.from), 0.125));
        //var TrainFn = Boxed(SurfaceFuncs.Paraboloid, new Vector3(0, 0, -4), m * new Vector3(1 / (trainR.to - trainR.from), 1 / (trainR.to - trainR.from), 0.125));

        //return (new Shape()
        //{
        //    Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
        //    Convexes = Convexes.SquaresBoth(trainN, trainN)
        //}.ToMeta() + Shapes.NativeCube.ToLines()).ToMotion();

        var training = (trainN, trainN)
            .SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, (x, y) => TrainFn(x, y))
            .Select(v => (new double[] { v.x, v.y }, new double[] { v.z }))
            .ToArray();

        var trainer = new NTrainer(options.With(o => o.Training = training));
        trainer.Init();


        void SpliteN(int k)
        {
            //trainer.CleanupTrainTails();
            trainer.model.HorizontalSplit(trainer.model.ns[k]);
        }

        void AddE(int i, int j)
        {
            trainer.model.AddE(trainer.model.ns[i], trainer.model.ns[j]);
        }

        (int time, Action modify)[] plan =
        [
            (10, () => SpliteN(2)),
            (10, () => SpliteN(4)),
            (10, () => SpliteN(6)),
            (10, () => SpliteN(8)),
            (10, () => SpliteN(10)),
            (20, () => SpliteN(12)),
            (20, () => SpliteN(14)),
            (20, () => SpliteN(16)),
            (20, () => SpliteN(18)),
            (30, () => AddE(4, 3)),
            (30, () => AddE(4, 7)),
            (30, () => AddE(6, 3)),
            (30, () => AddE(10, 20)),
            (30, () => AddE(12, 19)),
            (30, () => AddE(14, 7)),
            (30, () => AddE(18, 11)),
            (250, () => AddE(8, 5)),
        ];

        Shape GetTopologyShape() =>
            trainer.model.GetTopology().ToShape3().Perfecto(3).ToNumSpots3(0.5) +
            trainer.model.GetTopology().ToShape3().Perfecto(3).ToMeta(Color.Red, Color.Blue);

        NModel model = trainer.model.Clone();
        Debug.WriteLine($"Brain: n={model.ns.Count()} e={model.es.Count()} ({model.input.Length}->{model.output.Length})");

        //return Topology().ToMotion();

        Vector3 ModelFn(double xx, double yy)
        {
            var x = (float)(m * xx - trainR.from) / (trainR.to - trainR.from);
            var y = (float)(m * yy - trainR.from) / (trainR.to - trainR.from);
            var z = model!.Predict([x, y])[0];

            return new Vector3(x, y, z);
        }


        var bestErr = double.MaxValue;

        Shape GetShape() => new[]
        {
            new Shape()
            {
                Points3 = (modelN, modelN).SelectInterval(modelR.from, modelR.to, modelR.from, modelR.to, ModelFn).ToArray(),
                Convexes = Convexes.Squares(modelN, modelN)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToPoints(Color.Red, 0.7),
            new Shape()
            {
                Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
                Convexes = Convexes.Squares(trainN, trainN)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToLines(Color.Blue),
            Shapes.Cube.Mult(2).ToLines(Color.Black)
        }.ToSingleShape();

        var planI = 0;
        var planCount = 0;
        var planStartI = 50;

        IEnumerable<Shape> Animate() 
        {
            yield return GetTopologyShape();

            yield return GetShape();

            for (var k = 0; k < nEpoch / nEpochPart; k++)
            {
                var err = double.MaxValue;
                var errChanged = false;
                var bestErrChanged = false;

                if (planStartI < k+3)
                {
                    if (planI < plan.Length)
                    {
                        if (planCount == 0)
                        {
                            plan[planI].modify();
                        }

                        planCount++;

                        if (planCount == plan[planI].time)
                        {
                            planCount = 0;
                            planI++;
                        }
                    }
                    else if (planI == plan.Length)
                    {
                        yield return GetTopologyShape();
                        break;
                    }
                }

                (nEpochPart).ForEach(_ =>
                {
                    var newErr = trainer.Train();

                    if (newErr < err)
                    {
                        err = newErr;
                        errChanged = true;
                        model = trainer.model.Clone();

                        if (err < bestErr)
                        {
                            bestErr = err;
                            bestErrChanged = true;
                        }
                    }
                });

                if (errChanged)
                {
                    if (bestErrChanged)
                        Debug.WriteLine($"bestErr: {err} [{k + 3}]");
                    else
                        Debug.WriteLine($"err: {err}");

                    model.ShowDebugE();
                }

                yield return GetShape();
            }

            if (planI == plan.Length)
                yield break;
        }

        return Animate().ToMotion(3);
    }
}