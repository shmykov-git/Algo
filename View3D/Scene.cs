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
            var models = new List<WaterCubePlaneModel>();

            var s = Surfaces.MobiusStrip(120, 20).SplitByConvexes().ToSingleShape().Normalize().Rotate(1, 1, 1).Mult(3).ApplyColor(Color.Black);
            models.Add(new WaterCubePlaneModel() {VisibleShape = s, SkipLogic = true});

            return WaterSystem.CubePlatform(
                new WaterCubeModel()
                {
                    PlaneModels = models,
                },
                new WaterCubeOptions()
                {
                    SceneSize = new Vector3(12, 18, 12),
                    ParticleCount = 2000,
                    ParticlePerEmissionCount = 2,
                    EmissionAnimations = 1,
                    ParticleSpeed = new Vector3(0.002, 0.12, 0.004),
                    Gravity = new Vector3(0, -1, 0),
                    GravityPower = 0.001,
                    LiquidPower = 0.0001,
                    SkipAnimations = 100,
                    StepAnimations = 200,
                    SceneSteps = (1, 1)
                });

            var shape = new Shape[]
            {

                Shapes.CoodsWithText, Shapes.CoodsNet
            }.ToSingleShape();

            return shape;
        }
    }
}
