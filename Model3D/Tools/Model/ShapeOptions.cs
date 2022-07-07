using System;
using System.Drawing;
using System.Text.Json;
using Microsoft.VisualBasic.CompilerServices;

namespace Model3D.Tools.Model
{
    public class SolidOptions : PolygonOptions
    {
        public double? ZVolume { get; set; } = 1;
        public TriangulationStrategy TriangulationStrategy { get; set; } = TriangulationStrategy.Trio;
        public double TriangulationFixFactor { get; set; } = 0;
    }

    public class ShapeOptions : SolidOptions
    {
        public double? ToLinesSize = null;
        public double? SpliteLineLevelsDistance = null;
        public double? SpliteAllPolygonsDistance = null;
        public double? ToSpotNumSize = null;
        public (Color odd, Color even) LineColors = (Color.Red, Color.Blue);
        public (Color odd, Color even) NumColors = (Color.DarkGreen, Color.DarkGreen);
        public int SmoothOutLevel { get; set; } = 2;
        public double SmoothAngleScalar { get; set; } = -1;
        public bool ComposePolygons { get; set; } = true;
        public bool DebugProcess { get; set; } = false;

        public ShapeOptions With(Action<ShapeOptions> modify)
        {
            var clone = JsonSerializer.Deserialize<ShapeOptions>(JsonSerializer.Serialize(this));
            modify(clone);

            return clone;
        }
    }
}