﻿using Aspose.ThreeD.Utilities;

namespace Model3D.Actives;

public class ActiveWorldOptions
{
    public int SceneCount { get; set; }
    public int StepsPerScene { get; set; }
    public int OverCalculationMult { get; set; }
    public double MaterialDapming { get; set; }
    public double MaterialForceMult { get; set; }
    public Vector3 Gravity { get; set; }
    public double GravityPower {  get; set; }
    public Vector3 Wind { get; set; }
    public double WindPower { get; set; }
    public double FrictionForce { get; set; }
    public double ClingForce { get; set; }
    public bool UseDefaultGround {  get; set; }
}