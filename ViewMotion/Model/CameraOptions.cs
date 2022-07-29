using System.Windows.Media;
using Aspose.ThreeD.Utilities;

namespace ViewMotion.Model;

class LightOptions
{
    public Color Color { get; set; }
    public Vector3 Direction { get; set; }
}
class CameraOptions
{
    public Vector3 Position { get; set; }
    public Vector3 LookDirection { get; set; }
    public Vector3 UpDirection { get; set; }
    public double FieldOfView { get; set; } = 60;
}