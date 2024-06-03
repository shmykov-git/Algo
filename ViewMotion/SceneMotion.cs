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
        // положить куб в куб
        
        var m = 0.5f;
        //var external = 1.8f;
        var trainN = 10;
        (double from, double to) trainR = (-2, 2);
        var modelN = 30;
        (double from, double to) modelR = (-4, 4);

        var nEpoch = 500000;
        var nEpochPart = 200;

        var options = new NOptions()
        {
            Seed = 1,
            NInput = 2,
            NHidden = (31, 1),
            NOutput = 1,
            Weight0 = (0.00001, -0.000005),
            Nu = 0.1,
            ShaffleFactor = 0.01,
            PowerFactor = 100,
            FillFactor = 0.6,
            LinkFactor = 0.4
        };

        Func<double, double, Vector3> Boxed(SurfaceFunc fn, Vector3 move, Vector3 scale) => (u, v) => (fn(u, v) + move).MultC(scale) + new Vector3(0.5, 0.5, 0.5);

        var TrainFn = Boxed(SurfaceFuncs.Hyperboloid, new Vector3(0, 0, 0), m * new Vector3(1 / (trainR.to - trainR.from), 1 / (trainR.to - trainR.from), 0.125));
        //var TrainFn = Boxed(SurfaceFuncs.Paraboloid, new Vector3(0, 0, -4), m * new Vector3(1 / (trainRange.to - trainRange.from), 1 / (trainRange.to - trainRange.from), 0.125));

        //return (new Shape()
        //{
        //    Points3 = (trainN, trainN).SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, TrainFn).ToArray(),
        //    Convexes = Convexes.SquaresBoth(trainN, trainN)
        //}.ToMeta() + Shapes.NativeCube.ToLines()).ToMotion();

        var training = (trainN, trainN)
            .SelectInterval(trainR.from, trainR.to, trainR.from, trainR.to, (x, y) => TrainFn(x, y))
            .Select(v => (new double[] { v.x, v.y }, new double[] { v.z }))
            .ToArray();

        var brain = new NBrain(options.With(o => o.Training = training));
        brain.Init();

        NModel model = brain.model.Clone();
        Debug.WriteLine($"Brain: n={model.ns.Count()} e={model.es.Count()} ({model.input.Length}->{model.output.Length})");

        //return model.GetTopology().ToShape3().Perfecto(3).ToMeta(Color.Red, Color.Blue).ToMotion();

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

        IEnumerable<Shape> Animate() 
        {
            yield return GetShape();

            for (var k = 0; k < nEpoch / nEpochPart; k++)
            {
                var err = double.MaxValue;

                (nEpochPart).ForEach(_ =>
                {
                    var newErr = brain.Train();

                    if (newErr < err)
                    {
                        err = newErr;
                        model = brain.model.Clone();

                        model.ShowDebugE();

                        if (err < bestErr)
                        {
                            bestErr = err;
                            Debug.WriteLine($"bestErr: {err} [{k + 2}]");
                        }
                        else
                        {
                            Debug.WriteLine($"err: {err}");
                        }
                    }
                });

                yield return GetShape();
            }
        }

        return Animate().ToMotion(3);
    }
}