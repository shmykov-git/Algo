using System;

namespace Model3D.Actives;

public static class ActiveShapeAnimations
{
    public static Action<ActiveShape> BlowUp(double stepPower, int since = 0) => a =>
    {
        if (a.Options.StepNumber >= since)
            a.Options.BlowPower += stepPower;
    };
}
