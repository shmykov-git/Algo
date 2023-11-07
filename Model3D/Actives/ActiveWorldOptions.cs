using System;
using System.Drawing;
using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public class ActiveWorldOptions
{
    public event Action<ActiveWorld> OnStep;
    public void Step(ActiveWorld world) => OnStep?.Invoke(world);
    public int SkipSteps { get; set; }
    public int StepNumber { get; set; }
    public int SceneCount { get; set; }
    public int StepsPerScene { get; set; }
    public int OverCalculationMult { get; set; } // same world forces, but not material forces
    public bool UseMaterialDamping { get; set; }
    public double MaterialForceMult { get; set; }
    public double CollideForceMult { get; set; }
    public Vector3 Gravity { get; set; }
    public double GravityPower {  get; set; }
    public Vector3 Wind { get; set; }
    public double WindPower { get; set; }
    public double PressurePower { get; set; }
    public double PressurePowerMult { get; set; }
    public double FrictionForce { get; set; }
    public double ClingForce { get; set; }
    public bool AllowModifyStatics {  get; set; }
    public GroundOptions Ground { get; set; }
    public bool UseInteractions { get; set; }

    public class GroundOptions
    {
        public int Size { get; set; }
        public double LineMult { get; set; }
        public double Mult { get; set; }
        public Color? Color { get; set; }
        public bool UseWaves { get; set; }
        public double WavesSize { get; set; }
        public double Y { get; set; }
    }
}


