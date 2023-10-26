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
        return Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.4).AlignY(0).MoveY(1).ToActiveShape(false, 0.00005).ToWorld(o=> { o.OverCalculationMult = 1; }).ToMotion(10);

        var n = 12;
        var actives = (n).SelectRange(i => (i, fi: i * 2 * Math.PI / n))
            .Select(v => Shapes.Stone(4, v.i).RotateToMassY().Perfecto(0.5).AlignY(0).Move(new Vector3(Math.Cos(v.fi), 0, Math.Sin(v.fi)))).ToArray();

        var statics = new[]
        {
            Surfaces.Plane(20, 20).ToOy().Perfecto(3).ToLines(1, Color.Black)
        };

        return (actives, statics).ToWorld().ToMotion(2);
    }
}