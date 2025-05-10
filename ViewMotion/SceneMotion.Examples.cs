using System.Collections.Generic;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Model3D.AsposeModel;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems;
using Model3D.Systems.Model;
using ViewMotion.Extensions;
using ViewMotion.Models;
using static Model3D.Systems.WaterSystemPlatform;
using Model3D.Tools;
using MathNet.Numerics;
using View3D.Libraries;
using Model3D;
using Model3D.Actives;
using Model.Fourier;
using System.Diagnostics;
using System.CodeDom;
using Model.Bezier;
using Vector2 = Model.Vector2;
using Model3D.Tools.Model;

namespace ViewMotion;
/// <summary>
/// You can find some video results on this youtube channel https://www.youtube.com/channel/UCSXMjRAXWmRL4rV7wy06eOA
/// </summary>
partial class SceneMotion
{
    public Task<Motion> DominoJs()
    {
        int n = 138;
        double r = 100;

        var polygon = new Fr[] { (-4, 3), (-1, 10), (2, -14), (20, 1) }.ToPolygon(n).SmoothOutFar(1, 5).Perfecto(1.5);
        //var polygon = new Fr[] { (-1, 10), (5, 5) }.ToPolygon(n).Perfecto();
        //var polygon = new Fr[] { (-7, 1), (-3, 2), (-11, 1), (-6, 2), (-9, 1), (4, 2), (-1, 10) }.ToPolygon(n).SmoothOut(10).Perfecto(1.71);

        IEnumerable<(int i, Vector2 p)> iterator = polygon.Select((p, i) => (i, p));

        ((int x, int y, int z) size, (double x, double y, double z) center, Quaternion q)[] data =
            iterator.SelectCircleTriple((a, b, c) => (a, b, c))
                .Select(v => (v.b.i, v.b.p, l: v.c.p - v.a.p))
                .Select(v => ((2, 15, 1), (r * v.p.x, 7.5, r * v.p.y), Quaternion.FromRotation(Vector3.ZAxis, new Vector3(v.l.x, 0, v.l.y).Normalize()))).ToArray();

        var s = data.Select(v =>
            Shapes.PerfectCube
                .Centered()
                .Scale(v.size.x, v.size.y, v.size.z)
                .Rotate(v.q)
                .Move(v.center.x, v.center.y, v.center.z)).ToSingleShape();

        var str = data.Select(v => $"[[{v.size.x}, {v.size.y}, {v.size.z}], [{v.center.x}, {v.center.y}, {v.center.z}], [{v.q.x}, {v.q.y}, {v.q.z}, {v.q.w}]]").SJoin(", ");
        Debug.WriteLine($"[{str}]");

        return s.ApplyColor(Color.Blue).ToMotion(50);
    }

    public Task<Motion> SpiralShapeJs()
    {
        var n = 6;
        var zK = 5;
        return Shapes.Plane(5 * n * zK, 16, Convexes.ChessSquares, false, true)
            .Scale(2 * Math.PI, n * 2 * 3.1 / Math.PI, 1)
            .PullOnSurface(SurfaceFuncs.Cylinder)
            .ToOx()
            .MoveY(zK)
            .PullOnSurface(SurfaceFuncs.Spiral.Scale(1, 1, 0.7))
            .ToOy().ApplyColor(Color.Red)
            //.AddNormalVolume(-0.05, Color.Red, Color.Green, Color.Black)
            .DebugJs()
            //.ToMeta()
            .ToMotion();
    }

    public Task<Motion> BezierText()
    {
        return (1).SelectInterval(10, 200, x =>
        {
            var bzs = vectorizer.GetTextBeziers("abcd", new BezierTextOptions()
            {
                FontSize = 300,
                FontName = "Royal Inferno"
            }
            .With(BezierValues.MediumLetterOptions)
            .With(o =>
            {
                o.DebugFillPoints = true;
                o.DebugProcess = true;
            })
            );

            var fpss = bzs.Select(b => { var fn = b.ToFn(); return (b.Length * 100).SelectInterval(x => fn(x)); }).ToArray();
            var m = 0.5;

            return new[]
            {
                //options.cps.Select(p=>p.ToShape2().ToShape3().ToPoints(m*0.36, Color.Yellow)).ToSingleShape(),
                //options.aps.Select(p=>p.ToShape2().ToShape3().ToPoints(m*0.32, Color.Green)).ToSingleShape(),
                //options.ps.Select(p=>p.ToShape2().ToShape3().ToPoints(m*0.3, Color.Blue)).ToSingleShape(),

                //options.ps.Select(p=>p.ToShape2().ToShape3().ToNumSpots3(m*0.1, Color.Blue)).ToSingleShape(),

                //options.lps.Select(p=>p.ToShape2().ToShape3().ToPoints(m*0.34, Color.Red)).ToSingleShape(),
                fpss.Select(fps => fps.ToShape2().ToShape3().ToPoints(m*0.1, Color.Red)).ToSingleShape(),
            }.ToSingleShape().Perfecto();
        }).ToMotion2D(1);
    }


