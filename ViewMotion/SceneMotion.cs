using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
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
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;
using Model.Tools;
using System.Drawing.Text;
using System.Threading.Tasks.Sources;
using Model.Graphs;
using Model3D.Actives;

namespace ViewMotion;

partial class SceneMotion
{
    #region ctor

    private readonly Vectorizer vectorizer;
    private readonly Random rnd;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
        this.rnd = new Random(0);
    }

    #endregion

    public Task<Motion> Scene()
    {
        return (
            new[]
            {
                Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.4).AlignY(0).MoveY(1)
                .ToActiveShape(o =>
                {
                    o.BlowPower = 0.00005;
                    o.StepModifyFn = a =>
                    {
                        //a.Options.BlowPower += 0.00005 / 100;
                    };
                })
            }, new [] 
            {
                vectorizer.GetText("Пора спать").Perfecto(2).AlignY(0).MoveY(2).ApplyColor(Color.SandyBrown)
            }
            ).ToWorld(o =>
            {
                Shape? g = null;

                o.AllowModifyStatics = true;
                o.OverCalculationMult = 1;
                o.StepModifyFn = w =>
                {
                    //w.ActiveShapes[0].Options.BlowPower += 0.00005 / 100;

                    g ??= w.Shapes[^1];
                    var t = w.Options.StepNumber * 0.0001;
                    var mult = 0.2;
                    w.Shapes[^1] = g.ToOyM().Mult(mult).ApplyZ(Funcs3Z.Waves2(t)).Mult(1/mult).ToOy();
                };
            }).ToMotion(10);

        var n = 12;
        var actives = (n).SelectRange(i => (i, fi: i * 2 * Math.PI / n))
            .Select(v => Shapes.Stone(4, v.i).RotateToMassY().Perfecto(0.5).AlignY(0).Move(new Vector3(Math.Cos(v.fi), 0, Math.Sin(v.fi)))).ToArray();

        var statics = new[]
        {
            Surfaces.Plane(20, 20).ToOy().Perfecto(3).ToLines(1, Color.Black)
        };

        return (actives, statics).ToWorld().ToMotion(2);
    }
}