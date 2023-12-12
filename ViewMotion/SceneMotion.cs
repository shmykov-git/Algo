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
using Aspose.ThreeD.Entities;
using Shape = Model.Shape;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics.Metrics;

namespace ViewMotion;

/// <summary>
/// - docks?
/// </summary>
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
        //return new Fr[] { (1, 1) }.ToSingleConvexShape(50, dis: 1m / 4).Perfecto(4).ToNumSpots3()/*.ToLines(1, Color.Red)*/.ToMotion();

        return (100).SelectInterval(v => new Fr[] { (-1, 10), (v.i, 5) }.ToShape(256, dis: 1m/6).Perfecto().ApplyColor(Color.Red)).ToMotion();

        //.ToSingleConvexShape(256, true, (decimal)(v.v)).RotateOz(Math.PI / 2).ToLines(1, Color.Red)) ; ;.ToMotion();

        //return new Fr[] { (1, 1), (2, -2), (-11, 1, 0, 0.25), (-6, 2), (-9, 1), (4, 3), (-1, 12) }.ToSingleConvexShape().Perfecto().ToLines(1, Color.Blue).ToMotion();

        //return Polygons.FourierSeries(256, new Fr[] { (1, 1), (2, -2), (-11, 1), (-6, 2), (-9, 1), (4, 3), (-1, 12) }).ToShape().Perfecto().ToLines().ApplyColor(Color.Red).ToMotion();

        return new Fr[] { (-1, 5), (4, 1) }.ToShape(dis:1m/6).ApplyColor(Color.Red).ToMotion();

        return Ranges.Pyramid3(10).Select(v => Shapes.Cube.Move(1.01*v.x, 1.01*v.y, v.z).ApplyColor(Color.Red)).ToSingleShape().ToOy().ToMotion();

        var ss = Shapes.Plane(2,2).Centered().FlipX().AddConvexPoint(0, new Vector3(0, 0, 1), true, true).SplitPlanes(0.1).FilterGraphConvexes((pointD, convexD) => pointD % 2 == 0, 4)
            .ApplyColor(Color.Red);

        //var ss = Surfaces.Shamrock(120, 20, false, false).ApplyConvexes(Convexes.ChessHedgehogBoth(20, 120)).Perfecto()
        //    //.Scale(2 * Math.PI, -2*Math.PI, 0)//.MoveY(-Math.PI / 2)
        //    //.Transform(TransformFuncs3.Flower(2, 3, 4))
        //    .ApplyColor(Color.Red);

        return ss.Centered().ToMotion();

        return ss.ToOy().Centered().Rotate(new Vector3(1,2,3)).PutOn(1).ToActiveShape(o => { o.UseSkeleton = true; /*o.Skeleton.Type = ActiveShapeOptions.SkeletonType.CenterPoint;*/ }).ToWorldMotion();


        //return new[]
        //{
        //    //Shapes.Plane2PI(20, 20, Convexes.Hedgehog, true, ConvexTransforms.Hedgehog(p=>p.SetZ(1))).ScaleY(Math.PI * 2)/*.ReversePlanes()*/.ToLines(1, Color.Blue),//.ApplyColor(Color.Blue),,
        //    Shapes.Plane(20, 30, Convexes.ChessSquares, true, true).Scale(2*Math.PI, -Math.PI, 1).Transform(TransformFuncs3.PullOnSphere)/*.AddNormalVolume(-0.1)*//*.ToDirectLines()*/.Perfecto(),//.ToLines(1, Color.Red),
        //}.ToSingleShape().RotateOx(0.6);

        //return .ToLines(1, Color.Red).ToMotion();

        //var m = 6;
        //var n = 21;
        //var ccc = 0;
        //var s = Surfaces.Cylinder(n, m);
        //var ps = s.Points3;

        //var s0 = new Shape
        //{
        //    Points = s.Points,
        //    Convexes = Convexes.SquaresBoth(m, n),
        //};

        //var ss = (4).SelectRange(i => new Shape
        //{
        //    Points = s.Points,
        //    Convexes = Convexes.SpotSquares(m, n-1, false, true, i) //??
        //}.MoveY(i).ToLines()).ToSingleShape();

        //var sss = new Shape
        //{
        //    Points = s.Points,
        //    Convexes = Convexes.Squares(m, n)
        //};

        //return new[]
        //{
        //    //s0/*.MoveZ(-1.5)*//*.ToLines()*/.ApplyColor(Color.Blue),
        //    ss.ApplyColor(Color.Red),
        //    s/*.MoveZ(1.5).ToLines()*/.ApplyColor(Color.Green)
        //}.ToSingleShape().Centered().ToMotion();

        //return TwoBallFallingToNetMotion();
        //return TwoCubesWorldMotion();

        //var s = Shapes.Line.Perfecto();

        

        //return (new[] {
        //    //s.PutOn(4.5).Move(0.5,0,-1).ToActiveShape(o =>
        //    //{
        //    //    //o.ShowMeta = true;
        //    //    o.Mass = 3;
        //    //    o.AllowTriangulation0  =false;
        //    //}),
        //    s/*.RotateOz(0.5)*/.PutOn(2).Move(0,0,0).ToActiveShape(o =>
        //    {
        //        o.UseSkeleton = false;
        //        o.Skeleton.Power = 50;
        //        o.AllowTriangulation0 = false;
        //        o.Mass = 1;
        //        o.ShowMeta = true;
        //        o.AllowTriangulation0 = false;
        //    }),
        //    Shapes.Cube.Perfecto().Scale(5, 1, 5).PutOn(-1).MoveX(-1.95).ToActiveShape(o =>
        //    {
        //        o.ShowMeta = true;
        //        o.AllowTriangulation0 = false;
        //        o.UseSkeleton = true;
        //        o.Mass = 1;
        //        o.MaterialPower = 5;
        //        //o.Fix = ActiveShapeOptions.FixDock.Right | ActiveShapeOptions.FixDock.Left;
        //    }),
        //    //Surfaces.Plane(2, 2).GroupMembers(5).ToOy()/*.RotateOz(Math.PI/4)*/.PutOn().MoveX(-3).ToActiveShape(o =>
        //    //{
        //    //    o.Type = ActiveShapeOptions.ShapeType.D2;
        //    //    o.ShowMeta = true;
        //    //    o.AllowTriangulation0 = false;
        //    //    o.UseSkeleton = false;
        //    //    o.Mass = 1;
        //    //    o.MaterialPower = 5;
        //    //    o.Fix = ActiveShapeOptions.FixDock.Right | ActiveShapeOptions.FixDock.Left;
        //    //}),
        //},
        //new Shape[]
        //{
        //    //Shapes.CoodsWithText,
        //    //Shapes.Cube.GroupMembers().Scale(5, 1, 5).PutOn(-1).MoveX(-2.4).ToNumSpots3(),
        //}).ToWorld(o =>
        //{
        //    o.Ground.Y = -1;
        //    o.Ground.ShowGround = true;
        //    o.Ground.LineMult = 1;
        //    o.Ground.Color = Color.Green;
        //    o.Ground.ClingForce = 1;
        //    o.InteractionType = InteractionType.ParticleWithPlane;
        //    o.Interaction.ElasticForce = 5;
        //    o.Interaction.ClingForce = 1;
        //    o.Interaction.FrictionForce = 1;
        //    o.Interaction.UseVolumeMass = false;
        //}).ToMotion(8);
    }
}