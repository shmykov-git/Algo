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
            var contentName = "debug1";

            var options = new ShapeOptions()
            {
                //ZVolume = null,
                //ToLinesSize = 0.3,
                //TriangulationStrategy = TriangulationStrategy.None,
                //ComposePolygons = false,

                ZVolume = 0.02,
                TriangulationStrategy = TriangulationStrategy.Sort,

                ColorLevel = 200,
                LevelStrategy = LevelStrategy.All,
                SmoothOutLevel = 0,
                SmoothAngleScalar = 0.71,
                PolygonOptimizationLevel = 0,
                MinimumPolygonPointsCount = 3,

                DebugPerimeterLength = true,
                DebugBitmap = true,
                DebugProcess = true,
            };

            var lineOptions = new ShapeOptions()
            {
                ZVolume = null,
                ToLinesSize = 0.3,
                SpliteLineLevelsDistance = 0.02,
                SpliteLineColors = (Color.Red, Color.Green),
                TriangulationStrategy = TriangulationStrategy.None,
                ComposePolygons = false,

                ColorLevel = options.ColorLevel,
                LevelStrategy = options.LevelStrategy,
                SmoothOutLevel = options.SmoothOutLevel,
                SmoothAngleScalar = options.SmoothAngleScalar,
                PolygonOptimizationLevel = options.PolygonOptimizationLevel,
                MinimumPolygonPointsCount = options.MinimumPolygonPointsCount,
            };

            var readyShape = vectorizer.GetContentShape(contentName, options);
            var lineShape = vectorizer.GetContentShape(contentName, lineOptions);

            return new[]
                {
                    readyShape.ApplyColor(Color.Black),
                    lineShape.MoveZ(0.03),
                    //Shapes.CoodsWithText.ApplyColor(Color.Black),
                }.ToSingleShape();
        }
    }
}
