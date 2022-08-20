
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
        var s = Shapes.Icosahedron.HardFaces().ApplyColor(Color.Blue);

        return new[]
        {
            s,

        }.ToMotion();


        var s0 = vectorizer.GetContentShape("b7").ApplyColor(Color.Blue);
        //s0.ApplyZ(Funcs3Z.Hyperboloid)

        return (20).Range().Select(i => (3 + Math.Sin(2 * Math.PI * i/20))/4)
            .Select(d => s0.Mult(d).ApplyZ(Funcs3Z.Hyperboloid).Mult(1 / d)).ToMotion();

        //return new[] {s}.ToMotion();
    }
}