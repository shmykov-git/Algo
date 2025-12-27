using Model3D;
using System.Windows.Media;

namespace ViewMotion.Models;

public enum LightType
{
    Directional,
    Ambient
}
class LightOptions
{
    public LightType LightType { get; set; }
    public Color Color { get; set; }
    public Vector3 Direction { get; set; }
}
