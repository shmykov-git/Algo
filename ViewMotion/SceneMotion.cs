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
        var fnH = SurfaceFuncs.Paraboloid;
        Vector3 TrainFn(double u, double v) => (fnH(u, v) + new Vector3(2, 2, 4)).MultC(new Vector3(0.25, 0.25, 0.125));

        //return (new Shape()
        //{
        //    Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, (u,v)=>fn(u,v)).ToArray(),
        //    Convexes = Convexes.SquaresBoth(10, 10)
        //}.ToMeta()+Shapes.CoodsWithText()).ToMotion();

        var training = (10, 10)
            .SelectInterval(-2, 2, -2, 2, (x, y) => TrainFn(x, y).ToFloat())
            .Select(v => (new float[] { v.x, v.y }, new float[] { v.z }))
            .ToArray();

        var o = new NOptions()
        {
            Seed = 1,
            Shaffle = 0.05f,
            CleanupPrevTrain = false,
            NInput = 2,
            NHidden = (27, 3),
            NOutput = 1,
            Weight0 = (2f, -1f),
            Alfa = 0.5f,
            Nu = 0.2f,
            FillFactor = 0.7f,
            LinkFactor = 0.3f
        }.With(o => o.Training = training);

        var brain = new NNet(o);
        brain.Init();

        NModel model = null;

        Vector3 ModelFn(double xx, double yy)
        {
            var x = (float)(xx + 2) * 0.25f;
            var y = (float)(yy + 2) * 0.25f;
            var res = model!.Predict([x, y]);

            return new Vector3(x, y, res[0]);
            //return new Vector3(4 * res[0] - 2, 4 * res[1] - 2, 4 * res[2] - 2);
        }

        var nEpoch = 100000;
        var part = 100;

        IEnumerable<Shape> Animate() 
        {
            for (var k = 0; k < nEpoch/part; k++)
            {
                var err = float.MaxValue;

                (part).ForEach(_ => 
                { 
                    var newErr = brain.Train();

                    if (newErr < err)
                    {
                        err = newErr;
                        Debug.WriteLine($"err: {err}");
                        model = brain.model.Clone();
                    }
                });

                yield return new[]
                {
                    new Shape()
                    {
                        Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, ModelFn).ToArray(),
                        Convexes = Convexes.SquaresBoth(10, 10)
                    }.ToMeta(),
                    new Shape()
                    {
                        Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, TrainFn).ToArray(),
                        Convexes = Convexes.SquaresBoth(10, 10)
                    }.ToLines(Color.Black, 0.4)
                }.ToSingleShape().Move(-0.5, -0.5, -0.5);
            }
        }

        return Animate().ToMotion(3);
    }
}