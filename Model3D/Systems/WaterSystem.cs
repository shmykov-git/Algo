using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using Meta;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Systems.Model;
using Model3D.Tools.Model;
using View3D.Libraries;
using Vector2 = Model.Vector2;
using Item = Model3D.Systems.WaterSystemPlatform.Item;
using Model3D.Tools.Vectorization;

namespace Model3D.Systems
{
    public static class WaterSystem
    {
        public static Shape Aqueduct(WaterCubeOptions options = null) =>
            AqueductMotion(options).WaterCubeMotionToStatic(options);
        public static IEnumerable<Shape> AqueductMotion(WaterCubeOptions options = null)
        {
            // todo: cylinder collider

            options ??= new WaterCubeOptions()
            {
                SceneSize = new Vector3(16, 16, 16),
                ParticleInitCount = 5000,
                SkipAnimations = 150,
                SceneSteps = (1, 1),
                StepAnimations = 50,
                WaterEnabled = false,
                WaterSpeed = 0.066,
                WaterDir = new Vector3(0.2, 1, 0.2),
                WaterPosition = new Vector3(-2, -16 / 2 + 8, -2),
                ParticlePerEmissionCount = 10,
                GravityPower = 0.001,
                LiquidPower = 0.0002,
                FrictionFactor = 0.8,
                ParticleCount = 10000,
                ParticlePlaneBackwardThikness = 1,
                PlatformType = PlatformType.Circle,
            };

            var sceneSize = options.SceneSize;

            var rnd = new Random(options.Seed);

            Shape TorusModify(Shape s) => s.Perfecto(15).ToOy()
                .Where(v => v.y < 0.3)
                .Where(v =>
                {
                    var nAlfa = (int) (30 * Math.Atan2(v.x.Abs(), v.z.Abs()) / (Math.PI / 2));

                    if (nAlfa == 7 || nAlfa == 22)
                        return v.y > -0.75;

                    return true;
                });

            var ballColor = Color.OrangeRed;

            var torus = TorusModify(Surfaces.Torus(240, 40, 8)).AddNormalVolume(0.2)
                .ApplyColor(Color.Black)
                .ApplyColorSphereRGradient(10, new Vector3(0, -sceneSize.y / 2 + 3, 0), ballColor, ballColor, ballColor, ballColor, ballColor, ballColor, ballColor, null, null, null);
                
            var torusCollider = TorusModify(Surfaces.Torus(120, 20, 8)).AddNormalVolume(0.2)/*.ReversePlanes()*/;

            var stoneCount = 8;
            var stoneRadius = 6.5;
            var stoneAlfa0 = 0.37;
            var qs = new Quaternion[stoneCount];
            var stones = (stoneCount).SelectRange(i => Shapes.Stone(4, i, 1, 5, new Vector3(1.2, 3, 1.2))
                    .RotateToMassY(out qs[i]).Mult(4).AlignY(0).Move(stoneRadius * (i, stoneCount).ToCircleYV3(stoneAlfa0))).ToSingleShape()
                .MoveY(-sceneSize.y / 2)
                .ApplyColor(Color.Black)
                .ApplyColorSphereRGradient(10, new Vector3(0, -sceneSize.y / 2 + 3, 0), ballColor, ballColor, ballColor, ballColor, ballColor, null, null, null, null);
            var stoneColliders = (stoneCount).SelectRange(i => Shapes.Stone(4, i, 1, 1, new Vector3(1.2, 3, 1.2))
                    .Rotate(qs[i]).Mult(4).AlignY(0).Move(stoneRadius * (i, stoneCount).ToCircleYV3(stoneAlfa0))).ToSingleShape()
                .MoveY(-sceneSize.y / 2);

            var ball = Shapes.Ball.MoveY(-sceneSize.y / 2 + 3).ApplyColor(ballColor);

            Item[] GetInitItemsFn(int n) =>
                (n).SelectRange(_ =>
                {
                    var alfa = rnd.NextDouble() * 2 * Math.PI;
                    var r = 6.3 + rnd.NextDouble();
                    var y = rnd.NextDouble() - 0.3;

                    return new Item()
                    {
                        Position = new Vector3(r * Math.Cos(alfa), y, r * Math.Sin(alfa)),
                        Speed = new Vector3(0, 0, 0)
                    };
                }).ToArray();

            void ModifyParticle(Shape particle)
            {
                particle.ApplyColorSphereRGradient(10, new Vector3(0, -sceneSize.y / 2 + 3, 0), ballColor, ballColor, ballColor, ballColor, ballColor, null, null, null, null, null);
            }

            return WaterSystemPlatform.CubeMotion(
                new WaterCubeModel()
                {
                    //RunCalculations = false,
                    //DebugColliders = true,
                    //DebugCollidersAsLines = true,
                    //DebugCollidersNoVisible = false,
                    //DebugCollidersSkipCube = false,
                    //DebugCollidersSkipShift = true,
                    //DebugCollidersAsLinesThikness = 2,
                    //DebugNetPlanes = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        new(){VisibleShape = torus, ColliderShape = torusCollider},
                        new(){VisibleShape = stones, ColliderShape = stoneColliders},
                        new(){VisibleShape = ball},
                        //new() {VisibleShape = Shapes.CoodsWithText.Mult(5), DebugColliderSkip = true},
                    },
                    GetInitItemsFn = GetInitItemsFn,
                    ModifyParticleFn = ModifyParticle
                }, options);
        }

