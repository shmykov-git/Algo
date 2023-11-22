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
        //var from = 0;
        //var n = 21;
        //return (n-3).SelectSquarePoints((i, x, y) =>
        //{
        //    var ps = (i+3+from).SelectCirclePoints((i, x, y) => new Vector3(x, y, 0)).ToArray();
        //    var s = new Shape
        //    {
        //        Points3 = ps,
        //        Convexes = new[] { ps.Index().ToArray() }
        //    };

        //    //var s = new Shape { Points = ss.Points, Convexes = ss.Convexes.Take(1).ToArray() }.Normalize(false, false).Centered();
        //    var ts = s.TriangulateByFour();

        //    return (ts.ApplyColor(Color.Blue) + ts.ToMetaShape3(3, 3, Color.Red, Color.Green) + /*s.ToNumSpots3() + */vectorizer.GetTextLine(ts.Convexes.Length.ToString()).Centered().Mult(0.5).MoveY(1.6)).Centered().Move(4*x, 4*y, 0);
        //}).ToArray().ToSingleShape().ToMotion();

        return BallToPyramidWorldMotion();

        //return TwoCubesWorldMotion();
        //return WorldInteractionMotion();

        //return Shapes.Cube.Perfecto().PutOn(1).ToActiveShape(o => { o.RotationSpeedAngle = 0.01; }).ToWorld().ToMotion();
        //return (10).SelectCirclePoints((y, x, z) => Shapes.IcosahedronSp2.Perfecto().ScaleY(0.5).PutOn().Move(0.3 * x, y * 0.8, 0.3 * z)).ToWorldMotion();


        //var shape = Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.4).Rotate(1, 2, 3).AlignY(0).MoveY(1).ApplyColorGradientX(Color.Blue, Color.Red);
        //var a = Shapes.Cube.Perfecto().SplitPlanes(0.4).PutOn().Move(-1, 1, 0).ApplyColorGradientX(Color.White, Color.Green);
        //var b = Shapes.Cube.Perfecto().SplitPlanes(0.4).PutOn().Move(1, 1, 0).ApplyColorGradientX(Color.Blue, Color.White);

        //var n = 50;
        //var fixZPos = 14;

        //var actives = new[]
        //{
        //    Shapes.IcosahedronSp2.PutOn().MoveY(1).ApplyColorGradientX(Color.Blue, Color.White)
        //    .ToActiveShape(o =>
        //        {
        //            o.UseSkeleton = true;
        //            //o.UseSelfInteractions = true;
        //        }),


        //    //(20, 5, 1).SelectRange((y, j, k) => Shapes.NativeCubeWithCenterPoint.Move(y, j, k)).ToSingleShape().NormalizeWith2D().Centered()
        //    //    .Mult(0.2)
        //    //    .PullOnSurface(SurfaceFuncs.Cylinder)
        //    //    .Mult(5)
        //    //    //.Where(v => v.z < fixZPos + 1.5)
        //    //    .Perfecto(2)
        //    //    .ScaleY(0.5)
        //    //    .PutOn()
        //    //    .ToActiveShape(o =>
        //    //    {
        //    //        //o.UseSkeleton = true;
        //    //        o.UseSelfInteractions = true;
        //    //    }),
        //};



        //var r = 1;
        //var actives = (6).SelectCirclePoints((i, x, z) => Shapes.Stone(4, i + 20).Perfecto(2).RotateToTopY().PutOn().Move(r * x, 0.3, r * z).ToActiveShape(o =>
        ////var actives = (2).SelectCirclePoints((i, x, z) => Shapes.Stone(4, i + 20).Perfecto(2)/*.RotateToMassY()*/.PutOn().Move(0, i * 1.7, 0).ToActiveShape(o =>
        ////var actives = (2).SelectRange(i=> Shapes.Cube.SplitPlanes(0.5)/*.Rotate(i*0.7, new Vector3(1,1,1).Normalize())*/.PutOn().Move(i*0.1, i*1.6, i*0.1).ToActiveShape(o =>
        ////var actives = (1).SelectCirclePoints((i, x, z) => Shapes.Stone(4, 21).Perfecto(2).RotateToTopY().PutOn().Move(0, 0.3, 0).ToActiveShape(o =>
        //{
        //    o.Skeleton.Power = 5;
        //    //o.Speed = Math.Pow(-1, y) * 0.002 * new Vector3(x, 0, z).MultV(Vector3.YAxis);
        //    //o.UseSkeleton = true;
        //    //o.UseSelfInteractions = true;
        //    //o.Mass = i % 2 == 0 ? 2 : 1;
        //})).ToArray();

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

        //var statics = new Shape[]
        //    {
        //        //Shapes.IcosahedronSp2.Perfecto().ApplyColor(Color.Red)
        //    };



        //var actives = new[]
        //{
        //    Shapes.Cube.SplitPlanes(1).PutOn().MoveY(1.3).ApplyColorGradientX(Color.White, Color.Blue).ToActiveShape(o =>
        //    {
        //        o.Skeleton.Power = 3;
        //        o.MaterialPower = 3;
        //        o.RotationSpeedAngle = 0.002;
        //        //o.MaterialThickness = 0.1;
        //    }),
        //    Shapes.Cube.SplitPlanes(1).PutOn().ApplyColorGradientX(Color.White, Color.Red).ToActiveShape(o =>
        //    {
        //        o.Skeleton.Power = 3;
        //        o.MaterialPower = 3;
        //        //o.MaterialThickness = 0.1;
        //    }),

        //    //Shapes.Stone(4, 21).Perfecto(2).RotateToTopY().ToOy().ToOx().PutOn().MoveY(1),
        //    //Shapes.Stone(4, 25).Perfecto(2).RotateToTopY().ToOy().PutOn(),
        //}
        ////.Select(s => s.ToActiveShape(o =>
        ////{
        ////    o.Skeleton.Power = 10;
        ////    o.MaterialPower = 10;
        ////    //o.MaterialThickness = 0.1;
        ////}))
        //.ToArray();

        //return (actives, statics).ToWorld(o =>
        //    {
        //        o.UseGround = true;                
        //        o.Interaction.MaterialClingForce = 0.1;
        //        o.Interaction.MaterialFrictionForce = 0.1;
        //        o.Interaction.PlaneForce = 4;

        //        //o.Ground.GravityPower = 0.6;
        //        //o.UseMassCenter = false;
        //        //o.Interaction.EdgeSize = 0.2;
        //        //o.Interaction.SelfInteractionGraphDistance = 10;
        //        //o.Interaction.ParticleForce = 10;
        //        //o.UseInteractions = true;
        //        //o.MassCenter.GravityConst = 0.00002;
        //        //o.Interaction.EdgeSize = 0.4;
        //    }).ToMotion(10);
    }
}