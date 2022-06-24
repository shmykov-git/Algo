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

        public class Options : WaterCubeOptions
        {
            public double WaterSpeed = 0.07;
        }


        public Shape GetShape()
        {
            var options = new Options()
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
            var sceneSize = options.SceneSize;


            var shamrock = Surfaces.Shamrock(480, 40, true).Perfecto(20).ToOy().MoveY(-sceneSize.y / 2 + 2)
                .Where(v=>(v-new Vector3(-3, -5, 3)).Length >= 6.5).ApplyColor(Color.Black).WithBackPlanes(Color.DarkRed);
            //var logicShamrock = Surfaces.Shamrock(96, 8).Perfecto(20).ToOy().MoveY(-sceneSize.y / 2 + 2);

            var ball = Shapes.Ball.Perfecto(9).Move(-3, -5, 3).ToLines(2).ApplyColor(Color.Red);

            var text = vectorizer.GetText("I'll be back", 300, "Royal Inferno").Perfecto(10)
                .ToOxM().Move(-3,0,0)
                .Where(v => (v - new Vector3(-3, -5, 3)).Length >= 4.8).ToLines(3).ApplyColor(Color.Red);

            var level1 = Surfaces.CircleAngle(40, 10, 0, Math.PI / 2)
                .Perfecto(8).AddPerimeterVolume(0.6).MoveZ(-2).ApplyZ(Funcs3Z.SphereMR(10)).MoveZ(12).ToOy()
                .MoveY(-sceneSize.y / 2 + 0.5);

            var level2 = Surfaces.CircleAngle(40, 10, 0, Math.PI / 2)
                .Perfecto(5).AddPerimeterVolume(0.6).MoveZ(-1.3).ApplyZ(Funcs3Z.SphereMR(7)).MoveZ(8.3).ToOy()
                .MoveY(-sceneSize.y / 2 + 3.5);

            var level3 = Surfaces.CircleAngle(20, 10, 0, Math.PI / 2)
                .Perfecto(3).AddPerimeterVolume(0.6).MoveZ(-1).ApplyZ(Funcs3Z.SphereMR(4)).MoveZ(5).ToOy()
                .MoveY(-sceneSize.y / 2 + 5.5);

            var fountain = (level1 + level2 + level3)
                .Where(v => (v - new Vector3(-3, -5, 3)).Length >= 4.8)
                .ApplyColor(Color.Black).WithBackPlanes(Color.DarkRed);

            //WaterSystemPlatform.Item[] GetStepItems(int n) => (n).SelectRange(_ => new WaterSystemPlatform.Item
            //{
            //    Position = rnd.NextCenteredV3(0.3) + new Vector3(2, -cubeSize.y / 2 + 14, 2.5),
            //    Speed = -options.WaterSpeed * waterDir
            //}).ToArray();


            // todo: направить фонтан (коллайдеры)

            return WaterSystemPlatform.Cube(
                new WaterCubeModel()
                {
                    RunCalculations = false,
                    DebugColliders = true,
                    DebugCollidersLogicOnly = false,
                    DebugCollidersSkipCube = true,
                    DebugCollidersSkipShift = true,
                    DebugCollidersAsLines = true,
                    DebugCollidersAsLinesThikness = 2,
                    DebugNetPlanes = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        new() {VisibleShape = shamrock, /*ColliderShape = logicShamrock, ColliderShift = options.ParticleRadius*/},
                        new() {VisibleShape = ball},
                        new() {VisibleShape = text},
                        new() {VisibleShape = fountain},

                        //new() {VisibleShape = Shapes.CoodsWithText.Mult(5), SkipCollider = true},
                    },
                    //GetStepItemsFn = GetStepItems
                }, options);

            var shape = new Shape[]
            {

                Shapes.CoodsWithText, Shapes.CoodsNet
            }.ToSingleShape();

            return shape;
        }
    }
}
