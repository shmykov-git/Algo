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
using Aspose.ThreeD.Entities;

namespace View3D
{
    partial class Scene
    {
        #region ctor

        private readonly StaticSettings staticSettings;
        private readonly Vectorizer vectorizer;

        public Scene(StaticSettings staticSettings, Vectorizer vectorizer)
        {
            this.staticSettings = staticSettings;
            this.vectorizer = vectorizer;
        }

        #endregion

        public Shape GetShape()
        {
            var c1 = Color.DarkOrange; // Color.FromArgb(63, 27, 0);
            var c2 = Color.Black;
            var txt = vectorizer.GetTextLine("все будет заведись", "Gogol").Centered().Mult(3);

            return new[]
            {
                vectorizer.GetContentShape("vs").Perfecto().AlignY(0).ApplyColorGradientY(c1, c1, c2),
                new Shape[]
                {
                    new Shape[]
                    {
                        txt.PullOnSurface(SurfaceFuncs.CylinderYm),
                        txt.MoveX(Math.PI*2/3).PullOnSurface(SurfaceFuncs.CylinderYm),
                        txt.MoveX(Math.PI*4/3).PullOnSurface(SurfaceFuncs.CylinderYm)
                    }.ToSingleShape().AlignY(0.5).MoveY(0.02).ApplyColorGradientY(c2, c1),
                    Shapes.CylinderR(50, 0.01, 1).ToOy().AlignY(1).ApplyColor(c1)
                }.ToSingleShape().Rotate(2,1,0, Vector3.YAxis),
            }.ToSingleShape();

            //return Parquets.PentagonalKershner8(0.07, 1.5).ToShape3().GroupMembers().ToCubeMetaShape3(0.8, 0.8, Color.Blue, Color.Red);
            //return Parquets.PentagonalKershner8(0.05, 1.5).ToShape3().GroupMembers(3).ApplyZ(Funcs3Z.Atan).ToMetaShape3().Rotate(Rotates.Z_Y);

            var contentName = "vs";

            var options = new ShapeOptions()
            {
                ColorLevel = 150,                                       // 0 - white, 200 - middle, 255 - black
                //InvertColor = true,                                     // invert black and white color
                //ColorMask = ColorMasks.Ellipse4(1, 1),                        // function to set white color to outside points
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
                    vectorizer.GetContentShape(contentName, options).ApplyColor(Color.Blue),
                    //vectorizer.GetContentShape(contentName, lineOptions).MoveZ(0.015),
                }.ToSingleShape();
        }
    }
}
