using System.Drawing;
using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public static class ActiveWorldValues
{
    public static ActiveShapeOptions DefaultActiveShapeOptions => new ActiveShapeOptions
    {
        ShowMeta = false,
        AllowTriangulation0 = false,
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
        Type = ActiveShapeOptions.ShapeType.Unknown,
        UseSkeleton = true,
        Skeleton = new ActiveShapeOptions.SkeletonOptions
        {
            ShowPoints = false,
            Type = ActiveShapeOptions.SkeletonType.ShapeSizeRatioRadius,
            Radius = 0.3,
            Power = 2,
        },
        MaterialPower = 1,
        UseMaterialDamping = true,
        MaterialDamping = 0.001,
        UseBlow = false,
        BlowPower = 2,
        Fix = new(),
        UseInteractions = true,
        UseSelfInteractions = false,
    };

    public static ActiveWorldOptions DefaultActiveWorldOptions => new ActiveWorldOptions
    {
        SceneCount = 2000,
        StepsPerScene = 10,
        OverCalculationMult = 1,
        MaterialForceMult = 0.001,
        MaterialForceBorder = 0.75,
        MaterialThickness = 0.175,
        JediMaterialThickness = 0.025,
        UsePowerLimit = true,
        PowerLimit = 0.01,
        ParticleConst = 0.00005,
        PlaneConst = 0.00001,
        PressurePower = 1,
        PressurePowerMult = 0.0001,
        AllowModifyStatics = false,
        UseGround = true,
        Ground = new ActiveWorldOptions.GroundOptions
        {
            Gravity = new Vector3(0, -0.0000005, 0),
            GravityPower = 1,
            FrictionForce = 1.2,
            ClingForce = 1.5,
            Wind = new Vector3(0.0000002, 0, 0),
            WindPower = 0,
            ShowGround = true,
            LineMult = 3,
            Mult = 10,
            Size = 11,
            Color = Color.Black,
            UseWaves = false,
            WavesSize = 1,
        },
        UseMassCenter = false,
        MassCenter = new ActiveWorldOptions.MassCenterOptions
        {
            MassCenter = new Vector3(0, 0, 0),
            GravityPower = 1,
            GravityConst = 0.0000005
        },
        InteractionType = InteractionType.ParticleWithPlane,
        Interaction = new ActiveWorldOptions.InteractionOptions
        {
            EdgeSizeMult = 1.5,
            EdgeSize = null, //auto
            SelfInteractionGraphDistance = 3,
            ParticleForce = 1,
            InteractionAreaScale = new Vector3(2, 2, 2),
            UseVolumeMass = true,
            FrictionForce = 1,
            ClingForce = 1,
            ElasticForce = 1,
        },



        Debug = new WorldDebugOptions
        {
            DebugPlaneMaterialPenetration = true,
        }
    };
}
