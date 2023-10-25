using System.Drawing;
using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public static class ActiveWorldValues
{
    public static ActiveShapeOptions DefaultActiveShapeOptions => new ActiveShapeOptions
    {
        ShowMeta = false,
        MetaPointMult = 2,
        MetaLineMult = 2,
        Color1 = Color.SaddleBrown,
        Color2 = Color.Red,
        RotationAngle = 0,
        RotationAxis = Vector3.ZAxis,
        RotationSpeedAngle = 0, // 0.001
        RotationSpeedAxis = Vector3.YAxis,
        UseSkeleton = true,
        ShowSkeletonPoint = false,
        SkeletonPower = 0.002,
        MaterialPower = 0.001,
        BlowPower = 0, //0.001,
        BlowUp = null
    };

    public static ActiveWorldOptions DefaultActiveWorldOptions => new ActiveWorldOptions
    {
        SceneCount = 2000,
        StepsPerScene = 10,
        UseDefaultGround = true,
        MaterialDapming = 0.8,
        Gravity = new Vector3(0, -0.000001, 0),
        Wind = Vector3.Origin,
        FrictionForce = 0.00006,
        ClingForce = 0.000075
    };

    public static ActiveShapeBlowUpOptions DefaultActiveShapeBlowUpOptions = new ActiveShapeBlowUpOptions
    {
        SinceStep = 100 * 10,
        BlowUpStepPower = 0.0000001,
    };
}
