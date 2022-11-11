
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
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
using ViewMotion.Extensions;
using ViewMotion.Models;

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
        var q = Quaternion.Identity;
        //q.ToMatrix()

        Matrix4 GetQ(double a) => new Quaternion(1, a, 0, 0).Normalize().ToMatrix();

        //var polygon = new Fr[] { (-41, 0.25), (-11, 1, 0.1), (-9, 1), (-6, 2, 0.15), (-3, 1.8), (-1, 13), (1, 1), (2, -2), (4, 3), (9, -1) }.ToShape().Perfecto().ToPolygon();
        ////var polygon = MandelbrotFractalSystem.GetPoints(2, 0.002, 1000).ToShape().Perfecto().ToPolygon();

        //var fn = polygon.VolumeFn(0.25);
        //var cutPlane = Surfaces.Plane(100, 100).Perfecto().Cut(polygon);
        //var plane = cutPlane.ApplyZ(fn);
        //var backPlane = cutPlane.ApplyZ(fn.Minus());

        //var s = plane.ApplyColor(Color.Blue)/*.WithBackPlanes(Color.Green)*/ + backPlane.ReversePlanes().ApplyColor(Color.Green).WithBackPlanes(Color.Blue);
        
        Shape GetShape(Matrix4 q)
        {
            return Surfaces.Sphere(20, 20).Perfecto().TransformPoints(p => q * p).ToLines(1, Color.Blue) + Shapes.CoodsWithText;
        }

        IEnumerable<Shape> Animate()
        {
            //yield return s;
            return (101).Range(i => GetQ(i / 100.0)).Select(GetShape);
            //yield return vectorizer.GetContentShape("b17").ApplyColor(Color.Blue);
            //return (75).SelectRange(i => vectorizer.GetContentShape("t5", new ShapeOptions() { ZVolume = 0.02, ColorLevel = 50 + 2*i }).ApplyColor(Color.Red));
        }
      
        return Animate().ToMotion(10);

        //var s = Surfaces.Plane(10,10).Perfecto().AddVolumeZ(0.5).ApplyColor(Color.Blue);

        //return new[]
        //{
        //    s,

        //}.ToMotion();


        //var s0 = vectorizer.GetContentShape("lenin1").ApplyColor(Color.Blue);

        return (100).Range()
            .Select(i => vectorizer.GetContentShape("lenin1", new ShapeOptions(){ZVolume = 0.02, SmoothOutLevel = i}).ApplyColor(Color.Blue))
            .ToMotion();

        //s0.ApplyZ(Funcs3Z.Hyperboloid)

        //return (20).Range()
        //    .Select(i => (3 + Math.Sin(2 * Math.PI * i/20))/4)
        //    .Select(d => s0.Mult(d).ApplyZ(Funcs3Z.Hyperboloid).Mult(1 / d))
        //    .ToMotion();

 
    }
}