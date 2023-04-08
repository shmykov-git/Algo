using System;
using Aspose.ThreeD.Utilities;

namespace ViewMotion.Models;

class CameraMotionOptions
{
    public Func<int, Vector3> PositionFn { get; set; }
    public Func<int, Vector3> LookDirectionFn { get; set; }
    public Func<int, Vector3> UpDirectionFn { get; set; }
}