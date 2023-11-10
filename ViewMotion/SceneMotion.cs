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
        //return WorldInteractionMotion();
        //var shape = Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.4).Rotate(1, 2, 3).AlignY(0).MoveY(1).ApplyColorGradientX(Color.Blue, Color.Red);
        //var a = Shapes.Cube.Perfecto().SplitPlanes(0.4).PutOn().Move(-1, 1, 0).ApplyColorGradientX(Color.White, Color.Green);
        //var b = Shapes.Cube.Perfecto().SplitPlanes(0.4).PutOn().Move(1, 1, 0).ApplyColorGradientX(Color.Blue, Color.White);

        var r = 5;
        var actives2 = (6).SelectCirclePoints((i, x, z) => Shapes.Stone(4, i + 20, 1, 3).Perfecto(2).PutOn().Move(r*x, 0, r*z).ToActiveShape(o =>
        {
            o.Speed = Math.Pow(-1, i) * 0.002 * new Vector3(x, 0, z).MultV(Vector3.YAxis);
            o.UseSkeleton = true;
            o.UseMaterialDamping = true;
        })).ToArray();

        //var actives = new[]
        //    {
        //        a.ToActiveShape(o =>
        //        {
        //            o.UseSkeleton = true;
        //            o.RotationSpeedAngle = 0.001;
        //            o.Speed = new Vector3(0.001, 0, 0);
        //        }),
        //        b.ToActiveShape(o =>
        //        {
        //            o.UseSkeleton = true;
        //            o.RotationSpeedAngle = 0.001;
        //            o.Speed = new Vector3(-0.001, 0, 0);
        //        })
        //    };

        var statics = new Shape[]
            {
                Shapes.IcosahedronSp2.Perfecto().ApplyColor(Color.Red)
            };

        return (actives2, statics).ToWorld(o =>
            {
                o.UseGround = false;
                o.UseSpace = true;
                
                o.UseInteractions = true;
                o.Space.GravityConst = 0.00002;
                o.EdgeSize = 0.4;
            }).ToMotion(10);
    }
}