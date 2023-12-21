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
        public int ColorLevel { get; set; } = 200;      // 0 - white, 200 - middle, 255 - black
        public bool InvertColor { get; set; } = false;  // invert black and white color
        [JsonIgnore]public Func<double, double, Func<double, double, bool>> ColorMask { get; set; } // function to set white color to outside points
    }

    public class PolygonOptions: BitmapOptions
    {
        public int PolygonOptimizationLevel { get; set; } = 3;      // line center point skip. 0 - off, 1 - 3 points, 2 - 3 & 5 points, 3 - 3 & 5 & 7 points
        public int MinimumPolygonPointsCount { get; set; } = 0;     // skip polygons with equal or less points
        public LevelStrategy PolygonLevelStrategy { get; set; } = LevelStrategy.All;    // what kind of polygon levels should be taken

        public PolygonPointStrategy PolygonPointStrategy { get; set; } = PolygonPointStrategy.Circle; // how to get points from single bitmap point
        public double PolygonPointRadius { get; set; } = 0.01;      // radius of single point for some polygon strategies

        public bool NormalizeScale = true;
        public bool NormalizeAlign = true;

        public bool DebugBitmap = false;        // show bitmap as chars
        public bool DebugPerimeters = false;    // show perimeter information
    }
}