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

namespace ViewMotion;
/// <summary>
/// You can find some video results on this youtube channel https://www.youtube.com/channel/UCSXMjRAXWmRL4rV7wy06eOA
/// </summary>
partial class SceneMotion // WaterSystem
{
    public Task<Motion> SliderMotion()
    {
        // see result here: https://www.youtube.com/watch?iMp=RkE_z8ilk8g&ab_channel=%D0%90%D0%BB%D0%B5%D0%BA%D1%81%D0%B5%D0%B9%D0%A8%D0%BC%D1%8B%D0%BA%D0%BE%D0%B2

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
        }).ToMotion(cameraDistance: 25);

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


}
