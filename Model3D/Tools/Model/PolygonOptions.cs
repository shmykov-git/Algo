using System;
using System.Text.Json.Serialization;
using Model.Libraries;
using Model3D.Libraries;

namespace Model3D.Tools.Model
{
    public enum PolygonPointStrategy
    {
        Center,
        Circle,
        Square
    }

    public class BitmapOptions
    {
        public int ColorLevel { get; set; } = 200;
        public bool InvertColor { get; set; } = false;
        [JsonIgnore]public Func<double, double, Func<double, double, bool>> ColorMask { get; set; }
    }

    public class PolygonOptions: BitmapOptions
    {
        public int PolygonOptimizationLevel { get; set; } = 3;
        public int MinimumPolygonPointsCount { get; set; } = 0;
        public LevelStrategy PolygonLevelStrategy { get; set; } = LevelStrategy.All;

        public PolygonPointStrategy PolygonPointStrategy { get; set; } = PolygonPointStrategy.Circle;
        public double PolygonPointRadius { get; set; } = 0.01;

        public bool NormalizeScale = true;
        public bool NormalizeAlign = true;

        public bool DebugBitmap = false;
        public bool DebugPerimeters = false;
    }
}