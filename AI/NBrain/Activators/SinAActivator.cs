﻿using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SinAActivator : NActivator
{
    public SinAActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSinAFn(alfa * power, options.SinA);
        DerFunc = NFuncs.GetDerSinFn(alfa * power);
    }
}