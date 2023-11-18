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

    public Task<Motion> GetSkeleton(Shape s)
    {
        // предельное расположение точек - shape (объединить точки)
        // максимальная длина ребра скелетона - shape (объединить точки)
        // сохранить связи точек и новыми объединенными - shape skeleton (2d shape)
        // перейти из shape2d в shape1d (с учетом массы точек, усреднением)
        // длина ребра скелетона

        IEnumerable<Shape> Animate()
        {
            foreach(var ss in s.AddSkeleton(0.01))
            {
                yield return s.ToMetaShape3(0.05, 0.05).ApplyColor(Color.Blue) + ss/*.ToNumSpots3()*/.ToMetaShape3(0.3, 0.3).ApplyColor(Color.Red);
            }
        }

        return Animate().ToMotion(1);
    }

    public Task<Motion> Scene()
    {
        //return (Shapes.Cube.ScaleX(0.5).ToNumSpots3() + Shapes.CoodsWithText).ToMotion();
        return GetSkeleton(Shapes.Stone(4, 10).Perfecto());
        //return GetSkeleton(Surfaces.Torus(20, 10, 3, true).Perfecto());
        //return GetSkeleton(Shapes.IcosahedronSp2.Perfecto().ScaleX(0.5));

        //return Shapes.CubeT.ToMotion();

        //return Shapes.Cube.Perfecto().PutOn(1).ToActiveShape(o => { o.RotationSpeedAngle = 0.01; }).ToWorld().ToMotion();
        //return WorldInteractionMotion();
        //return (10).SelectCirclePoints((y, x, z) => Shapes.IcosahedronSp2.Perfecto().ScaleY(0.5).PutOn().Move(0.3 * x, y * 0.8, 0.3 * z)).ToWorldMotion();


        //var shape = Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.4).Rotate(1, 2, 3).AlignY(0).MoveY(1).ApplyColorGradientX(Color.Blue, Color.Red);
        //var a = Shapes.Cube.Perfecto().SplitPlanes(0.4).PutOn().Move(-1, 1, 0).ApplyColorGradientX(Color.White, Color.Green);
        //var b = Shapes.Cube.Perfecto().SplitPlanes(0.4).PutOn().Move(1, 1, 0).ApplyColorGradientX(Color.Blue, Color.White);

        var n = 50;
        var fixZPos = 14;

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



        var r = 1;
        //var actives = (6).SelectCirclePoints((i, x, z) => Shapes.Stone(4, i + 20).Perfecto(2)/*.RotateToMassY()*/.PutOn().Move(r * x, 1, r * z).ToActiveShape(o =>
        var actives = (2).SelectCirclePoints((i, x, z) => Shapes.Stone(4, i + 20).Perfecto(2)/*.RotateToMassY()*/.PutOn().Move(0, i*1.7, 0).ToActiveShape(o =>
        //var actives = (2).SelectRange(i=> Shapes.Cube.SplitPlanes(0.5)/*.Rotate(i*0.7, new Vector3(1,1,1).Normalize())*/.PutOn().Move(i*0.1, i*1.6, i*0.1).ToActiveShape(o =>
        {
            o.SkeletonPower = 10;
            //o.Speed = Math.Pow(-1, y) * 0.002 * new Vector3(x, 0, z).MultV(Vector3.YAxis);
            //o.UseSkeleton = true;
            //o.UseSelfInteractions = true;
            //o.Mass = i % 2 == 0 ? 2 : 1;
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
                //Shapes.IcosahedronSp2.Perfecto().ApplyColor(Color.Red)
            };

        return (actives, statics).ToWorld(o =>
            {
                o.UseGround = true;
                o.MaterialClingForce = 0;
                o.MaterialFrictionForce = 0;
                //o.Ground.GravityPower = 0.6;
                //o.UseMassCenter = false;
                //o.Interaction.EdgeSize = 0.2;
                //o.Interaction.SelfInteractionGraphDistance = 10;
                //o.Interaction.InteractionForce = 10;
                //o.UseInteractions = true;
                //o.MassCenter.GravityConst = 0.00002;
                //o.Interaction.EdgeSize = 0.4;
            }).ToMotion(10);
    }
}