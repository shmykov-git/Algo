using System.Collections.Generic;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
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

namespace ViewMotion;
/// <summary>
/// You can find some video results on this youtube channel https://www.youtube.com/channel/UCSXMjRAXWmRL4rV7wy06eOA
/// </summary>
partial class SceneMotion
{

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
        bps = bzs.Select(b => b.points).ToArray();
        var fn = bzs.ToBz(true);
        var ps = (1000).SelectInterval(1, x => fn(x), true);

        var circle = Funcs2.Circle();
        var cps = (1000).SelectInterval(2 * Math.PI, x => 0.498 * circle(x) + (0.5, 0.5), true);

        return new[]
        {
            cps.ToShape2().ToShape3().ToLines(0.3, Color.Red),
            bps.Select(aa=>aa[0]).ToArray().ToShape().ToPoints(Color.Green, 1.5),
            bps.SelectMany(aa=>aa.Skip(1)).ToArray().ToShape().ToPoints(Color.Yellow, 1.5),
            ps.ToShape2().ToShape3().ToLines(0.3, Color.Blue),
            Shapes.Coods2WithText
        }.ToSingleShape().Move(-0.5, -0.5, 0).ToMotion(1.5);
    }

    public Task<Motion> BezierCircle()
    {
        var alfa = Math.PI / 2;
        var L = 0.5 * 4 / 3 * Math.Tan(alfa / 4);

        Vector2[][] bps =
        [
            [(0, 0.5), (0, 0.5 + L), (0.75, 0.25), (0.5 - L, 1)],
            [(0.5, 1), (0.5 + L, 1), (1.125, 1.125), (1, 0.5 + L)],
            [(1, 0.5), (1, 0.5 - L), (0.5 + L, 0)],
            [(0.5, 0), (0.5 - L, 0), (0, 0.5 - L)],
        ];

        var fn = bps.ToBz(true);

        var ps = (1000).SelectInterval(1, x => fn(x), true);

        var circle = Funcs2.Circle();
        var cps = (1000).SelectInterval(2 * Math.PI, x => 0.498 * circle(x) + (0.5, 0.5), true);

        return new[]
        {
            cps.ToShape2().ToShape3().ToLines(0.3, Color.Red),
            bps.Select(aa=>aa[0]).ToArray().ToShape().ToPoints(Color.Green, 1.5),
            bps.SelectMany(aa=>aa.Skip(1)).ToArray().ToShape().ToPoints(Color.Yellow, 1.5),
            ps.ToShape2().ToShape3().ToLines(0.3, Color.Blue),
            Shapes.Coods2WithText
        }.ToSingleShape().Move(-0.5, -0.5, 0).ToMotion(1.5);
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

    public Task<Motion> WallMotion()
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

    public Task<Motion> BulletCatchMotion() => new BulletCatchMotionExample().Scene();
    public Task<Motion> BulletThrowMotion() => new BulletThrowMotionExample().Scene();

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

    #region Bullet
    public class BulletThrowMotionExample // todo: ActiveWorld
    {
        class Node : INet3Item
        {
            public int i;
            public List<int> ns;
            public Vector3 position;
            public Vector3 speed = Vector3.Origin;
            public double mass = 1;
            public bool locked;
            public Func<Vector3> PositionFn => () => position;
        }

        class PointObject
        {
            public Vector3 position;
            public Vector3 speed;
            public double mass;
            public double radius;
        }

        public Task<Motion> Scene()
        {
            var rnd = new Random();

            var sceneCount = 1000;
            var n = 50;
            var activeRadius = 5;
            var brokenRadius = 50;
            var k = 0.05;
            var aCoef = k * 1;
            var gCoef = k * 1;
            var bulletForceCoef = k * 0.005;  // fix bullet interactive force
            var bulletBorderCoef = -0.4;      // depends on bullet.powerRadius
            var dampingCoef = 0.8;
            var forceBorder = 0.5;
            var gravityCoef = 0.0001;
            var gravity = new Vector3(0, -1, 0);
            var rotate = -0.2;

            var fixZPos = 14;
            var bullet = new PointObject
            {
                position = new Vector3(0, 0, 1.5),
                speed = new Vector3(0, 0, 0),
                mass = 30,
                radius = 1,
            };

            //var aim = (5, 5, 5).SelectRange((iMp, j, k) => Shapes.NativeCubeWithCenterPoint.Move(iMp, j, k)).ToSingleShape().NormalizeWith2D().Centered().MoveZ(50);
            var block = (n, n, 1).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered()
                .Mult(0.05)
                .PullOnSurface(SurfaceFuncs.Paraboloid)
                .Mult(20)
                .Where(v => v.z < fixZPos + 1.5)
            //+ aim
            ;

            var ps = block.Points3;

            var nodes = block.PointIndices.Select(i => new Node()
            {
                i = i,
                position = ps[i]
            }).ToArray();
            nodes.ForEach(n => n.ns = block.Links[n.i].ToList());
            nodes.ForEach(n => n.locked = fixZPos < n.position.z && n.position.z < 40);

            block = block.RotateOx(rotate);
            ps = block.Points3;
            nodes.ForEach(n => n.position = ps[n.i]);

            block = block.Normalize();
            var net = new Net3<Node>(nodes, activeRadius);

            Func<double, double> blockForceFn = d =>
            {
                if (d < forceBorder)
                    d = forceBorder;

                return -aCoef / d.Pow4() + gCoef / d.Pow2();
            };

            Func<double, double> bulletForceFn = d =>
            {
                var a = bulletBorderCoef - bullet.radius;

                if (d + a < forceBorder)
                    d = forceBorder - a;

                return -bulletForceCoef / (d + a).Pow4();
            };

            Vector3 CalcSpeed(Vector3 p0, Vector3 s0, IEnumerable<Vector3> ps)
            {
                Vector3 offset = Vector3.Origin;

                foreach (var p in ps)
                {
                    var d = (p - p0).Length;
                    var ds = blockForceFn(d);

                    offset += ds * (p - p0) / d;
                }

                return s0 + offset * dampingCoef;
            }

            void Step()
            {
                net.SelectItemsByRadius(bullet.position, activeRadius)
                    .ForEach(n =>
                    {
                        var d = (n.position - bullet.position).Length;
                        var ds = 0.5 * (bullet.mass + n.mass) * bulletForceFn(d);
                        n.speed -= ds * (n.position - bullet.position) / (d * n.mass);
                        bullet.speed += ds * (n.position - bullet.position) / (d * bullet.mass);
                    });
                bullet.speed += gravityCoef * gravity;

                nodes.Where(n => !n.locked).ForEach(n => n.speed = CalcSpeed(n.position, n.speed, n.ns.Select(j => nodes[j].position)));
                nodes.Where(n => !n.locked).ForEach(n => n.speed += gravityCoef * gravity);

                nodes.Where(n => !n.locked).ForEach(n => n.position += n.speed);
                bullet.position += bullet.speed;
            }

            Shape GetBlock(int i) => new Shape
            {
                Points3 = nodes.Select(n => n.position).ToArray(),
                Convexes = block.Convexes
            };

            var border = Surfaces.Torus(60, 10, 11).Perfecto(37).MoveZ(fixZPos + 0.5).RotateOx(rotate).ApplyColor(Color.SaddleBrown);
            var bulletShape = Shapes.IcosahedronSp3.Perfecto(2 * bullet.radius).ApplyColor(Color.Red);

            IEnumerable<Shape> Animate()
            {
                for (var i = 0; i < sceneCount; i++)
                {
                    yield return new[]
                    {
                    GetBlock(i).ApplyColor(Color.Blue),//.ToMetaShape3(5, 5, Color.Blue, Color.Green),
                    bulletShape.Move(bullet.position),
                    border,
                }.ToSingleShape();

                    Step();
                }
            }

            return Animate().ToMotion(n * 2);
        }
    }

    public class BulletCatchMotionExample
    {
        class Node : INet3Item
        {
            public int i;
            public List<int> ns;
            public Vector3 position;
            public Vector3 speed = Vector3.Origin;
            public Func<Vector3> PositionFn => () => position;
        }

        class PointObject
        {
            public Vector3 position;
            public Vector3 speed;
            public double mass;
            public double radius;
        }

        public Task<Motion> Scene()
        {
            var rnd = new Random();

            var n = 7;
            var bulletRadius = 2;
            var brokenRadius = 3;
            var k = 0.01;
            var aCoef = k * 1;
            var gCoef = k * 1;
            var dampingCoef = 0.8;
            var forceBorder = 0.7;

            var bullet = new PointObject
            {
                position = new Vector3(-9, 0, 0),
                speed = new Vector3(0.5, 0, 0),
                mass = 10 * 4,
                radius = 0.5
            };

            var block = (n, n, n).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered();
            var ps = block.Points3;

            var nodes = block.PointIndices.Select(i => new Node()
            {
                i = i,
                position = ps[i]
            }).ToArray();
            nodes.ForEach(n => n.ns = block.Links[n.i].ToList());

            var net = new Net3<Node>(nodes, bulletRadius);

            Func<double, double> blockForceFn = d =>
            {
                if (d < forceBorder)
                    d = forceBorder;

                return -aCoef / d.Pow4() + gCoef / d.Pow2();
            };

            Func<double, double> bulletForceFn = d =>
            {
                var a = -bullet.radius;

                if (d + a < forceBorder)
                    d = forceBorder - a;

                return -k / (d + a).Pow4();
            };

            Vector3 CalcSpeed(Vector3 p0, Vector3 s0, IEnumerable<Vector3> ps)
            {
                Vector3 offset = Vector3.Origin;

                foreach (var p in ps)
                {
                    var d = (p - p0).Length;
                    var ds = blockForceFn(d);

                    offset += ds * (p - p0) / d;
                }

                return s0 + offset * dampingCoef;
            }

            bool IsBroken(Vector3 a, Vector3 b) => (b - a).Length2 > brokenRadius * brokenRadius;

            void Step()
            {
                net.SelectItemsByRadius(bullet.position, bulletRadius)
                    .ForEach(n =>
                    {
                        var d = (n.position - bullet.position).Length;
                        var ds = 0.5 * (bullet.mass + 1) * bulletForceFn(d);
                        n.speed -= ds * (n.position - bullet.position) / d;
                        bullet.speed += ds * (n.position - bullet.position) / (d * bullet.mass);
                    });

                nodes.ForEach(n => n.speed = CalcSpeed(n.position, n.speed, n.ns.Select(j => nodes[j].position)));

                nodes.ForEach(n => n.position += n.speed);
                bullet.position += bullet.speed;

                nodes.ForEach(n => n.ns.ToArray().ForEach(j =>
                {
                    if (IsBroken(n.position, nodes[j].position))
                    {
                        n.ns.Remove(j);
                    }
                }));
            }

            Shape GetBlock(int i) => new Shape
            {
                Points3 = nodes.Select(n => n.position).ToArray(),
                Convexes = nodes.SelectMany(n => n.ns.Select(j => (n.i, j).OrderedEdge())).Distinct().Select(v => v.EdgeToArray()).ToArray(),
            };

            IEnumerable<Shape> Animate()
            {
                for (var i = 0; i < 1000; i++)
                {
                    yield return new[]
                    {
                    GetBlock(i).ToMetaShape3(5, 5, Color.Blue, Color.Green),
                    Shapes.IcosahedronSp3.Perfecto(bullet.radius).Move(bullet.position).ApplyColor(Color.Red),
                    Surfaces.Plane(2*n, 2*n).Centered().ToOx().ToLines(3, Color.White)
                }.ToSingleShape();

                    Step();
                }
            }

            return Animate().ToMotion(n * 2);
        }
    }
    #endregion
}