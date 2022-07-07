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
            var contentName = "m13";

            // заливка

            var options = new ShapeOptions()
            {
                ZVolume = 0.02,                         // null - no volume, number - add volume to the shape
                //ToLinesSize = 0.3,                      // .ToLines(number)
                TriangulationStrategy = TriangulationStrategy.Sort,         // triangulation strategy
                PolygonPointStrategy = PolygonPointStrategy.Circle,         // how to get points from single bitmap point
                PolygonPointRadius = 0.01,              // radius of single point for some polygon strategies
                //TriangulationFixFactor = 0.0001,

                ColorLevel = 200,                       // 0 - white, 200 - middle, 255 - black
                LevelStrategy = LevelStrategy.All,      // what kind of levels should be taken
                SmoothOutLevel = 0,                     // number of 3 point smooth out process run
                SmoothAngleScalar = 0.5,                // (on) -1:180%, -0.5:150%, 0:90%, 0.5:30%, 1:0% (off) on or off smoothing on 3 point condition
                PolygonOptimizationLevel = 0,           // 0 - off, 1 - 3 line point center skip, 2 - 3, 5 line point skip, 3 = 3, 5, 7 line point skip
                MinimumPolygonPointsCount = 0,          // skip polygons with equal or less points

                //DebugPerimeters = true,                 // show perimeter information
                //DebugBitmap = true,                     // show bitmap as chars
                DebugProcess = true,                    // show process operations time
            };

            var lineOptions = new ShapeOptions()
            {
                ZVolume = null,
                ToLinesSize = 0.2,
                //ToSpotNumSize = 0.15,
                SpliteLineLevelsDistance = 0.00,                      // move even and odd polygons to the distance
                SpliteAllPolygonsDistance = 0.000,                      // move all polygons to the distance
                ComposePolygons = false,                                 // run polygon composition process
                TriangulationStrategy = TriangulationStrategy.None,

                ColorLevel = options.ColorLevel,
                LevelStrategy = options.LevelStrategy,
                SmoothOutLevel = options.SmoothOutLevel,
                SmoothAngleScalar = options.SmoothAngleScalar,
                PolygonOptimizationLevel = options.PolygonOptimizationLevel,
                PolygonPointStrategy = options.PolygonPointStrategy,
                MinimumPolygonPointsCount = options.MinimumPolygonPointsCount,
                PolygonPointRadius = options.PolygonPointRadius,

                //DebugBitmap = true,
                DebugPerimeters = true,                                 
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
                    //vectorizer.GetContentShape(contentName, options.With(o=>o.PolygonPointStrategy = PolygonPointStrategy.Circle)).MoveZ(-0.3).ApplyColor(Color.Black),
                    ////vectorizer.GetContentShape(contentName, lineOptions).MoveZ(0.03),
                    //vectorizer.GetContentShape(contentName, centerOptions).MoveZ(0.05),
                    //Shapes.CoodsWithText.ApplyColor(Color.Black),
                }.ToSingleShape();
        }
    }
}
