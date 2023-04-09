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
using ViewMotion.Extensions;
using ViewMotion.Libraries;
using ViewMotion.Models;
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;


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
        var a = new Vector3(2, 3, 5);
        var b = new Vector3(0, 0, 0);
        var n = 100;

        //return new[]
        //{
        //    (1000).SelectRange(i => i / 1000.0).Select(x => new Vector2(x, Sin(x))).ToShape2().ToShape3().ToLines(1, Color.Green),
        //    (1000).SelectRange(i => i / 1000.0).Select(x => new Vector2(x, Poly2(x))).ToShape2().ToShape3().ToLines(1, Color.Blue),
        //    (1000).SelectRange(i => i / 1000.0).Select(x => new Vector2(x, Line(x))).ToShape2().ToShape3().ToLines(1, Color.Red),
        //    Shapes.Coods2WithText
        //}.ToSingleShape().Centered().ToMotion();

        var s = Shapes.Cube.ToMetaShape3(5, 5, Color.Red, Color.Green);

        IEnumerable<Shape> Animate() => (n).SelectRange(_ => s);

        return Animate().ToMotion(/*new MotionOptions() {CameraMotionOptions = CameraAnimations.FlyArround(new Vector3(2, 2, 2)) }*/);
    }

    public Task<Motion> Scene1()
    {


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