using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D;
using Model3D.Extensions;
using Model3D.Libraries;
using Model3D.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Mapster.Utils;
using MathNet.Numerics;
using Model.Fourier;
using Model.Graphs;
using Model.Tools;
using Model3D.Systems;
using Model3D.Systems.Model;
using View3D.Libraries;
using Shape = Model.Shape;
using Triangulator = Model.Tools.Triangulator;
using Vector2 = Model.Vector2;

namespace View3D
{
    partial class Scene
    {
        #region ctor

        private readonly Settings settings;
        private readonly Vectorizer vectorizer;

        public Scene(Settings settings, Vectorizer vectorizer)
        {
            this.settings = settings;
            this.vectorizer = vectorizer;
        }    

        #endregion

        class SlideOptions : WaterCubeOptions
        {
            public double WaterSpeed = 0.07;
        }

        public Shape GetShape()
        {
            var options = new SlideOptions()
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
            //var particleRadius = options.ParticleRadius;


            //var mebius = Surfaces.MobiusStrip(120, 20, bothFaces:true).Perfecto(3).ToOy()
            //    .MoveY(-cubeSize.y / 2 + 3).ApplyColor(Color.Black);

            //var dini = Surfaces.DiniSurface(60, 30, bothFaces: true).Perfecto(5).ToOy().ScaleY(0.5).AlignY(0).MoveY(-cubeSize.y / 2)
            //    .Multiplicate(new Func<Shape,Shape>[]
            //    {
            //        s => s.Move(cubeSize.x / 2, 0, cubeSize.z / 2),
            //        s => s.Move(cubeSize.x / 2, 0, -cubeSize.z / 2),
            //        s => s.Move(-cubeSize.x / 2, 0, cubeSize.z / 2),
            //        s => s.Move(-cubeSize.x / 2, 0, -cubeSize.z / 2),
            //    }).ApplyColor(Color.Black);

            var ground = Surfaces.Plane(9, 9).FilterConvexes(c=>c[0].IsEven()).Perfecto().Scale(cubeSize).ToOy().AddNormalVolume(0.25).AlignY(0).MoveY(-cubeSize.y / 2).ApplyColor(Color.Black);
            var logicGround = ground
                .FilterPlanes(p => p.NOne.MultS(-Vector3.YAxis) < 0.999)
                .FilterConvexPlanes((convex,_) =>
                {
                    var c = convex.Center();

                    if (c.x < -0.99 * cubeSize.x / 2 || c.x > 0.99 * cubeSize.x / 2)
                        return false;

                    if (c.z < -0.99 * cubeSize.z / 2 || c.z > 0.99 * cubeSize.z / 2)
                        return false;

                    return true;
                });

            //return logicGround.ResizeByNormals(-options.ParticleRadius).ApplyColor(Color.Blue);

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

            //Shape TransformSphere(Shape s) => s.Where(v=>v.y < 0.2).Perfecto(5).FilterConvexes(c => (c[0] + c[2]) % 2 == 0)
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
                    DebugCollidersLogicOnly = false,
                    DebugCollidersSkipCube = false,
                    DebugCollidersSkipShift = false,
                    DebugCollidersAsLines = true,
                    DebugCollidersAsLinesThikness = 2,
                    DebugNetPlanes = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        //new() {VisibleShape = mebius, SkipCollider = true},
                        //new() {VisibleShape = dini, SkipCollider = true},
                        new() {VisibleShape = spiral, ColliderShape = logicSpiral, ColliderShift = options.ParticleRadius},
                        //new() {VisibleShape = sphere, ColliderShape = logicSphere},
                        new() {VisibleShape = box, ColliderShape = logicBox},
                        new() {VisibleShape = tap},
                        new() {VisibleShape = ground, ColliderShape = logicGround},

                        //new() {VisibleShape = Shapes.CoodsWithText.Mult(5), SkipCollider = true},
                    },
                    GetStepItemsFn = GetStepItems
                }, options).Rotate(1, 0, 1);

            var shape = new Shape[]
            {

                Shapes.CoodsWithText, Shapes.CoodsNet
            }.ToSingleShape();

            return shape;
        }
    }
}
