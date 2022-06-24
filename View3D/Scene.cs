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
            return IllBeBack();

            var options = new WaterCubeOptions()
            {
                SceneSize = new Vector3(16, 32, 16),
                SkipAnimations = 600,
                SceneSteps = (1, 1),
                StepAnimations = 200,
                WaterEnabled = true,
                WaterSpeed = 0.2,
                WaterDir = new Vector3(0.04, 1.5, 0.04),
                WaterPosition = new Vector3(0, -15.5, 0),
                ParticlePerEmissionCount = 2,
                FrictionFactor = 0.6,
                ParticleCount = 10000,
                ParticlePlaneBackwardThikness = 2
            };

            var rnd = new Random(options.Seed);
            var sceneSize = options.SceneSize;



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
                        //new() {VisibleShape = shamrock, /*ColliderShape = logicShamrock, ColliderShift = options.ParticleRadius*/},

                        //new() {VisibleShape = Shapes.CoodsWithText.Mult(5), DebugColliderSkip = true},
                    }
                }, options);
        }
    }
}
