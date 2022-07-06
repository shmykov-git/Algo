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
using Model3D.Tools.Model;
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
            var contentName = "m12";

            // что делать с полигонами для m13?

            var options = new ShapeOptions()
            {
                ZVolume = 0.02,
                //ToLinesSize = 0.3,
                TriangulationStrategy = TriangulationStrategy.Trio,
                PolygonPointStrategy = PolygonPointStrategy.Circle,
                PolygonCircleRadius = 0.04,
                //TriangulationFixFactor = 0.0001,

                ColorLevel = 200,
                LevelStrategy = LevelStrategy.All,
                SmoothOutLevel = 2,
                SmoothAngleScalar = 0,
                PolygonOptimizationLevel = 3,
                MinimumPolygonPointsCount = 0,

                //DebugPerimeterLength = true,
                //DebugBitmap = true,
                DebugProcess = true,
            };

            var lineOptions = new ShapeOptions()
            {
                ZVolume = null,
                ToLinesSize = 0.3,
                //ToSpotNumSize = 0.15,
                SpliteLineLevelsDistance = 0.00,
                //SpliteAllPolygonsDistance = 0.01,
                ComposePolygons = true,
                TriangulationStrategy = TriangulationStrategy.None,

                ColorLevel = options.ColorLevel,
                LevelStrategy = options.LevelStrategy,
                SmoothOutLevel = options.SmoothOutLevel,
                SmoothAngleScalar = options.SmoothAngleScalar,
                PolygonOptimizationLevel = options.PolygonOptimizationLevel,
                PolygonPointStrategy = options.PolygonPointStrategy,
                MinimumPolygonPointsCount = options.MinimumPolygonPointsCount,

                DebugBitmap = true,
                DebugPerimeterLength = true,
            };

            //var centerOptions = new ShapeOptions()
            //{
            //    ZVolume = null,
            //    ToLinesSize = 0.3,
            //    //ToSpotNumSize = 0.15,
            //    SpliteLineLevelsDistance = 0.00,
            //    SpliteAllPolygonsDistance = 0.01,
            //    ComposePolygons = false,
            //    TriangulationStrategy = TriangulationStrategy.None,

            //    ColorLevel = options.ColorLevel,
            //    LevelStrategy = options.LevelStrategy,
            //    SmoothOutLevel = options.SmoothOutLevel,
            //    SmoothAngleScalar = options.SmoothAngleScalar,
            //    PolygonOptimizationLevel = options.PolygonOptimizationLevel,
            //    PolygonPointStrategy = PolygonPointStrategy.Center,
            //    MinimumPolygonPointsCount = options.MinimumPolygonPointsCount,
            //};

            return new[]
                {
                    vectorizer.GetContentShape(contentName, options).ApplyColor(Color.Black),
                    //vectorizer.GetContentShape(contentName, lineOptions).MoveZ(0.03),
                    //vectorizer.GetContentShape(contentName, centerOptions).MoveZ(0.05),
                    //Shapes.CoodsWithText.ApplyColor(Color.Black),
                }.ToSingleShape();
        }
    }
}
