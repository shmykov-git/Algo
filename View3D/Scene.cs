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

        public Shape GetShape()
        {
            var sceneSize = new Vector3(16, 16, 16);

            var options = new WaterCubeOptions()
            {
                SceneSize = sceneSize,
                SkipAnimations = 400,
                SceneSteps = (1, 1),
                StepAnimations = 200,
                WaterEnabled = true,
                WaterSpeed = 0.066,
                WaterDir = new Vector3(0.2, 1, 0.2),
                WaterPosition = new Vector3(-2, -sceneSize.y/2 + 8, -2),
                ParticlePerEmissionCount = 10,
                LiquidPower = 0.0002,
                FrictionFactor = 0.8,
                ParticleCount = 10000,
                ParticlePlaneBackwardThikness = 2
            };

            var rnd = new Random(options.Seed);
            



            return WaterSystemPlatform.Cube(
                new WaterCubeModel()
                {
                    RunCalculations = false,
                    DebugColliders = false,
                    DebugCollidersNoVisible = false,
                    DebugCollidersSkipCube = true,
                    DebugCollidersSkipShift = true,
                    DebugCollidersAsLines = true,
                    DebugCollidersAsLinesThikness = 2,
                    DebugNetPlanes = false,

                    PlaneModels = new List<WaterCubePlaneModel>()
                    {
                        

                        //new() {VisibleShape = Shapes.CoodsWithText.Mult(5), DebugColliderSkip = true},
                    }
                }, options);
        }
    }
}
