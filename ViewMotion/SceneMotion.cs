using System;
using System.Collections.Concurrent;
using System.Drawing;
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
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Quaternion = Aspose.ThreeD.Utilities.Quaternion;
using Vector2 = Model.Vector2;
using Model.Tools;
using System.Drawing.Text;
using System.Threading.Tasks.Sources;
using Model.Graphs;
using Model3D.Actives;
using Aspose.ThreeD.Entities;
using System.Windows.Shapes;
using System.Windows;
using System.Diagnostics.Metrics;
using Aspose.ThreeD;
using Model.Bezier;
using System.Linq;

namespace ViewMotion;

partial class SceneMotion
{
    public Task<Motion> Scene()
    {
        Vector2[][] bps = [[(0, 1), (0.05, 0.8), (0.15, 0.5)], [(0.5, 0.5), (0.85, 0.5), (0.95, 1)], [(1, 0)]];

        var fn = Funcs2.Bz(bps);

        var ps = (100).SelectInterval(1, x => fn(x));

        return new[]
        {
            bps.Select(aa=>aa[0]).ToArray().ToShape().ToPoints(Color.Green, 1.5),
            bps.SelectMany(aa=>aa.Skip(1)).ToArray().ToShape().ToPoints(Color.Yellow, 1.5),
            ps.ToShape2().ToShape3().ToLines(Color.Blue),
            Shapes.Coods2WithText
        }.ToSingleShape().ToMotion();
    }
}