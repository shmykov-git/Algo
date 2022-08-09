using Aspose.ThreeD.Utilities;

namespace ViewMotion.Models;

class CameraOptions
{
    public Vector3 Position { get; set; }
    public Vector3 LookDirection { get; set; }
    public Vector3 UpDirection { get; set; }
    public double FieldOfView { get; set; } = 60;
}