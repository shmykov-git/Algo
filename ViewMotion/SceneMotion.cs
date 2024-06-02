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
        var fnH = SurfaceFuncs.Hyperboloid;
        SurfaceFunc fn = (u, v) => (fnH(u, v) + new Vector3(2, 2, 4)).MultC(new Vector3(0.25, 0.25, 0.125));

        //return (new Shape()
        //{
        //    Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, (u,v)=>fn(u,v)).ToArray(),
        //    Convexes = Convexes.SquaresBoth(10, 10)
        //}.ToMeta()+Shapes.CoodsWithText()).ToMotion();

        (float[] input, float[] expected)[] training = (10, 10)
            .SelectInterval(-2, 2, -2, 2, (x, y) => fn(x, y).ToFloat())
            .Select(v => (new float[] { v.x, v.y }, new float[] { v.z }))
            .ToArray();

        //(float[] input, float[] expected)[] training = [([], [])];


        var o = new NOptions()
        {
            Seed = 0,
            NInput = 2,
            NHidden = (11, 5),
            NOutput = 1,
            BaseWeightFactor = (0.5f, -0.25f),
            Alfa = 0.2f,
            Nu = 0.1f,
            FillFactor = 0.9f,
            LinkFactor = 0.9f
        }.With(o => o.Training = training);

        var brain = new NNet(o);

        Vector3 BrainFn(double xx, double yy)
        {
            var x = (float)(xx + 2) * 0.25f;
            var y = (float)(yy + 2) * 0.25f;
            var res = brain.Predict([x, y]);

            return new Vector3(x, y, res[0]);
            //return new Vector3(4 * res[0] - 2, 4 * res[1] - 2, 4 * res[2] - 2);
        }

        brain.Init();
        //var y = BrainFn(1, 1);
        brain.ShowDebug();

        //(5).ForEach(brain.Train);
        //var ys = (10, 10).SelectInterval(-2, 2, -2, 2, BrainFn).ToArray();
        //brain.ShowDebug();


        var nEpoch = 100000;
        var part = 100;

        IEnumerable<Shape> Animate() 
        {
            for (var k = 0; k < nEpoch/part; k++)
            {
                (part).ForEach(_ => brain.Train());
                //brain.ShowDebug();

                yield return new[]
                {
                    new Shape()
                    {
                        Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, BrainFn).ToArray(),
                        Convexes = Convexes.SquaresBoth(10, 10)
                    }.ToMeta(),
                    new Shape()
                    {
                        Points3 = (10, 10).SelectInterval(-2, 2, -2, 2, (u,v)=>fn(u,v)).ToArray(),
                        Convexes = Convexes.SquaresBoth(10, 10)
                    }.ToLines(Color.Green, 0.5),
                    //Shapes.Coods()
                }.ToSingleShape().Centered();
            }
        }

        return Animate().ToMotion(2);
    }
}