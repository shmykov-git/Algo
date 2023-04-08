using System;
using Aspose.ThreeD.Utilities;

namespace ViewMotion.Models;

class CameraMotionOptions
{
    public Func<int, (Vector3 pos, Vector3 look, Vector3 up)> CameraFn { get; set; }
}