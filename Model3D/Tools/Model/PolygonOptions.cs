namespace Model3D.Tools.Model
{
    public class PolygonOptions
    {
        public int ColorLevel = 200;
        public int PolygonOptimizationLevel { get; set; } = 3;
        public int MinimumPolygonPointsCount { get; set; } = 0;
        public LevelStrategy LevelStrategy { get; set; } = LevelStrategy.All;
        public bool NormalizeScale = true;
        public bool NormalizeAlign = true;
    }
}