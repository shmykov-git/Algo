
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
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
using ViewMotion.Extensions;
using ViewMotion.Models;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Item = Model3D.Systems.WaterSystemPlatform.Item;

namespace ViewMotion;

partial class SceneMotion
{
    #region ctor

    private readonly Vectorizer vectorizer;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
    }

    #endregion

    public Task<Motion> Scene()
    {
        var options = new WaterfallOptions()
        {
            SceneSize = new Vector3(12, 15, 12),
            GutterCurvature = 0.4,
            //GutterRotation = new Vector3(0.05, 6, 1),
            ParticleInitCount = 500,
            SceneMotionSteps = 100,
            StepAnimations = 10,
            PlatformColor = Color.FromArgb(64, 0, 0),
            SphereColor = Color.FromArgb(64, 0, 0),
            GutterColor = Color.FromArgb(64, 0, 0),
        };

        var rnd = new Random(options.Seed);

        var cubeSize = options.SceneSize;
        var particleRadius = options.ParticleRadius;

        var moveShpereZ = 0;
        var sphere = Shapes.Ball.Perfecto(options.SphereRadius).Where(v => v.y > -0.4).MoveY(-cubeSize.y / 2).MoveZ(moveShpereZ).Move(options.SphereOffset).ApplyColor(options.SphereColor);
        var logicSphere = Shapes.IcosahedronSp2.Perfecto().Perfecto(options.SphereRadius).Where(v => v.y > -0.1).MoveY(-cubeSize.y / 2).MoveZ(moveShpereZ).Move(options.SphereOffset).MovePlanes(-particleRadius);

        Shape GetGutter(Vector3 scale, Vector3 rotation, Vector3 move)
        {
            var gutterTmp = Surfaces.Plane(20, 2).Perfecto().FlipY().Scale(scale).AddPerimeterVolume(.6);
            gutterTmp = options.GutterCurvature.Abs() < 0.001
                ? gutterTmp.MoveZ(-2.5)
                : gutterTmp.MoveZ(-2 / options.GutterCurvature).ApplyZ(Funcs3Z.CylinderXMR(4 / options.GutterCurvature))
                    .MoveZ(6 / options.GutterCurvature - 2.5);
            var gutter = gutterTmp.Centered().Rotate(rotation).Move(move).ApplyColor(options.GutterColor);
            
            return gutter;
        }

        var gutter1 = GetGutter(new Vector3(4, 80, 1), new Vector3(0.1, 6, 1), new Vector3(0, cubeSize.y / 2 - 3, -2));
        var gutter2 = GetGutter(new Vector3(4, 40, 1), new Vector3(-0.1, 6, -1), new Vector3(0, cubeSize.y / 2 - 10, 3));

        //var logicGutter = gutter.AddBorder(-particleRadius);

        var models = new List<WaterCubePlaneModel>
        {
            new() {VisibleShape = sphere, ColliderShape = logicSphere},
            new() {VisibleShape = gutter1, ColliderShape = gutter1, ColliderShift = -particleRadius},
            new() {VisibleShape = gutter2, ColliderShape = gutter2, ColliderShift = -particleRadius},
        };

        Item[] GetInitItems(int n) => (n).SelectRange(_ => new Item
        {
            Position = rnd.NextCenteredV3(1.5) + new Vector3(0, cubeSize.y / 2 - 1, -3) + options.WaterPosition +
                       options.WatterOffset
        }).ToArray();

        return WaterSystemPlatform.CubeMotion(
            new WaterCubeModel()
            {
                PlaneModels = models,
                GetInitItemsFn = GetInitItems
            }, options).ToMotion(25);
    }

    public Task<Motion> Scene1()
    {
        return Waterfall();


        //return MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().ToMetaShape3().ApplyColor(Color.Red)
        //    .ToMotion();

        ////Func<Vector3, bool> solidFn = v => v.x.Pow2() + v.y.Pow2() + v.z.Pow2() < 1 ;
        //Func<Vector3, bool> solidFn = v => MandelbrotQuaternionFractalSystem.CheckBounds(new Model4D.Quaternion(v.x, v.y, v.z, 0), 1000);
        //var step = 0.05;
        //var ss = Surfer.FindSurface(solidFn, step)/*.Where(v => v.x >= 0 && v.y >= 0)*/;

        //return new[]
        //{
        //    ss.ApplyColor(Color.Blue),
        //    //ss.ToSpots3(3*step).ApplyColor(Color.Blue),
        //    //ss.ToShapedSpots3(Shapes.Cube.MassCentered().Mult(0.5 * step).ToLines(step), Color.Green),
        //    //MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().ToSpots3().ApplyColor(Color.Red),
        //    //Surfaces.Sphere(40, 20).ToLines(0.1, Color.Red),
        //    Shapes.CoodsWithText
        //}.ToSingleShape().ToMotion();

        //var n = 50;
        //double Fn(int k) => 3.0 * k / (n - 1) - 1.5;

        //var vs = (n, n, n).SelectRange((a, b, c) => new Model4D.Quaternion(
        //        Fn(a),
        //        Fn(b),
        //        Fn(c),
        //        0
        //        ))
        //    .Where(q => MandelbrotQuaternionFractalSystem.CheckBounds(q, 1000)).ToArray();

        //var point = Shapes.Dodecahedron.Mult(0.04);

        //var s = vs.Select(v => point.Move(v.x, v.y, v.z)).ToSingleShape();

        //return new[]
        //{
        //    s.ApplyColor(Color.Blue),
        //    MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().ToSpots3().ApplyColor(Color.Red),
        //    Shapes.CoodsWithText
        //}.ToSingleShape().ToMotion();

        //return Shapes.Ball.Perfecto(0.1).TransformPoints(p => new Quaternion(1, p.x, p.y, p.z).Normalize().EulerAngles()).Perfecto().ToMetaShape3(0.1, 0.1, Color.Blue, Color.Red).ToMotion();

        //var net = Parquets.Triangles(50, 100).ToShape3().Perfecto(1.5);

        //var fShape = new Fr[]
        //    {(-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 2), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1)};


        //return .ToMetaShape3(0.1, 0.1, Color.Blue, Color.Green).ToMotion();

        //var s = new Fr[] { (-41, 0.25), (-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 1.8), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1) }.ToShape(1000, 0.05).ApplyColor(Color.Blue);//.ToShape().Perfecto().ToPolygon();
        ////var polygon = MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().Perfecto().ToPolygon();

        //var polygon = new Fr[] { (-41, 0.25), (-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 1.8), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1) }.ToShape().Perfecto().ToPolygon();
        //var catNet = net.Cut(polygon);
        //var joinNet = catNet.ToShape2()

        //return net.ApplyZ(Funcs3Z.Waves).ApplyColor(Color.Blue).ToMotion();
        //var fn = polygon.VolumeFn(0.25);
        //var cutPlane = Surfaces.Plane(100, 100).Perfecto().Cut(polygon);
        //var plane = cutPlane.ApplyZ(fn);
        //var backPlane = cutPlane.ApplyZ(fn.Minus());

        //var s = plane.ApplyColor(Color.Blue)/*.WithBackPlanes(Color.Green)*/ + backPlane.ReversePlanes().ApplyColor(Color.Green).WithBackPlanes(Color.Blue);

        //Shape GetShape(Matrix4 q)
        //{
        //    return Surfaces.Sphere(20, 20).Perfecto().TransformPoints(p => q * p).ToLines(1, Color.Blue) + Shapes.CoodsWithText;
        //}

        IEnumerable <Shape> Animate()
        {
            //yield return s;
            //return (101).Range(i => GetQ(i / 100.0)).Select(GetShape);
            yield return vectorizer.GetContentShape("z11").ApplyColor(Color.Black);
            //return (75).SelectRange(i => vectorizer.GetContentShape("t5", new ShapeOptions() { ZVolume = 0.02, ColorLevel = 50 + 2*i }).ApplyColor(Color.Red));
        }

        return Animate().ToMotion(2);

        //var s = Surfaces.Plane(10,10).Perfecto().AddVolumeZ(0.5).ApplyColor(Color.Blue);

        //return new[]
        //{
        //    s,

        //}.ToMotion();


        //var s0 = vectorizer.GetContentShape("lenin1").ApplyColor(Color.Blue);

        //return (100).Range()
        //    .Select(i => vectorizer.GetContentShape("lenin1", new ShapeOptions(){ZVolume = 0.02, SmoothOutLevel = i}).ApplyColor(Color.Blue))
        //    .ToMotion();

        //s0.ApplyZ(Funcs3Z.Hyperboloid)

        //return (20).Range()
        //    .Select(i => (3 + Math.Sin(2 * Math.PI * i/20))/4)
        //    .Select(d => s0.Mult(d).ApplyZ(Funcs3Z.Hyperboloid).Mult(1 / d))
        //    .ToMotion();


    }
}