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
        Color2 = null,
        RotationAngle = 0,
        RotationAxis = Vector3.ZAxis,
        RotationSpeedAngle = 0, // 0.001
        RotationSpeedAxis = Vector3.YAxis,
        UseSkeleton = false,
        ShowSkeletonPoint = false,
        SkeletonPower = 2,
        MaterialPower = 1,
        MaterialDamping = 0.001,
        UseBlow = false,
        BlowPower = 2,
        Fix = new(),
    };

    public static ActiveWorldOptions DefaultActiveWorldOptions => new ActiveWorldOptions
    {
        SceneCount = 2000,
        StepsPerScene = 10,
        OverCalculationMult = 1,
        Gravity = new Vector3(0, -0.0000005, 0),
        GravityPower = 1,
        Wind = new Vector3(0.0000002, 0, 0),
        WindPower = 0,
        CollideForceMult = 0.00005,
        MaterialForceMult = 0.001,
        FrictionForce = 1.2,
        ClingForce = 1.5,
        PressurePower = 1,
        PressurePowerMult = 0.0001,
        AllowModifyStatics = false,
        Ground = ActiveWorldDefaultGroundOptions,
        UseMaterialDamping = true,
    };

    public static ActiveWorldOptions.GroundOptions ActiveWorldDefaultGroundOptions => new ActiveWorldOptions.GroundOptions
    {
        LineMult = 3,
        Mult = 10,
        Size = 11,
        Color = Color.Black,
        UseWaves = false,
        WavesSize = 3,
    };
}