    public Task<Motion> BezierPower2To3()
    {
        Vector2[][] bps =
        [
            [(0, 0.5), (0, 1)],
            [(0.5, 1), (1, 1)],
            [(1, 0.5), (1, 0)],
            [(0.5, 0), (0, 0)],
        ];

        var bzs = bps.ToBzs(true);
        bzs[0] = bzs[0].ToPower3();
        var fn = bzs.ToFn();
        var ps = (1000).SelectInterval(1, x => fn(x), true);

        var circle = Funcs2.CircleY();
        var cps = (1000).SelectInterval(2 * Math.PI, x => 0.498 * circle(x) + (0.5, 0.5), true);

        return new[]
        {
            cps.ToShape2().ToShape3().ToLines(0.3, Color.Red),
            bzs.LinePoints().ToShape().ToPoints(Color.Green, 1.5),
            bzs.ControlPoints().ToShape().ToPoints(Color.Yellow, 1.5),
            ps.ToShape2().ToShape3().ToLines(0.3, Color.Blue),
            Shapes.Coods2WithText()
        }.ToSingleShape().Move(-0.5, -0.5, 0).ToMotion(1.5);
    }

    public Task<Motion> BezierLikeEllipse()
    {
        var size = 8;
        var coods = Shapes.Coods2WithText(size, Color.Red, Color.DarkGray);
        var point = Shapes.Tetrahedron.Mult(0.015);

        return (100).SelectInterval(-0.9, 3, x =>
        {
            var a0 = new Bz((4, 2));
            var b0 = new Bz((5 + x, 3));
            var c0 = new Bz((4, 4));
            var d0 = new Bz((3 - x, 3), Math.PI / 2);

            var r1 = 0.5 * (a0.a - c0.a).Len;
            var r2 = 0.5 * (b0.a - d0.a).Len;
            var center = 0.5 * (a0.a + c0.a);

            var a = a0.Join(b0, BzJoinType.PowerTwoLikeEllipse);
            var b = a.Join(c0, BzJoinType.PowerTwoLikeEllipse);
            var c = b.Join(d0, BzJoinType.PowerTwoLikeEllipse);
            var d = c.Join(a, BzJoinType.PowerTwoLikeEllipse);

            Bz[] bzs = [a, b, c, d];

            var fn = bzs.ToFn();
            var ps = (500).SelectInterval(0, 1, v => fn(v));
            var lp = bzs.LinePoints();

            var f = Funcs2.Circle();
            var cps = (100).SelectInterval(2 * Math.PI, x => r1 * f(x).Scale((r2 / r1, 1)) + center);

            return new[]
            {
                cps.ToShape2().ToShape3().ToLines(Color.Red, 1),
                bzs.LinePoints().ToShape().ToPoints(Color.Green, 1.5),
                bzs.ControlPoints().ToShape().ToPoints(Color.Yellow, 1.8),
                ps.ToShape2().ToShape3().ToShapedSpots3(point, Color.Blue),
                coods
            }.ToSingleShape().Move(-size / 2, -size / 2, 0);
        }).ToMotion(new Vector3(0, 0, size * 1.1));
    }

    public Task<Motion> SaintPetersburgLamps()
    {
        var n = 15;
        var m = 15;
        var aFrom = Math.PI / 10;
        var aTo = Math.PI;

        var pC = Color.Red;
        var c1 = Color.Yellow;
        var c2 = Color.Yellow;

        var l = 100;
        var motionCount = 500;
        var lTh = 1;
        var pTh = 0.6;
        var maxCurve = (m - 1) * Math.PI / (n * (aTo - aFrom));
        var point = Shapes.IcosahedronSp1.Perfecto(0.05 * pTh);

        return (motionCount).SelectInterval(maxCurve,
                curve => (n).SelectClosedInterval(2 * Math.PI, a =>
                {
                    var fL = Funcs3.SphereSpiral(curve, a - aFrom * curve);
                    var fR = Funcs3.SphereSpiral(-curve, a + aFrom * curve);
                    var mCurve = (m - 1) * curve / maxCurve;
                    var pointN = (int)mCurve + 1;
                    var aPointTo = aFrom + (aTo - aFrom) * ((int)mCurve) / mCurve;

                    return new[]
                    {
                        (pointN).SelectInterval(aFrom, aPointTo, t =>point.Move(fL(t))).ToSingleShape().ApplyColor(pC),

                        (l).SelectInterval(aFrom, aTo, t => fL(t)).ToShape(false).ToLines(lTh, c1),
                        (l).SelectInterval(aFrom, aTo, t => fR(t)).ToShape(false).ToLines(lTh, c2),
                    }.ToSingleShape();
                }).ToSingleShape()).ToMotion();
    }

