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
        public double? ZVolume { get; set; } = 1;
        public double? ToLinesSize = null;
        public TriangulationStrategy TriangulationStrategy { get; set; } = TriangulationStrategy.Ears;
    }

    public class ShapeOptions : SolidOptions
    {
        public bool UseLineDirection = false;
        public double? SpliteLineLevelsDistance = null;
        public double? SpliteAllPolygonsDistance = null;
        public double? ToSpotNumSize = null;
        public (Color odd, Color even) LineColors = (Color.Red, Color.DarkGreen);
        public (Color odd, Color even) NumColors = (Color.Blue, Color.BlueViolet);
        public int SmoothOutLevel { get; set; } = 2;
        public double SmoothAngleScalar { get; set; } = -0.1;
        public bool ComposePolygons { get; set; } = true;
        public Action<Shape, int> modifyPolygonShapeFn = null;
        public bool DebugProcess { get; set; } = false;

        public ShapeOptions With(Action<ShapeOptions> modify)
        {
            var clone = this.Clone();
            modify(clone);

            return clone;
        }
    }
}