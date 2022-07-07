namespace Model3D.Tools.Model
{
    public enum PolygonPointStrategy
    {
        Center,
        Circle,
        //Square
    }

    public class PolygonOptions
    {
        public int ColorLevel = 200;
        public int PolygonOptimizationLevel { get; set; } = 3;
        public int MinimumPolygonPointsCount { get; set; } = 0;
        public LevelStrategy LevelStrategy { get; set; } = LevelStrategy.All;

        public PolygonPointStrategy PolygonPointStrategy { get; set; } = PolygonPointStrategy.Center;
        public double PolygonCircleRadius { get; set; } = 0.4999;

        public bool NormalizeScale = true;
        public bool NormalizeAlign = true;

        public bool DebugBitmap = false;
        public bool DebugPerimeters = false;
    }
}