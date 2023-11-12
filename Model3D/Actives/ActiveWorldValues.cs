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
        Color2 = Color.SandyBrown,
        RotationAngle = 0,
        RotationAxis = Vector3.ZAxis,
        RotationSpeedAngle = 0, // 0.001
        RotationSpeedAxis = Vector3.YAxis,
        Speed = Vector3.Origin,
        Mass = 1,
        UseSkeleton = true,
        ShowSkeletonPoint = false,
        SkeletonPower = 2,
        MaterialPower = 1,        
        UseMaterialDamping = true,
        MaterialDamping = 0.001,
        UseBlow = false,
        BlowPower = 2,
        Fix = new(),
        UseInteractions = true,
        UseSelfInteractions = false,
        ColliderScale = 0.73 * new Vector3(1, 1, 1),
    };

    public static ActiveWorldOptions DefaultActiveWorldOptions => new ActiveWorldOptions
    {
        SceneCount = 2000,
        StepsPerScene = 10,
        OverCalculationMult = 1,
        CollideForceMult = 0.00005,
        MaterialForceMult = 0.001,
        MaterialForceBorder = 0.75,
        UsePowerLimit = true,
        PowerLimit = 0.01,
        FrictionForce = 1.2,
        ClingForce = 1.5,
        PressurePower = 1,
        PressurePowerMult = 0.0001,
        AllowModifyStatics = false,
        UseGround = true,
        Ground = new ActiveWorldOptions.GroundOptions
        {
            GravityPower = 1,
            WindPower = 0,
            Gravity = new Vector3(0, -0.0000005, 0),
            Wind = new Vector3(0.0000002, 0, 0),
            ShowGround = true,
            LineMult = 3,
            Mult = 10,
            Size = 11,
            Color = Color.Black,
            UseWaves = false,
            WavesSize = 3,
        },
        UseMassCenter = false,
        MassCenter = new ActiveWorldOptions.MassCenterOptions
        {
            MassCenter = new Vector3(0, 0, 0),
            GravityPower = 1,
            GravityConst = 0.0000005
        },
        UseInteractions = true,
        Interaction = new ActiveWorldOptions.InteractionOptions
        {
            EdgeSizeMult = 1.5,
            EdgeSize = null, //auto
            SelfInteractionGraphDistance = 3,
            InteractionForce = 1,
            InteractionAreaScale = new Vector3(2, 2, 2),
            UseMass = true,            
        },
    };
}
