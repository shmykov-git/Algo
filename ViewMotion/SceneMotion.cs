
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

        IEnumerable<Shape> Animate()
        {
            yield return vectorizer.GetContentShape("d13").ApplyColor(Color.DarkRed);
            //return (240).SelectRange(i => Surfaces.ShamrockDynamic(240, 20, i).ApplyColor(Color.Blue).WithBackPlanes(Color.Red));
        }
      
        return Animate().ToMotion(3);

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