    public Task<Motion> ParquetMotion()
    {
        var model0 = vectorizer.GetText("parquet", 300, "Times New Roman").Perfecto(3).ScaleZ(0.2).SplitPlanes(0.05); // Shapes.PlaneTorus(10, 10, 3).Perfecto(2)/*.SplitPlanes(0.1)*/;
        var modelRotateAxis = new Vector3(1, 3, -2).Normalize();
        var stepCount = 800;
        var useLine = false;
        var showModel = false;
        var lineSplitNum = 6;

        var planeN = 48;
        var planeSize = 3.0;
        var itemSize = planeSize / (planeN - 1);
        var plane = Parquets.PentagonalKershner8(planeN / 4, planeN / 4, 1.7, false).ToShape3().Perfecto(planeSize).RotateOz(Math.PI/4);
        //var plane = Parquets.Hexagons(planeN, planeN, 1, false).ToShape3().Perfecto(planeSize);
        //var plane = Shapes.Plane(planeN, planeN, Convexes.Squares).SplitByConvexes(false).ToSingleShape().Perfecto(planeSize);
        var ps = plane.Points3;
        var ccs = plane.Planes.Select(p => p.Center()).ToArray();
        var fns = plane.Convexes.Index().Select(DistanceFn).ToArray();

        Vector3[] modelPoints = GetShapePoints(model0);
        var net = new NetV3(modelPoints.Length, i => modelPoints[i].SetZ(0), 0.6 * itemSize, true);

        Vector3[] GetShapePoints(Shape s) => useLine
            ? s.Lines3.SelectMany(l => (lineSplitNum).SelectInterval(x => l.a + x * l.ab)).ToArray()
            : s.Planes.Select(v => v.Center()).Concat(s.Points3).ToArray();

        Func<Vector3, double> DistanceFn(int i)
        {
            var prFn = plane.ProjectionFn(i);
            var insFn = plane.IsInsideConvexFn(i);
            var disFn = plane.ConvexDistanceFn(i);

            return p => insFn(prFn(p)) ? Math.Max(0, disFn(p)) : 0;
        };

        Shape GetShape(double v)
        {
            var dynPlane = plane.Copy();
            var model = model0.Rotate(2 * Math.PI * v, modelRotateAxis);

            modelPoints = GetShapePoints(model);
            net.Update();

            dynPlane.Convexes.ForEach((c, iC) =>
            {
                var iMps = net.SelectNeighborVs(ccs[iC]).ToArray();

                if (iMps.Length > 0)
                {
                    var z = iMps.Select(iMp => modelPoints[iMp]).Max(p => fns[iC](p));
                    c.ForEach(i => dynPlane.Points[i] = ps[i].SetZ(z).ToV4());
                }                
            });
            
            var s = new[]
            {
                dynPlane.FilterConvexPlanes((ps, _)=> !ps.Any(p=>p.z>0)).Normalize().ToLines(1, Color.Blue),
                //dynPlane.FilterConvexPlanes((ps, _)=> !ps.Any(position=>position.z>0)).Normalize().AddPerimeterZVolume(-itemSize).ApplyColor(Color.Black),
                dynPlane.FilterConvexPlanes((ps, _)=> ps.Any(p=>p.z>0)).AddNormalVolume(-itemSize).ApplyColor(Color.Red),
                showModel ? model.ToLines(5, Color.Yellow) : Shape.Empty
            }.ToSingleShape();

            return s;
        }

        return (stepCount).SelectInterval(v => GetShape(v)).ToMotion(5);
    }

    public Task<Motion> TrySkeleton()
    {
        var s = Shapes.Stone(4, 21).Perfecto();
        //var ball = Shapes.Cube.Scale(60, 10, 40).GroupMembers(2).SplitPlanes(0.3).GroupMembers();
        //var ball = Shapes.IcosahedronSp2.GroupMembers().ScaleX(0.4);
        //var ball = Surfaces.Torus(31, 10, 3, true).GroupMembers();
        //var ball = Surfaces.Shamrock(120, 10, true).GroupMembers();
        //var ball = Shapes.Cube.SplitPlanes(0.3).GroupMembers();

        return s.AddSkeleton().ToMetaShape3(0.3, 0.3, Color.Red, Color.Blue).ToMotion(1);
    }

