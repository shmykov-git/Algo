using Model3D;

namespace ViewMotion.Models;

class CameraOptions
{
    public Vector3 Position { get; set; }
    public Vector3 LookDirection { get; set; }
    public Vector3 UpDirection { get; set; }
    public double FieldOfView { get; set; } = 60;
    public bool RotateUpDirection { get; set; }

    public CameraOptions Copy()
    {
        return new CameraOptions
        {
            Position = Position,
            LookDirection = LookDirection,
            UpDirection = UpDirection,
            FieldOfView = FieldOfView,
            RotateUpDirection = RotateUpDirection
        };
    }
}