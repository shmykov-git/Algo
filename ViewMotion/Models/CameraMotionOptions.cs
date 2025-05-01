using System;
using Model3D.AsposeModel;

namespace ViewMotion.Models;

class CameraMotionOptions
{
    public CameraOptions CameraStartOptions { get; set; }
    public Func<int, int, (Vector3 pos, Vector3 look, Vector3 up)> CameraFn { get; set; }
}