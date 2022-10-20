
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
        //var s = vectorizer.GetContentShape("d13").ApplyColor(Color.DarkRed);
        var s = Surfaces.Shell(40, 10).Perfecto();

        var d = 1.005;

        Shape Step(double mult)
        {
            Vector3 Transform(Vector3 p)
            {
                p *= mult;

                if (p.x.Abs() > 0.5)
                    p.x = p.x.Sign() * 0.5;
                if (p.y.Abs() > 0.5)
                    p.y = p.y.Sign() * 0.5;
                if (p.z.Abs() > 0.5)
                    p.z = p.z.Sign() * 0.5;

                return p;
            }

            return s.TransformPoints(Transform);
        }

        IEnumerable<Shape> Animate()
        {
            return (550).Range(i=>d.Pow(i)).Select(v=> Step(v).ToMetaShape3(1, 1, Color.Red, Color.Green));
            //return (101).Range().Select(i => s.Where(v => v.y <= -0.5 + 0.01 * i).ToLines(1, Color.Red));
            //yield return vectorizer.GetContentShape("w40", 200, 0.01).ApplyColor(Color.Red);
            //return (75).SelectRange(i => vectorizer.GetContentShape("t5", new ShapeOptions() { ZVolume = 0.02, ColorLevel = 50 + 2*i }).ApplyColor(Color.Red));
        }
      
        return Animate().ToMotion(2);

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