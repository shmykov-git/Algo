using Microsoft.VisualBasic.CompilerServices;

namespace Model3D.Tools.Model
{
    public class ShapeOptions : PolygonOptions
    {
        public double? ZVolume { get; set; } = 1;
        public double? ToLinesSize = null;
        public int SmoothOutLevel { get; set; } = 2;
        public double SmoothAngleScalar { get; set; } = -1;
        public TriangulationStrategy TriangulationStrategy { get; set; } = TriangulationStrategy.Trio;
        public double TriangulationFixFactor { get; set; } = 0;
        public bool ComposePolygons { get; set; } = true;
    }
}