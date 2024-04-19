using System;
using System.Collections.Generic;
using Vector2 = Model.Vector2;

namespace Model3D.Tools.Model
{
    public class BezierOptions : PolygonOptions
    {
        public int MinPointDistance { get; set; } = 5;
        public int MaxPointDistance { get; set; } = 16;
        public int AnglePointDistance { get; set; } = 4;
        public double AllowedAngle { get; set; } = 0.75 * Math.PI;
        public double AngleSigma2 { get; set; } = 0.1;
        public int SmoothingResultLevel { get; set; } = 3;
        public int SmoothingAlgoLevel { get; set; } = 3;
        public int OptimizationAccuracy { get; set; } = 3;
        public double OptimizationEpsilon { get; set; } = 0.01;
        public int OptimizationMaxCount { get; set; } = 1000;

        public BezierOptions()
        {
            PolygonPointStrategy = PolygonPointStrategy.Center;
            PolygonOptimizationLevel = 0;
        }

        public bool DebugProcess { get; set; }
        public bool DebugBreak { get; set; }

        public List<Vector2[]> cps = new();
        public List<Vector2[]> lps = new();
        public List<Vector2[]> ps = new();
        public List<Vector2[]> aps = new();
    }
}