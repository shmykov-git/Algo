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
        var m = 0.3f;

        Func<double, double, Vector3> Boxed(SurfaceFunc fn, Vector3 move, Vector3 scale) => (u, v) => (fn(u, v) + move).MultC(scale) + new Vector3(0.5, 0.5, 0.5);

        //var TrainFn = Boxed(SurfaceFuncs.Hyperboloid, new Vector3(0, 0, 0), m * new Vector3(0.25, 0.25, 0.125));
        var TrainFn = Boxed(SurfaceFuncs.Paraboloid, new Vector3(0, 0, -4), m * new Vector3(0.25, 0.25, 0.125));

        //return (new Shape()
        //{
        //    Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, TrainFn).ToArray(),
        //    Convexes = Convexes.SquaresBoth(10, 10)
        //}.ToMeta() + Shapes.NativeCube.ToLines()).ToMotion();

        var training = (10, 10)
            .SelectInterval(-2, 2, -2, 2, (x, y) => TrainFn(x, y).ToFloat())
            .Select(v => (new float[] { v.x, v.y }, new float[] { v.z }))
            .ToArray();

        var o = new NOptions()
        {
            Seed = 1,
            Shaffle = 0.01f,
            CleanupPrevTrain = false,
            NInput = 2,
            NHidden = (11, 1),
            NOutput = 1,
            Weight0 = (4f, -2f),
            Alfa = 0.5f,
            Nu = 0.1f,
            ScaleFactor = 1f,
            FillFactor = 0.6f,
            LinkFactor = 0.4f
        }.With(o => o.Training = training);

        var brain = new NBrain(o);
        brain.Init();

        NModel model = brain.model.Clone();
        Debug.WriteLine($"Brain: n={model.ns.Count()} e={model.es.Count()} ({model.input.Length}->{model.output.Length})");

        //var topology = model.GetTopology().ToShape3().Perfecto(100);
        //return (topology.ToPoints(Color.Red, 10) + topology.ToDirectLines(10, Color.Blue)).ToMotion();

        Vector3 ModelFn(double xx, double yy)
        {
            var x = (float)(m*xx + 2) * 0.25f;
            var y = (float)(m*yy + 2) * 0.25f;
            var res = model!.Predict([x, y]);

            return new Vector3(x, y, res[0]);
            //return new Vector3(4 * res[0] - 2, 4 * res[1] - 2, 4 * res[2] - 2);
        }

        var nEpoch = 200000;
        var part = 200;
        var bestErr = float.MaxValue;

        Shape GetShape() => new[]
        {
            new Shape()
            {
                Points3 = (30, 30).SelectInterval(-6, 6, -6, 6, ModelFn).ToArray(),
                Convexes = Convexes.SquaresBoth(30, 30)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToPoints(Color.Red, 0.7),
            new Shape()
            {
                Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, TrainFn).ToArray(),
                Convexes = Convexes.SquaresBoth(10, 10)
            }.Move(-0.5, -0.5, -0.5).Mult(2).ToLines(Color.Blue),
            Shapes.Cube.Mult(2).ToLines(Color.Black)
        }.ToSingleShape();

        IEnumerable<Shape> Animate() 
        {
            yield return GetShape();

            for (var k = 0; k < nEpoch / part; k++)
            {
                var err = float.MaxValue;

                (part).ForEach(_ =>
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