using System;
using System.Collections.Generic;
using System.Drawing;
using Model3D.AsposeModel;
using MessagePack;
using Meta.Extensions;
using Model;
using Model3D.Extensions;

namespace Model3D.Actives;

public class ActiveShapeOptions
{
    public event Action<ActiveShape> OnStep;
    public event Func<Shape, Shape> OnShow;

    public void Step(ActiveShape activeShape) => OnStep?.Invoke(activeShape);
    public Shape Show(Shape shape) => OnShow.RollRaise(shape);

    public ActiveWorldOptions WorldOptions { get; set; }

    public Shape Docked { get; set; }
    public ShapeType Type { get; set; }  
    public int StepNumber { get; set; }
    public bool ShowMeta { get; set; }
    public bool AllowTriangulation0 { get; set; }
    public double MetaLineMult { get; set; }
    public double MetaPointMult { get; set; }
    public Color? Color1 { get; set; }
    public Color? Color2 { get; set; }
    public double RotationAngle { get; set; }
    public Vector3 RotationAxis { get; set; }
    public Vector3? RotationCenter { get; set; }
    public Vector3 Speed { get; set; }
    public double Mass { get; set; }
    public double RotationSpeedAngle { get; set; }
    public Vector3 RotationSpeedAxis { get; set; }
    public Vector3? RotationSpeedCenter { get; set; }
    public bool UseSkeleton { get; set; }
    public SkeletonOptions Skeleton { get; set; }
    public double MaterialPower { get; set; }
    public bool UseMaterialDamping { get; set; }
    public double MaterialDamping { get; set; }
    public bool UseBlow { get; set; }
    public double BlowPower { get; set; }
    public FixOptions Fix { get; set; }
    public bool UseInteractions { get; set; }
    public bool UseSelfInteractions { get; set; }

    public enum ShapeType
    {
        Unknown,
        D1,
        D2,
        D3
    }

    [Flags]
    public enum FixDock
    {
        None = 0,
        Point = 1,
        Left = 2,
        Top = 4,
        Right = 8,
        Bottom = 16,
        Back = 32,
        Front = 64
    }

    public enum SkeletonType
    {
        CenterPoint,
        ShapeSizeRatioRadius,
        Radius
    }

    public class SkeletonOptions
    {
        public double Power { get; set; }
        public SkeletonType Type { get; set; }
        public double Radius { get; set; }
        public bool ShowPoints { get; set; }
    }

    public class FixOptions
    {
        public FixDock Dock { get; set; } = FixDock.None;
        public Vector3 Point { get; set; }
        public Vector3 Direction { get; set; } = Vector3.ZAxis;
        public double Distance { get; set; } = 0.1;

        public static implicit operator FixOptions(FixDock dock) => new FixOptions { Dock = dock };
    }

    public void With(Action<ActiveShapeOptions> action) => action(this);
}

public class ActiveWorldOptions
{
    public event Action<ActiveWorld> OnStep;
    public void Step(ActiveWorld world) => OnStep?.Invoke(world);
    public int StepNumber { get; set; }
    public double ForceInteractionRadius { get; internal set; }

    public int SkipSteps { get; set; }
    public int SceneCount { get; set; }
    public int StepsPerScene { get; set; }
    public int OverCalculationMult { get; set; } // same World forces, but not material forces
    public double MaterialForceBorder { get; set; }
    public double MaterialForceMult { get; set; }
    public double MaterialThickness { get; set; }
    public double JediMaterialThickness { get; set; }
    public double ParticleConst { get; set; }
    public double PlaneConst { get; set; }
    public double PressurePower { get; set; }
    public double PressurePowerMult { get; set; }
    public bool AllowModifyStatics {  get; set; }
    public InteractionType InteractionType { get; set; }
    public InteractionOptions Interaction { get; set; }
    public bool UseGround { get; set; }
    public GroundOptions Ground { get; set; }
    public bool UsePowerLimit { get; set; }
    public double PowerLimit { get; set; }
    public bool UseMassCenter { get; set; }
    public MassCenterOptions MassCenter { get; set; }


    public WorldDebugOptions Debug { get; set; }

    public bool UseExport { get; set; }
    public WorldExportOptions Export { get; set; }


    public class InteractionOptions
    {
        public double EdgeSizeMult { get; set; }
        public double? EdgeSize { get; set; }
        public int SelfInteractionGraphDistance { get; set; }
        public double ParticleForce { get; set; }
        public Vector3 InteractionAreaScale { get; set; }
        public bool UseVolumeMass {  get; set; }
        public double FrictionForce { get; set; }
        public double ClingForce { get; set; }
        //public double ClingForce { get; set; }
        public double ElasticForce { get; set; }
    }

    public class MassCenterOptions
    {
        public Vector3 MassCenter { get; set; }
        public double GravityPower { get; set; }
        public double GravityConst { get; set; }
    }

    public class GroundOptions
    {
        public Vector3 Gravity { get; set; }
        public double GravityPower { get; set; }
        public double FrictionForce { get; set; }
        public double ClingForce { get; set; }
        public Vector3 Wind { get; set; }
        public double WindPower { get; set; }
        public bool ShowGround { get; set; }
        public int Size { get; set; }
        public double LineMult { get; set; }
        public double Mult { get; set; }
        public Color? Color { get; set; }
        public bool UseWaves { get; set; }
        public double WavesSize { get; set; }
        public double Y { get; set; }
    }
}

[Flags]
public enum InteractionType
{
    None = 0,
    ParticleWithParticle = 1,
    ParticleWithPlane = 2,
    EdgeWithPlane = 4,
    Any = ParticleWithParticle | ParticleWithPlane | EdgeWithPlane
}

public class WorldDebugOptions
{
    public bool DebugPlaneMaterialPenetration { get; set; }
}

public class WorldExportOptions
{
    public Func<int, bool> FrameFn { get; set; } = i => (i % 40) == 0;       // StepPerScene = 10 * SceneCount = 2000
    public Func<int, bool> FrameSaveFn { get; set; } = i => (i % 2000) == 0;
    public string FileName { get; set; }
}

[MessagePackObject]
public class WorldExportState
{
    [Key(0)]
    public Active[] actives { get; set; }

    [MessagePackObject]
    public class Active
    {
        [Key(0)]
        public int offset { get; set; }
        [Key(1)]
        public int count { get; set; }
        [Key(2)]
        public int size { get; set; }
        [Key(3)]
        public List<float[][]> moves { get; set; }
    }
}