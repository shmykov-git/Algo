namespace Model3D.Tools.Model
{
    public class TextShapeOptions : ShapeOptions
    {
        public int FontSize { get; set; } = 50;
        public string FontName { get; set; } = "Arial";
        public double MultY { get; set; } = 1;
        public double MultX { get; set; } = 1;

        public TextShapeOptions()
        {
            TriangulationStrategy = TriangulationStrategy.Trio;
            SmoothOutLevel = 0;
            PolygonOptimizationLevel = 3;
            NormalizeAlign = false;
            NormalizeScale = true;
            ZVolume = 0.1;
        }
    }
}