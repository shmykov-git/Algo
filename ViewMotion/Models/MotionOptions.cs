using System;
using Model;

namespace ViewMotion.Models;

class MotionOptions
{
    public CameraMotionOptions CameraMotionOptions { get; set; }
    public double? CameraDistance { get; set; }
    public Shape StartShape { get; set; } 
    public TimeSpan? StepDelay { get; set; }
}