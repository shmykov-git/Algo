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

namespace ViewMotion;

/// <summary>
/// You can find some video results on this youtube channel https://www.youtube.com/channel/UCSXMjRAXWmRL4rV7wy06eOA
/// </summary>
partial class SceneMotion
{
    public Task<Motion> WorldMotion()
    {
        // list of active shapes
        var actives = new ActiveShape[]
            {
                Shapes.Cube.Scale(60, 10, 40).Perfecto(2).SplitPlanes(0.3).AlignY(0).MoveY(1).ApplyColor(Color.LightBlue)
                .ToActiveShape(o =>
                {
                    o.RotationSpeedAxis = Vector3.YAxis;
                    o.RotationSpeedAngle = 0.0005;
                    o.UseSkeleton = true;
                    o.SkeletonPower = 0.02;
                    o.UseBlow = true;
                    o.BlowPower = 2;
                    
                    o.OnStep += a =>
                    {                        
                        // Add any modification to animate
                        // o.BlowPower += 0.001;
                    };

                    // Add any predefined animation
                    o.OnStep += ActiveShapeAnimations.BlowUp(0.001);

                    o.OnShow += s =>
                    {
                        // change any shape options on show

                        return s; // s.ApplyColor(Color.LightBlue);
                    };
                })
            };

        // list of static shapes
        var statics = new Shape[]
            {
                vectorizer.GetText("Wind", 200).Perfecto(7).AlignY(0).MoveZ(-4).ApplyColor(Color.SaddleBrown)
            };

        return (actives, statics).ToWorld(o =>
        {
            o.PressurePowerMult = 0.0001;
            o.ClingForce = 0.1;
            o.FrictionForce = 0.03;
            o.WindPower = 2; // try wind carefully
            o.Ground.UseWaves = true;
            o.Ground.WavesSize = 2;
            
            o.OnStep += w =>
            {
                // Add any world animation
            };
        }).ToMotion(10);

    }

    #region Bullet
    public class BulletMotionExample2
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
            var bulletBorderCoef = -0.4;      // depends on bullet.radius
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

            //var aim = (5, 5, 5).SelectRange((i, j, k) => Shapes.NativeCubeWithCenterPoint.Move(i, j, k)).ToSingleShape().NormalizeWith2D().Centered().MoveZ(50);
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

    public class BulletMotionExample
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

    public Task<Motion> SliderMotion()
    {
        // see result here: https://www.youtube.com/watch?v=RkE_z8ilk8g&ab_channel=%D0%90%D0%BB%D0%B5%D0%BA%D1%81%D0%B5%D0%B9%D0%A8%D0%BC%D1%8B%D0%BA%D0%BE%D0%B2

        var sceneColor = Color.FromArgb(50, 60, 70);

        var options = new WaterCubeOptions()
        {
            SceneSize = new Vector3(16, 16, 16),
            StepAnimations = 5,
            SceneMotionSteps = 500,
            WaterSpeed = 0.07,
            FrictionFactor = 0.6,
            ParticlePerEmissionCount = 2,
            ParticleCount = 10000,
            ParticlePlaneBackwardThikness = 2,
            PlatformColor = sceneColor
        };

        var rnd = new Random(options.Seed);
        var cubeSize = options.SceneSize;

        var ground = Surfaces.Plane(9, 9).FilterConvexes(c => c[0].IsEven()).Perfecto().Scale(cubeSize).ToOy().AddNormalVolume(0.25).AlignY(0).MoveY(-cubeSize.y / 2).ApplyColor(sceneColor);
        var logicGround = ground
            .FilterPlanes(p => p.NOne.MultS(-Vector3.YAxis) < 0.999)
            .FilterConvexPlanes((convex, _) =>
            {
                var c = convex.Center();

                if (c.x < -0.99 * cubeSize.x / 2 || c.x > 0.99 * cubeSize.x / 2)
                    return false;

                if (c.z < -0.99 * cubeSize.z / 2 || c.z > 0.99 * cubeSize.z / 2)
                    return false;

                return true;
            });

        Shape TransformSpiral(Shape s) => s.Normalize().Scale(1, 1, 1 / Math.PI).Perfecto(11)
            .CurveZ(Funcs3.Spiral(1.25)).Mult(4).ToOyM().RotateOy(Math.PI / 10)
            .Move(0, -cubeSize.y / 2 + 9.5, -2);

        var spiral = TransformSpiral(Surfaces.ChessCylinder(15, 123)).AddNormalVolume(0.15).ApplyColor(sceneColor);
        var logicSpiral = TransformSpiral(Surfaces.Cylinder(15, 123)).ReversePlanes();

        var logicBox = Surfaces.Plane(23, 23).Perfecto().Transform(Multiplications.Cube)
            .FilterConvexes(c => c[0].IsEven()).Where(v => v.y < -0.3)
            .Mult(10).AlignY(0).MoveY(-cubeSize.y / 2 + 1).ReversePlanes();

        var box = logicBox.AddNormalVolume(-0.15).Normalize().ApplyColor(sceneColor);

        var waterDir = new Vector3(-0.6, -0.15, 1);
        var tap = Shapes.Cube.Perfecto(0.9).AlignZ(0).Scale(1, 1, 2.5).Rotate(waterDir, Vector3.YAxis)
            .Move(2, -cubeSize.y / 2 + 14, 2.5).ApplyColor(sceneColor);

        Item[] GetStepItems(int n) => (n).SelectRange(_ => new Item
        {
            Position = rnd.NextCenteredV3(0.3) + new Vector3(2, -cubeSize.y / 2 + 14, 2.5),
            Speed = -options.WaterSpeed * waterDir
        }).ToArray();

        return WaterSystemPlatform.CubeMotion(
            new WaterCubeModel()
            {
                PlaneModels = new List<WaterCubePlaneModel>()
                {
                    new() {VisibleShape = spiral, ColliderShape = logicSpiral, ColliderShift = options.ParticleRadius},
                    new() {VisibleShape = box, ColliderShape = logicBox},
                    new() {VisibleShape = tap},
                    new() {VisibleShape = ground, ColliderShape = logicGround},
                },
                GetStepItemsFn = GetStepItems
            }, options).ToMotion(35);
    }