    public Task<Motion> VariatorServiceMotion()
    {
        var c1 = Color.DarkOrange;
        var c2 = Color.Black;
        var txt = vectorizer.GetTextLine("все будет заведись", "Gogol", multY:1.1).Centered().Mult(3);
        var n = 200;

        IEnumerable<Shape> Animate()
        {
            for (var i = 0; i < n; i++)
                yield return new[]
                {
                    vectorizer.GetTextLine("#").Perfecto(0.2).Rotate(2*Math.PI * i/n).Rotate(1,1,1).Move(-0.6, 0.6, 0).ApplyColor(c1),
                    vectorizer.GetContentShape("vs").Perfecto().AlignY(0).ApplyColorGradientY(c1, c1, c2),
                    new Shape[]
                    {
                        new Shape[]
                        {
                            txt.MoveX(Math.PI*-1/3-2*Math.PI * i/n).PullOnSurface(SurfaceFuncs.CylinderABYm(0.5,1)),
                            txt.MoveX(Math.PI*1/3-2*Math.PI * i/n).PullOnSurface(SurfaceFuncs.CylinderABYm(0.5,1)),
                            txt.MoveX(Math.PI*3/3-2*Math.PI * i/n).PullOnSurface(SurfaceFuncs.CylinderABYm(0.5,1))
                        }.ToSingleShape().AlignY(0.5).MoveY(0.02).ApplyColorGradientY(c2, c1),
                        Shapes.CylinderR(50, 0.01, 1).ScaleX(0.5).ToOy().AlignY(1).ApplyColor(c1)
                    }.ToSingleShape().Rotate(2,1,0, Vector3.YAxis),
                }.ToSingleShape();
        }

        return Animate().ToMotion(2);
    }

    public Task<Motion> CubeGalaxiesIntersection()
    {
        // see result: https://www.youtube.com/watch?iMp=l9XWBsMDY9w&ab_channel=%D0%90%D0%BB%D0%B5%D0%BA%D1%81%D0%B5%D0%B9%D0%A8%D0%BC%D1%8B%D0%BA%D0%BE%D0%B2

        int n = 300;

        double? netSize = 0.5;
        double gravityPower = 0.0000008;
        double pSize = 0.1;
        double cubeStretch = 5;
        double sceneSize = 10;

        int netSide = 6;

        var particleShape = Shapes.Cube.Perfecto(pSize);

        var k = 0.2;

        var data = new (Shape s, Vector3 shift, Func<Shape, Vector3> speed, Func<Shape, Shape> modifyFn, Color color)[]
        {
                (Shapes.Cube.SplitPlanes(0.1).ScaleY(cubeStretch),
                    new Vector3(-2.5, 0, 0),
                    s => k * 0.5 * s.PointCenter.MultV(Vector3.YAxis),
                    s=>s,
                    Color.Black),

                (Shapes.Cube.SplitPlanes(0.1).ScaleY(cubeStretch).Rotate(1, 1, 1),
                    new Vector3(2.5, 0, 0),
                    s => k*0.5 * s.PointCenter.MultV(Vector3.YAxis.Rotate(1, 1, 1)),
                    s=>s.Rotate(1,1,1),
                    Color.Black),
        };

        var particles = data
            .SelectMany(s => s.s.SplitByConvexes()
                .Select(ss => new Particle()
                {
                    Position = ss.PointCenter + s.shift,
                    Speed = s.speed(ss),
                    Color = s.color,
                    ModifyFn = s.modifyFn
                }))
            .Select((p, i) =>
            {
                p.i = i;
                return p;
            }).ToArray();

        var animator = new Animator(new AnimatorOptions()
        {
            UseParticleGravityAttraction = true,
            GravityAttractionPower = gravityPower,
            NetSize = netSize,
            NetFrom = new Vector3(-netSide, -netSide, -netSide),
            NetTo = new Vector3(netSide, netSide, netSide)
        });

        animator.AddItems(particles);

        IEnumerable<Shape> Animate() => (n).SelectRange(_ =>
        {
            animator.Animate(1);

            return particles.Where(p => p.Position.Length < sceneSize)
                .Select(p => p.ModifyFn(particleShape).Move(p.Position).ApplyColor(p.Color))
                .ToSingleShape()
                .ApplyColorSphereGradient(Color.White, Color.Black, Color.Black);
        });

        return Animate().ToMotion(10);
    }

    class Particle : IAnimatorParticleItem
    {
        public int i;
        public Vector3 Position { get; set; }
        public Vector3 Speed { get; set; }
        public Color Color;
        public Func<Shape, Shape> ModifyFn;
    }
}