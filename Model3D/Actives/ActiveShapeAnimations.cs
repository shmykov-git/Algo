using System;

namespace Model3D.Actives;

public static class ActiveShapeAnimations
{
    public static Action<ActiveShape> BlowUp(double stepPower, int from = 0, int? to = null) => a =>
    {
        if (from <= a.Options.StepNumber && (!to.HasValue || a.Options.StepNumber < to.Value))
            a.Options.BlowPower += stepPower;
    };
}
