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
        public double TriangulationFixFactor { get; set; } = 0;
        public int? DebugTriangulationSteps { get; set; }
    }

    public class SolidOptions : TriangulationOptions
    {
        public double? ZVolume { get; set; } = 1;
        public TriangulationStrategy TriangulationStrategy { get; set; } = TriangulationStrategy.Ears;
    }

    public class ShapeOptions : SolidOptions
    {
        public double? ToLinesSize = null;
        public bool UseLineDirection = false;
        public double? SpliteLineLevelsDistance = null;
        public double? SpliteAllPolygonsDistance = null;
        public double? ToSpotNumSize = null;
        public (Color odd, Color even) LineColors = (Color.Red, Color.Blue);
        public (Color odd, Color even) NumColors = (Color.DarkGreen, Color.DarkGreen);
        public int SmoothOutLevel { get; set; } = 2;
        public double SmoothAngleScalar { get; set; } = -1;
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