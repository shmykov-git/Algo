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
using System.Threading;
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
            return Chess();

            var contentName = "chess3";

            var options = new ShapeOptions()
            {
                ColorLevel = 200,                                       // 0 - white, 200 - middle, 255 - black
                //InvertColor = true,                                     // invert black and white color
                //ColorMask = ColorMasks.Ellipse(1, 1),                        // function to set white color to outside points
                ZVolume = 0.02,                                         // null - no volume, number - add volume to the shape
                TriangulationStrategy = TriangulationStrategy.Ears,     // triangulation strategy
                PolygonPointStrategy = PolygonPointStrategy.Circle,     // how to get points from single bitmap point
                PolygonPointRadius = 0.01,                              // radius of single point for some polygon strategies

                PolygonLevelStrategy = LevelStrategy.All,   // what kind of polygon levels should be taken
                SmoothOutLevel = 2,                         // number of 3 point smooth out process run
                SmoothAngleScalar = 0.1,                   // (on) -1:180%, -0.5:150%, 0:90%, 0.5:30%, 1:0% (off) on or off smoothing on 3 point condition
                //SkipSmoothOut = SkipSmoothOut.OuterSquare,
                PolygonOptimizationLevel = 3,               // line center point skip. 0 - off, 1 - 3 points, 2 - 3 & 5 points, 3 - 3 & 5 & 7 points
                MinimumPolygonPointsCount = 0,              // skip polygons with equal or less points

                //DebugPerimeters = true,                   // show perimeter information
                //DebugBitmap = true,                       // show bitmap as chars
                DebugProcess = true,                        // show process operations time
            };

            var lineOptions = new ShapeOptions()
            {
                ZVolume = null,
                ToLinesSize = 0.05,                                     // .ToLines(number)
                UseLineDirection = true,
                //ToSpotNumSize = 0.05,
                //SpliteLineLevelsDistance = 0.00,                      // move even and odd polygons to the distance
                //SpliteAllPolygonsDistance = 0.01,                     // move all polygons to the distance
                ComposePolygons = true,                                 // run polygon composition process
                TriangulationStrategy = TriangulationStrategy.None,

                ColorLevel = options.ColorLevel,
                PolygonLevelStrategy = options.PolygonLevelStrategy,
                SmoothOutLevel = options.SmoothOutLevel,
                SmoothAngleScalar = options.SmoothAngleScalar,
                PolygonOptimizationLevel = options.PolygonOptimizationLevel,
                PolygonPointStrategy = options.PolygonPointStrategy,
                MinimumPolygonPointsCount = options.MinimumPolygonPointsCount,
                PolygonPointRadius = options.PolygonPointRadius,
            };

            return new[]
                {
                    vectorizer.GetContentShape(contentName, options),
                    //vectorizer.GetContentShape(contentName, lineOptions).MoveZ(0.015),
                }.ToSingleShape();
        }
    }
}
