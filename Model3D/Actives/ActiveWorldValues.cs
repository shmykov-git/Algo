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
        UseSkeleton = false,
        ShowSkeletonPoint = false,
        SkeletonPower = 0.002,
        MaterialPower = 0.001,
        BlowPower = 0, //0.001,
        BlowUp = null,
    };

    public static ActiveWorldOptions DefaultActiveWorldOptions => new ActiveWorldOptions
    {
        SceneCount = 2000,
        StepsPerScene = 10,
        OverCalculationMult = 1,
        MaterialDapming = 0.8,
        Gravity = new Vector3(0, -0.0000005, 0),
        GravityPower = 1,
        Wind = new Vector3(0, 0, -0.0000002),
        WindPower = 0,
        MaterialForceMult = 0.00005,
        FrictionForce = 1.2,
        ClingForce = 1.5,
        AllowModifyStatics = false,
        DefaultGround = ActiveWorldDefaultGroundOptions,
    };

    public static ActiveShapeBlowUpOptions DefaultActiveShapeBlowUpOptions => new ActiveShapeBlowUpOptions
    {
        SinceStep = 100 * 10,
        BlowUpStepPower = 0.0000001,
    };

    public static ActiveWorldDefaultGroundOptions ActiveWorldDefaultGroundOptions => new ActiveWorldDefaultGroundOptions
    {
        LineMult = 3,
        Mult = 10,
        Size = 11,
        Color = Color.Black,
        UseWaves = false,
        WavesSize = 3,
    };
}
