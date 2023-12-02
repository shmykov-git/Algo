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
        //return TwoBallFallingToNetMotion();
        //return TwoCubesWorldMotion();

        var s = Shapes.Cube.Perfecto();

        return new[] {
            //s.PutOn(4.5).Move(0.5,0,-1).ToActiveShape(o =>
            //{
            //    //o.ShowMeta = true;
            //    o.Mass = 3;
            //    o.AllowTriangulation0  =false;
            //}),
            s.PutOn(2).Move(0,0,0).ToActiveShape(o =>
            {
                o.Mass = 1;
                o.ShowMeta = true;
                o.AllowTriangulation0 = false;
            }),
            Surfaces.Plane(2, 2).Perfecto(5).ToOy()/*.RotateOz(Math.PI/4)*/.PutOn().MoveX(-3).ToActiveShape(o =>
            {
                o.Type = ActiveShapeOptions.ShapeType.D2;
                o.ShowMeta = true;
                o.AllowTriangulation0 = false;
                o.UseSkeleton = false;
                o.Mass = 1;
                o.MaterialPower = 5;
                o.Fix = ActiveShapeOptions.FixDock.Right | ActiveShapeOptions.FixDock.Left;
            }),
        }.ToWorld(o =>
        {
            o.Ground.Y = -2;
            o.Ground.ShowGround = true;
            o.Ground.LineMult = 1;
            o.Ground.Color = Color.Green;
            o.Ground.ClingForce = 1;
            o.InteractionType = InteractionType.EdgeWithPlane;
            o.Interaction.ElasticForce = 5;
            o.Interaction.ClingForce = 1;
            o.Interaction.FrictionForce = 1;
        }).ToMotion(8);
    }
}