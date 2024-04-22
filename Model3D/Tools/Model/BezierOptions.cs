using System;
using System.Collections.Generic;
using Vector2 = Model.Vector2;

namespace Model3D.Tools.Model;

public class BezierOptions : PolygonOptions
{
    /// <summary>
    /// Cannot take another point on this distance
    /// </summary>
    public int MinPointDistance { get; set; } = 5;

    /// <summary>
    /// Cannot skip points on this distance
    /// </summary>
    public int MaxPointDistance { get; set; } = 20;

    /// <summary>
    /// Check angle using this number or points for each angle side
    /// </summary>
    public int AnglePointDistance { get; set; } = 5;

    /// <summary>
    /// Point goes to angle with less angle only
    /// </summary>
    public double AllowedAngle { get; set; } = 0.75 * Math.PI;
    
    /// <summary>
    /// Point goes to angle with less factor only
    /// </summary>
    public int AllowedAngleFactor { get; set; } = 75;

    /// <summary>
    /// 0 - skip all allowed points (off), 90 - take all angle close points, 100 - take all allowed points (max power)
    /// </summary>
    public int PointIgnoreFactor { get; set; } = 90;

    /// <summary>
    /// Smoothing all points for result Bezier points
    /// </summary>
    public int SmoothingResultLevel { get; set; } = 3;

    /// <summary>
    /// Smoothing all points for angle determining only
    /// </summary>
    public int SmoothingAlgoLevel { get; set; } = 3;

    /// <summary>
    /// Compensate point move after smoothing
    /// </summary>
    public double SmoothingMoveCompensationLength { get; set; } = 1;

    /// <summary>
    /// Move smoothing point back to base positions by group of
    /// </summary>
    public int SmoothingMoveCompensationLevel { get; set; } = 3;

    /// <summary>
    /// Multiply number of Bezier points when optimizing control points
    /// </summary>
    public int OptimizationAccuracy { get; set; } = 3;

    /// <summary>
    /// Optimization accuracy
    /// </summary>
    public double OptimizationEpsilon { get; set; } = 0.01;

    /// <summary>
    /// Allow continue when incorrect optimization
    /// </summary>
    public int OptimizationMaxCount { get; set; } = 1000;

    public BezierOptions()
    {
        PolygonPointStrategy = PolygonPointStrategy.Center;
        PolygonOptimizationLevel = 0;
    }


    public bool DebugBezier { get; set; }
    public bool DebugProcess { get; set; }
    public bool DebugFillPoints { get; set; }

    public List<Vector2[]> cps = new();
    public List<Vector2[]> lps = new();
    public List<Vector2[]> ps = new();
    public List<Vector2[]> aps = new();
}