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
using Vector3 = Aspose.ThreeD.Utilities.Vector3;
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
        var coods = Shapes.Coods2WithText;

        return (1).SelectInterval(0, 5, x =>
        {
            var a = new Bz((3, 1), (3, 2));
            var b = new Bz((4, 3), (5, 3));
            var aa = new Bz((5, 4), (4, 4));
            var bb = new Bz((2, 2), (2, 1));

            //var c = a.CanJoin2(b) ? a.Join2(b) : a.Join3(b, x, y);
            var c = a.Join2(b);
            var d = new Bz(a.b, (3, 3.5), (2.5, 3), b.a);
            var cc = aa.Join2(bb);
            var dd = new Bz(aa.b, (3,4), (2, 3), bb.a);
            var ee = aa.JoinCircle(bb);
            var ff = new Bz(aa.b, (1, 4), (2, 5), bb.a);

            var baa = b.JoinCircle(aa);
            var bba = bb.JoinCircle(a);

            Bz[] bzs = [a, c, d, b, baa, aa, cc, bb, bba, dd, ee, ff];

            var fn = bzs.ToBz();
            var ps = (2000).SelectInterval(0, 1, v => fn(v));
            var lp = bzs.LinePoints();

            return new[]
            {
                bzs.LinePoints().ToShape().ToPoints(Color.Green, 1),
                bzs.ControlPoints().ToShape().ToPoints(Color.Yellow, 1.2),
                ps.ToShape2().ToShape3().ToShapedSpots3(Shapes.Tetrahedron.Mult(0.01), Color.Blue),
                coods.Mult(5)
            }.ToSingleShape().Move(-2.5, -2.5, 0);
        }).ToMotion(new Vector3(0, 0, 7));
    }
}