    public Task<Motion> CubeGalaxiesIntersection()
    {
        // see result: https://www.youtube.com/watch?v=l9XWBsMDY9w&ab_channel=%D0%90%D0%BB%D0%B5%D0%BA%D1%81%D0%B5%D0%B9%D0%A8%D0%BC%D1%8B%D0%BA%D0%BE%D0%B2

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

    public Task<Motion> Waterfall2()
    {
        var options = new WaterCubeOptions()
        {
            SceneSize = new Vector3(12, 15, 12),
            ParticleInitCount = 1000,
            SceneMotionSteps = 150,
            StepAnimations = 10,
            PlatformColor = Color.FromArgb(60, 100, 140),
            PlatformType = PlatformType.Square
        };

        var rnd = new Random(options.Seed);

        var cubeSize = options.SceneSize;
        var particleRadius = options.ParticleRadius;

        (Shape sphere, Shape collider) GetHalfSphere(double radius, Vector3 move, double removeLine = 0.5001, Vector3? rotate = null, bool up = true)
        {
            var sphere = Surfaces.SphereAngle(30, 60, 2 * Math.PI, 0).Mult(0.5).ToOy()
                .ModifyIf(up, s => s.Where(v => v.y > -removeLine), s => s.Where(v => v.y < removeLine).ReversePlanes().AddNormalVolume(-0.2 / radius))
                .Mult(radius)
                .ModifyIf(rotate.HasValue, s => s.RotateY(rotate.Value))
                .Move(move)
                .ApplyColor(options.PlatformColor);

            var collider = Surfaces.SphereAngle(10, 20, 2 * Math.PI, 0).Mult(0.5).ToOy()
                .ModifyIf(up, s => s.Where(v => v.y > -removeLine), s => s.Where(v => v.y < removeLine).ReversePlanes())
                .Mult(radius)
                .ModifyIf(rotate.HasValue, s => s.RotateY(rotate.Value))
                .Move(move);

            return (sphere, collider);
        }

        Shape GetGutter(Vector3 scale, Vector3 rotation, Vector3 move, double gutterCurvature = 0.4)
        {
            var gutterTmp = Surfaces.Plane(20, 2).Perfecto().FlipY().Scale(scale).AddPerimeterVolume(.6);
            gutterTmp = gutterCurvature.Abs() < 0.001
                ? gutterTmp.MoveZ(-2.5)
                : gutterTmp.MoveZ(-2 / gutterCurvature).ApplyZ(Funcs3Z.CylinderXMR(4 / gutterCurvature))
                    .MoveZ(6 / gutterCurvature - 2.5);
            var gutter = gutterTmp.Centered().Rotate(rotation).Move(move).ApplyColor(options.PlatformColor);

            return gutter;
        }

        var gutters = new[]
        {
            GetGutter(new Vector3(4, 80, 1), new Vector3(0.1, 6, 1), new Vector3(0, cubeSize.y / 2 - 3, -2)),
            GetGutter(new Vector3(4, 40, 1), new Vector3(-0.1, 6, -1), new Vector3(0, cubeSize.y / 2 - 10, 3))
        };

        var spheres = new[]
        {
            GetHalfSphere(2.5, new Vector3(0, -cubeSize.y / 2 + 2.5, 0)),
            GetHalfSphere(4, new Vector3(-2, -cubeSize.y / 2 + 2.5, 0), -0.1, new Vector3(1, 1.5, 0), false)
        };

        var models = new List<WaterCubePlaneModel>
        {
            new WaterCubePlaneModel { VisibleShape = Shapes.CoodsWithText.Mult(5) }
        };

        models.AddRange(gutters.Select(g => new WaterCubePlaneModel() { VisibleShape = g, ColliderShape = g, ColliderShift = -particleRadius }));
        models.AddRange(spheres.Select(s => new WaterCubePlaneModel() { VisibleShape = s.sphere, ColliderShape = s.collider, ColliderShift = -particleRadius }));

        Item[] GetInitItems(int n) => (n).SelectRange(_ => new Item
        {
            Position = rnd.NextCenteredV3(1.5) + new Vector3(0, cubeSize.y / 2 - 1, -3) + options.WaterPosition
        }).ToArray();

        return WaterSystemPlatform.CubeMotion(
            new WaterCubeModel()
            {
                PlaneModels = models,
                GetInitItemsFn = GetInitItems,
                //DebugColliders = true,
                //DebugCollidersAsLines = true,
            }, options).ToMotion(25);
    }

    public Task<Motion> Aqueduct() =>
        WaterSystem.AqueductMotion(new WaterCubeOptions()
        {
            SceneSize = new Vector3(16, 16, 16),
            ParticleInitCount = 5000,
            SceneMotionSteps = 200,
            StepAnimations = 10,
            GravityPower = 0.001,
            LiquidPower = 0.0002,
            FrictionFactor = 0.8,
            ParticleCount = 10000,
            ParticlePlaneBackwardThikness = 1,
            PlatformType = PlatformType.Circle,
            PlatformColor = Color.FromArgb(64, 0, 0),
        }).ToMotion(cameraDistance: 30);

    public Task<Motion> Waterfall() =>
        WaterSystem.WaterfallMotion(new WaterfallOptions()
        {
            SceneSize = new Vector3(12, 15, 12),
            GutterCurvature = 0,
            GutterRotation = new Vector3(0.05, 6, 1),
            ParticleInitCount = 500,
            SceneMotionSteps = 100,
            StepAnimations = 10,
            PlatformColor = Color.FromArgb(64, 0, 0),
            SphereColor = Color.FromArgb(64, 0, 0),
            GutterColor = Color.FromArgb(64, 0, 0),
        }).ToMotion(cameraDistance:25);

    public Task<Motion> IllBeBack()
    {
        var s0 = vectorizer.GetText("I'll be back", 300).Perfecto(10).ApplyColor(Color.Blue);

        var frames = WaterSystem.IllBeBackMotion(new WaterCubeOptions()
        {
            SceneSize = new Vector3(16, 16, 16),
            WaterEnabled = true,
            WaterPosition = new Vector3(0, -1.5, 0),
            WaterDir = new Vector3(-0.06, 1, 0.06),
            WaterSpeed = 0.16,

            SceneMotionSteps = 300,
            ParticlePerEmissionCount = 2,
            EmissionAnimations = 1,
            StepAnimations = 10,
        });

        return frames.ToMotion(null, s0);
    }

    public Task<Motion> Fountain()
    {
        var s0 = vectorizer.GetText("Fountain").Perfecto(10).ApplyColor(Color.Blue);

        var frames = WaterSystem.FountainMotion(new FountainOptions()
        {
            SceneSize = new Vector3(12, 18, 12),
            ParticleCount = 10000,
            ParticlePerEmissionCount = 2,
            EmissionAnimations = 1,
            ParticleSpeed = new Vector3(0.002, 0.12, 0.004),
            WaterPosition = new Vector3(0, 0.3, 0),
            Gravity = new Vector3(0, -1, 0),
            GravityPower = 0.001,
            LiquidPower = 0.0001,
            SkipAnimations = 0,
            StepAnimations = 10,
            SceneMotionSteps = 1000,
            JustAddShamrock = false,
            PlatformColor = Color.DarkGreen,
            FountainColor = Color.Gray,
        });

        return frames.ToMotion(null, s0);
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