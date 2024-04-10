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
        return ThreeBallsRace();

        return  (Surfaces.Slide(40, 10, 0.5, 0.2, 0.5, 0.8, 0.6).ToMetaShape3(0.3, 0.3) + Shapes.CoodsWithText).ToMotion();

        Vector2[][] GetBps(double x, double? y = null) => 
        [
            y == null 
            ? [(0, 0.5), (0, 0.5*x), (x, 0)]
            : [(0, 0.5), (0, 0.5 * x), (0.625, y.Value), (x, 0)],
            [(1, 0)],
        ];

        var coods = Shapes.Coods2WithText;

        return (100).SelectInterval(0.05, 0.95, x =>
        {
            var bps = GetBps(x, 1-x);
            var fn = bps.ToBz();
            var ps = (1000).SelectInterval(1, x => fn(x));

            return new[]
            {
                bps.Select(aa=>aa[0]).ToArray().ToShape().ToPoints(Color.Green, 1),
                bps.SelectMany(aa=>aa.Skip(1)).ToArray().ToShape().ToPoints(Color.Yellow, 1),
                ps.ToShape2().ToShape3().ToLines(Color.Blue),
                coods
            }.ToSingleShape().Move(-0.5, -0.5, 0);
        }).ToMotion(1.5);
    }
}