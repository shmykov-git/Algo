namespace Model3D.Tools.Model
{
    public class TextShapeOptions : ShapeOptions
    {
        public int FontSize { get; set; } = 50;
        public string FontName { get; set; } = "Arial";
        public double MultY { get; set; }= 1;
        public double MultX { get; set; } = 1;
    }
}