using System;
using System.Drawing;
using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public class ActiveWorldOptions
{
    public event Action<ActiveWorld> OnStep;
    public void Step(ActiveWorld world) => OnStep?.Invoke(world);
    public int StepNumber { get; set; }
    public double ForceInteractionRadius { get; internal set; }

    public int SkipSteps { get; set; }
    public int SceneCount { get; set; }
    public int StepsPerScene { get; set; }
    public int OverCalculationMult { get; set; } // same world forces, but not material forces
    public double MaterialForceBorder { get; set; }
    public double MaterialForceMult { get; set; }
    public double CollideForceMult { get; set; }
    public double PressurePower { get; set; }
    public double PressurePowerMult { get; set; }
    public double FrictionForce { get; set; }
    public double ClingForce { get; set; }
    public bool AllowModifyStatics {  get; set; }
    public bool UseInteractions { get; set; }
    public InteractionOptions Interaction { get; set; }
    public bool UseGround { get; set; }
    public GroundOptions Ground { get; set; }
    public bool UsePowerLimit { get; set; }
    public double PowerLimit { get; set; }
    public bool UseMassCenter { get; set; }
    public MassCenterOptions MassCenter { get; set; }
    
    
    public class InteractionOptions
    {
        public double EdgeSizeMult { get; set; }
        public double? EdgeSize { get; set; }
        public int SelfInteractionGraphDistance { get; set; }
        public double InteractionForce { get; set; }
        public Vector3 InteractionAreaScale { get; set; }
        public bool UseMass {  get; set; }
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


