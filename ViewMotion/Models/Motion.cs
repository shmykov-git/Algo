using System;
using System.Threading.Tasks;
using Shape = Model.Shape;

namespace ViewMotion.Models;

class Motion
{
    public MotionOptions Options { get; set; }
    public CameraMotionOptions CameraMotionOptions { get; set; }
    public double? CameraDistance = null;
    public Shape? Shape;
    public Func<int, Action<Shape>, Task<bool>> Step;
}