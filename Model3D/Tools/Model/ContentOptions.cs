namespace Model3D.Tools.Model
{
    public class ContentOptions
    {
        public int ColorLevel = 200;
        public double? ZVolume { get; set; } = 1;
        public int SmoothOutLevel { get; set; } = 2;
        public TriangulationStrategy TriangulationStrategy { get; set; } = TriangulationStrategy.Sort;
        public LevelStrategy LevelStrategy { get; set; } = LevelStrategy.All;
        public int PolygonOptimizationLevel { get; set; } = 3;
        public double TriangulationFixFactor { get; set; } = 0;
        public bool ComposePolygons { get; set; } = true;
    }
}