        public static Shape BigDee()
        {
            var sceneSize = new Vector3(14, 16, 14);

            var options = new WaterCubeOptions()
            {
                SceneSize = sceneSize,
                SkipAnimations = 400,
                SceneSteps = (1, 1),
                StepAnimations = 200,
                WaterEnabled = true,
                WaterSpeed = 0.066,
                WaterDir = new Vector3(0.2, 1, 0.2),
                WaterPosition = new Vector3(-2, -sceneSize.y / 2 + 8, -2),
                ParticlePerEmissionCount = 10,
                LiquidPower = 0.0002,
                FrictionFactor = 0.8,
                ParticleCount = 10000,
                ParticlePlaneBackwardThikness = 2,
                Seed = 1
            };

            var ballCollider = Shapes.IcosahedronSp3.Perfecto(5).Move(0, -sceneSize.y / 2 + 5, 0);
            var ball = Shapes.Ball.Perfecto(5).Move(0, -sceneSize.y / 2 + 5, 0).ApplyColorGradientY(Color.Red, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black);

            var b1 = Shapes.IcosahedronSp3.Perfecto(0.5).Move(-1.8 + 0.5, -sceneSize.y / 2 + 8 + 0.8, -1.8 - 0.5).ApplyColor(Color.Red);
            var b2 = Shapes.IcosahedronSp3.Perfecto(0.5).Move(-1.8 - 0.5, -sceneSize.y / 2 + 8 + 0.8, -1.8 + 0.5).ApplyColor(Color.Red);
            var bs = b1 + b2;

            var cylinder = Shapes.CylinderR(50, 2).Perfecto(6).ToOy().AlignY(0).Move(-3, -sceneSize.y / 2, 0).ApplyColor(Color.Red);
            var cylinderCollider = Shapes.CylinderR(10, 2).Perfecto(6).ToOy().AlignY(0).Move(-3, -sceneSize.y / 2, 0);

            var diniBase = Surfaces.DiniSurface(60, 30, bothFaces: true).Perfecto(5).ToOy().ScaleY(0.5).AlignY(0).MoveY(-sceneSize.y / 2);
            var dini = new[]
            {
                diniBase.ApplyColorGradientY(Color.Black, Color.Black, Color.Red).Move(-4, 0, -6),
                diniBase.ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Red).Move(4, 0, 3),
                diniBase.ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Red).Move(-4, 0, 6),
                diniBase.ApplyColorGradientY(Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Black, Color.Red).Move(5, 0, -3.5),
            }.ToSingleShape();

            return WaterSystemPlatform.Cube(
                new WaterCubeModel()
                {
                    RunCalculations = true,
                    DebugColliders = false,
                    DebugCollidersNoVisible = false,
                    DebugCollidersSkipCube = true,
                    DebugCollidersSkipShift = true,
                    DebugCollidersAsLines = true,
                    DebugCollidersAsLinesThikness = 2,
                    DebugNetPlanes = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        new() { VisibleShape = ball, ColliderShape = ballCollider, ColliderShift = options.ParticleRadius },
                        new() {VisibleShape = bs},
                        new() {VisibleShape = cylinder, ColliderShape = cylinderCollider},
                        new() {VisibleShape = dini},
                    }
                }, options).ToOx();
        }


        public static Shape IllBeBack(WaterCubeOptions options = null) =>
            IllBeBackMotion(options).WaterCubeMotionToStatic(options);

        public static IEnumerable<Shape> IllBeBackMotion(WaterCubeOptions options = null)
        {
            var shamrockColor = Color.FromArgb(0, 32, 0);
            var fountainColor = Color.SaddleBrown;
            var platformColor = Color.FromArgb(0, 16, 0);
            var ballColor = Color.Red;
            var darkColor = Color.DarkRed;

            var vectorizer = DI.Get<Vectorizer>();

            options ??= new WaterCubeOptions()
            {
                SceneSize = new Vector3(16, 16, 16),
                SkipAnimations = 1600,
                SceneSteps = (1, 1),
                WaterEnabled = true,
                WaterPosition = new Vector3(0, -1.5, 0),
                WaterDir = new Vector3(-0.06, 1, 0.06),
                WaterSpeed = 0.16
            };

            options.PlatformColor = platformColor;

            var sceneSize = options.SceneSize;

            var shamrock = Surfaces.Shamrock(480, 40, true).Perfecto(20).ToOy().MoveY(-sceneSize.y / 2 + 2)
                .Where(v => (v - new Vector3(-3, -5, 3)).Length >= 6.5).ApplyColor(shamrockColor).WithBackPlanes(darkColor);

            var shamrockCollider = Surfaces.Shamrock(96, 12).Perfecto(20).ToOy().MoveY(-sceneSize.y / 2 + 2)
                .Where(v => (v - new Vector3(-3, -5, 3)).Length >= 6.5);

            var ball = Shapes.Ball.Perfecto(9).Move(-3, -5, 3).ToLines(2).ApplyColor(ballColor);
            var ballCollider = Shapes.IcosahedronSp3.Perfecto(9).Move(-3, -5, 3).ResizeByNormals(0.3);

            var text = vectorizer.GetText("I'll be back", new TextShapeOptions()
                {
                    FontSize = 300, FontName = "Royal Inferno", SmoothAngleScalar = 0.71, SmoothOutLevel = 2
                }).Perfecto(10).ScaleZ(0.3)
                .ToOxM().Move(-3, 0, 0)
                .Where(v => (v - new Vector3(-3, -5, 3)).Length >= 4.8).ApplyColor(darkColor).WithBackPlanes(darkColor);

            var level1 = Surfaces.CircleAngle(40, 10, 0, Math.PI / 2).Normalize()
                .Perfecto(8).AddPerimeterZCenterVolume(0.6).MoveZ(-2).ApplyZ(Funcs3Z.SphereMR(10)).MoveZ(12).ToOy()
                .MoveY(-sceneSize.y / 2 + 0.5);

            var level2 = Surfaces.CircleAngle(40, 10, 0, Math.PI / 2).Normalize()
                .Perfecto(5).AddPerimeterZCenterVolume(0.6).MoveZ(-1.3).ApplyZ(Funcs3Z.SphereMR(7)).MoveZ(8.3).ToOy()
                .MoveY(-sceneSize.y / 2 + 3.5);

            var level3 = Surfaces.CircleAngle(20, 10, 0, Math.PI / 2).Normalize()
                .Perfecto(3).AddPerimeterZCenterVolume(0.6).MoveZ(-1).ApplyZ(Funcs3Z.SphereMR(4)).MoveZ(5).ToOy()
                .MoveY(-sceneSize.y / 2 + 5.5);

            var fountainCollider = (level1 + level2 + level3)
                .Where(v => (v - new Vector3(-3, -5, 3)).Length >= 4.8);

            var fountain = fountainCollider.ApplyColor(fountainColor).WithBackPlanes(darkColor);

            //bool AnnihilateWaterFromThisWorld(IAnimatorParticleItem x) =>
            //    (x.Position - new Vector3(-3, -5, 3)).Length >= 4.8;

            var frameShapes = WaterSystemPlatform.CubeMotion(
                new WaterCubeModel()
                {
                    //RunCalculations = true,
                    //DebugColliders = false,
                    //DebugCollidersAsLines = true,
                    //DebugCollidersSkipShift = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        new() {VisibleShape = shamrock, ColliderShape = shamrockCollider, ColliderShift = options.ParticleRadius },
                        new() {VisibleShape = ball, ColliderShape = ballCollider, ColliderShift = options.ParticleRadius},
                        new() {VisibleShape = text},
                        new() {VisibleShape = fountain, ColliderShape = fountainCollider, ColliderShift = options.ParticleRadius }
                    },
                    //GetParticleFilterFn = AnnihilateWaterFromThisWorld
                }, options);

            foreach (var shape in frameShapes)
            {
                yield return shape.ToOx();
            }
        }


        public static Shape Slide(WaterCubeOptions options = null)
        {
            options ??= new WaterCubeOptions()
            {
                SceneSize = new Vector3(16, 16, 16),
                SceneSteps = (1, 1),
                StepAnimations = 200,
                SkipAnimations = 1600,
                WaterSpeed = 0.07,
                FrictionFactor = 0.6,
                ParticlePerEmissionCount = 2,
                ParticleCount = 10000,
                ParticlePlaneBackwardThikness = 2
            };

            var rnd = new Random(options.Seed);
            var cubeSize = options.SceneSize;
            //var particleRadius = Options.ParticleRadius;


            //var mebius = Surfaces.MobiusStrip(120, 20, bothFaces:true).GroupMembers(3).ToOy()
            //    .MoveY(-cubeSize.y / 2 + 3).ApplyColor(Color.Black);

            //var dini = Surfaces.DiniSurface(60, 30, bothFaces: true).GroupMembers(5).ToOy().ScaleY(0.5).AlignY(0).MoveY(-cubeSize.y / 2)
            //    .Multiplicate(new Func<Shape,Shape>[]
            //    {
            //        s => s.Move(cubeSize.x / 2, 0, cubeSize.z / 2),
            //        s => s.Move(cubeSize.x / 2, 0, -cubeSize.z / 2),
            //        s => s.Move(-cubeSize.x / 2, 0, cubeSize.z / 2),
            //        s => s.Move(-cubeSize.x / 2, 0, -cubeSize.z / 2),
            //    }).ApplyColor(Color.Black);

            var ground = Surfaces.Plane(9, 9).FilterConvexes(c => c[0].IsEven()).Perfecto().Scale(cubeSize).ToOy().AddNormalVolume(0.25).AlignY(0).MoveY(-cubeSize.y / 2).ApplyColor(Color.Black);
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

            //return logicGround.ResizeByNormals(-Options.ParticleRadius).ApplyColor(Color.Blue);

            Shape TransformSpiral(Shape s) => s.Normalize().Scale(1, 1, 1 / Math.PI).Perfecto(11)
                .CurveZ(Funcs3.Spiral(1.25)).Mult(4).ToOyM().RotateOy(Math.PI / 10)
                .Move(0, -cubeSize.y / 2 + 9.5, -2);

            //var spikeValue = -particleRadius * 0.3;
            var spiral = TransformSpiral(Surfaces.ChessCylinder(15, 123)).AddNormalVolume(0.15).ApplyColor(Color.Black);
            var logicSpiral = TransformSpiral(Surfaces.Cylinder(15, 123)).ReversePlanes();

            var logicBox = Surfaces.Plane(23, 23).Perfecto().Transform(Multiplications.Cube)
                .FilterConvexes(c => c[0].IsEven()).Where(v => v.y < -0.3)
                .Mult(10).AlignY(0).MoveY(-cubeSize.y / 2 + 1).ReversePlanes();

            var box = logicBox.AddNormalVolume(-0.15).Normalize().ApplyColor(Color.Black);

            //Shape TransformSphere(Shape s) => s.Where(voxel=>voxel.y < 0.2).GroupMembers(5).FilterConvexes(c => (c[0] + c[2]) % 2 == 0)
            //    .Move(0, -cubeSize.y / 2 + 2.5, 3);

            //var ball = Shapes.GolfBall4;
            //var sphere = TransformSphere(ball).AddNormalVolume(0.1).ApplyColor(Color.Black);
            //var logicSphere = TransformSphere(ball).ReversePlanes();


            var waterDir = new Vector3(-0.6, -0.15, 1);
            var tap = Shapes.Cube.Perfecto(0.9).AlignZ(0).Scale(1, 1, 2.5).Rotate(waterDir, Vector3.YAxis)
                .Move(2, -cubeSize.y / 2 + 14, 2.5).ApplyColor(Color.Black);

            WaterSystemPlatform.Item[] GetStepItems(int n) => (n).SelectRange(_ => new WaterSystemPlatform.Item
            {
                Position = rnd.NextCenteredV3(0.3) + new Vector3(2, -cubeSize.y / 2 + 14, 2.5),
                Speed = -options.WaterSpeed * waterDir
            }).ToArray();

            return WaterSystemPlatform.Cube(
                new WaterCubeModel()
                {
                    RunCalculations = true,
                    DebugColliders = false,
                    DebugCollidersNoVisible = false,
                    DebugCollidersSkipCube = false,
                    DebugCollidersSkipShift = false,
                    DebugCollidersAsLines = true,
                    DebugCollidersAsLinesThikness = 2,
                    DebugNetPlanes = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        //new() {VisibleShape = mebius, DebugColliderSkip = true},
                        //new() {VisibleShape = dini, DebugColliderSkip = true},
                        new() {VisibleShape = spiral, ColliderShape = logicSpiral, ColliderShift = options.ParticleRadius},
                        //new() {VisibleShape = sphere, ColliderShape = logicSphere},
                        new() {VisibleShape = box, ColliderShape = logicBox},
                        new() {VisibleShape = tap},
                        new() {VisibleShape = ground, ColliderShape = logicGround},

                        //new() {VisibleShape = Shapes.CoodsWithText.Mult(5), DebugColliderSkip = true},
                    },
                    GetStepItemsFn = GetStepItems
                }, options);
        }


        public static Shape Fountain(FountainOptions options = null) =>
            FountainMotion(options).WaterCubeMotionToStatic(options);

        public static IEnumerable<Shape> FountainMotion(FountainOptions options = null)
        {
            var vectorizer = DI.Get<Vectorizer>();
            options ??= new FountainOptions();

            var rnd = new Random(options.Seed);
            var cubeSize = options.SceneSize;
            var particleRadius = options.ParticleRadius;

            var level1 = Surfaces.CircleAngleM(40, 10, 0, Math.PI / 2)
                .Perfecto(8).AddVolumeZ(0.6).MoveZ(-2).ApplyZ(Funcs3Z.SphereMR(10)).MoveZ(12).ToOy()
                .MoveY(-cubeSize.y / 2 + 0.5).ApplyColor(options.FountainColor);

            var level2 = Surfaces.CircleAngleM(40, 10, 0, Math.PI / 2)
                .Perfecto(5).AddVolumeZ(0.6).MoveZ(-1.3).ApplyZ(Funcs3Z.SphereMR(7)).MoveZ(8.3).ToOy()
                .MoveY(-cubeSize.y / 2 + 3.5).ApplyColor(options.FountainColor);

            var level3 = Surfaces.CircleAngleM(20, 10, 0, Math.PI / 2)
                .Perfecto(3).AddVolumeZ(0.6).MoveZ(-1).ApplyZ(Funcs3Z.SphereMR(4)).MoveZ(5).ToOy()
                .MoveY(-cubeSize.y / 2 + 5.5).ApplyColor(options.FountainColor);

            var models = new List<WaterCubePlaneModel>
            {
                new() {VisibleShape = level1, ColliderShape = level1, ColliderShift = options.ParticleRadius},
                new() {VisibleShape = level2, ColliderShape = level2, ColliderShift = options.ParticleRadius},
                new() {VisibleShape = level3, ColliderShape = level3, ColliderShift = options.ParticleRadius}
            };

            if (options.JustAddShamrock)
            {
                var shamrock = Surfaces.Shamrock(480, 40).Perfecto(20).ToOy().MoveY(-cubeSize.y / 2 + 2);
                var logicShamrock = Surfaces.Shamrock(96, 8).Perfecto(20).ToOy().MoveY(-cubeSize.y / 2 + 2)
                    .MovePlanes(-particleRadius);

                var fire = vectorizer.GetContentShapeObsolet("f1").Perfecto().ApplyZ(Funcs3Z.Waves).Mult(5)
                    .MoveY(-cubeSize.y / 2 + 3).ToLines(10);
                fire = fire.Rotate(1, 0, 1).Move(cubeSize.x / 2, 0, cubeSize.z / 2) +
                       fire.Rotate(-1, 0, 1).Move(-cubeSize.x / 2, 0, cubeSize.z / 2) +
                       fire.Rotate(1, 0, -1).Move(cubeSize.x / 2, 0, -cubeSize.z / 2) +
                       fire.Rotate(-1, 0, -1).Move(-cubeSize.x / 2, 0, -cubeSize.z / 2);

                models.Add(new WaterCubePlaneModel() { VisibleShape = shamrock, ColliderShape = logicShamrock, ColliderShift = options.ParticleRadius });
                models.Add(new WaterCubePlaneModel() { VisibleShape = fire });
            }

            Item[] GetStepItems(int n) => (n).SelectRange(_ => new Item
            {
                Position = rnd.NextCenteredV3(0.5) + new Vector3(0, -cubeSize.y / 2 + 6.5, 0) + options.WaterPosition,
                Speed = options.ParticleSpeed
            }).ToArray();

            return WaterSystemPlatform.CubeMotion(
                new WaterCubeModel()
                {
                    GetStepItemsFn = GetStepItems,
                    PlaneModels = models
                }, options);
        }


        public static Shape Waterfall(WaterfallOptions options = null) =>
            WaterfallMotion(options).WaterCubeMotionToStatic(options);

        public static IEnumerable<Shape> WaterfallMotion(WaterfallOptions options = null)
        {
            options ??= new WaterfallOptions();
            var rnd = new Random(options.Seed);

            var cubeSize = options.SceneSize;
            var particleRadius = options.ParticleRadius;

            var sphere = Shapes.Ball.Perfecto(options.SphereRadius).Where(v => v.y > -0.4).MoveY(-cubeSize.y / 2).MoveZ(4).Move(options.SphereOffset).ApplyColor(options.SphereColor);
            var logicSphere = Shapes.IcosahedronSp2.Perfecto().Perfecto(options.SphereRadius).Where(v => v.y > -0.1).MoveY(-cubeSize.y / 2).MoveZ(4).Move(options.SphereOffset).MovePlanes(-particleRadius);

            var gutterTmp = Surfaces.Plane(20, 2).Perfecto().FlipY().Scale(4, 50, 1).AddPerimeterZCenterVolume(.6);
            gutterTmp = options.GutterCurvature.Abs() < 0.001
                ? gutterTmp.MoveZ(-2.5)
                : gutterTmp.MoveZ(-2 / options.GutterCurvature).ApplyZ(Funcs3Z.CylinderXMR(4 / options.GutterCurvature))
                    .MoveZ(6 / options.GutterCurvature - 2.5);
            var gutter = gutterTmp.Rotate(options.GutterRotation, Vector3.YAxis).Move(0, cubeSize.y / 2 - 2, -2).Move(options.GutterOffset).ApplyColor(options.GutterColor);
            var logicGutter = gutter.AddBorder(-particleRadius);

            var models = new List<WaterCubePlaneModel>
            {
                new() {VisibleShape = sphere, ColliderShape = logicSphere},
                new() {VisibleShape = gutter, ColliderShape = logicGutter},
            };

            Item[] GetInitItems(int n) => (n).SelectRange(_ => new Item
            {
                Position = rnd.NextCenteredV3(1.5) + new Vector3(0, cubeSize.y / 2 - 1, -3) + options.WaterPosition +
                           options.WatterOffset
            }).ToArray();

            return WaterSystemPlatform.CubeMotion(
                new WaterCubeModel()
                {
                    PlaneModels = models,
                    GetInitItemsFn = GetInitItems
                }, options);
        }
    }
}
