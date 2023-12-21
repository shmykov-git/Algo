using System;
using System.Drawing;
using System.Text.Json;
using Meta.Extensions;
using Microsoft.VisualBasic.CompilerServices;
using Model;

namespace Model3D.Tools.Model
{
    public class TriangulationOptions : PolygonOptions
    {
        public bool DebugTriangulation { get; set; }
        public int ProtectionTriangulationCount { get; set; } = 100000;
        public double TriangulationFixFactor { get; set; } = 0;
        public int? DebugTriangulationSteps { get; set; }
    }

    public class SolidOptions : TriangulationOptions
    {
        public double? ZVolume { get; set; } = 1;       // null - no volume, number - add volume to the shape
        public bool ZHardFaces { get; set; } = true;
        public double? ToLinesSize = null;              // .ToLines(number)
        public TriangulationStrategy TriangulationStrategy { get; set; } = TriangulationStrategy.Ears;  // triangulation strategy
    }

    public enum SkipSmoothOut
    {
        None,
        OuterSquare
    }

    public class ShapeOptions : SolidOptions
    {
        public bool UseLineDirection = false;                                           
        public double? SplitLineLevelsDistance = null;          // move even and odd polygons to the distance
        public double? SplitAllPolygonsDistance = null;         // move all polygons to the distance
        public double? ToSpotNumSize = null;
        public (Color odd, Color even) LineColors = (Color.Red, Color.DarkGreen);
        public (Color odd, Color even) NumColors = (Color.Blue, Color.BlueViolet);
        public int SmoothOutLevel { get; set; } = 2;            // number of 3 point smooth out process run
        public double SmoothAngleScalar { get; set; } = -0.1;   // (on) -1:180%, -0.5:150%, 0:90%, 0.5:30%, 1:0% (off) on or off smoothing on 3 point condition
        public int SmoothPointCount { get; set; } = 3;          // number of point for single smooth process
        public SkipSmoothOut SkipSmoothOut { get; set; } = SkipSmoothOut.None;
        public double SkipSmoothOutFactor { get; set; } = 0.99;
        public bool ComposePolygons { get; set; } = true;       // run polygon composition process
        public Action<Shape, int> modifyPolygonShapeFn = null;
        public bool DebugProcess { get; set; } = false;         // show process operations time

        public ShapeOptions With(Action<ShapeOptions> modify)
        {
            var clone = this.Clone();
            modify(clone);

            return clone;
        }
    }
}