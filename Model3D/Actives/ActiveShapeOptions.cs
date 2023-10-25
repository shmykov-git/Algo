using System.Drawing;
using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public class ActiveShapeOptions
{
    public bool ShowMeta { get; set; }
    public double MetaLineMult { get; set; }
    public double MetaPointMult { get; set; }
    public Color? Color1 { get; set; }
    public Color? Color2 { get; set; }
    public double RotationAngle { get; set; }
    public Vector3 RotationAxis { get; set; }
    public Vector3? RotationCenter { get; set; }
    public double RotationSpeedAngle { get; set; }
    public Vector3 RotationSpeedAxis { get; set; }
    public Vector3? RotationSpeedCenter { get; set; }
    public bool UseSkeleton { get; set; }
    public double SkeletonPower { get; set; }
    public bool ShowSkeletonPoint { get; set; }
    public double MaterialPower { get; set; }
    public double BlowPower { get; set; }

    public ActiveShapeBlowUpOptions BlowUp { get; set; }
}
