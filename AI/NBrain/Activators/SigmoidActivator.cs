﻿using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SigmoidActivator : NActivator
{
    public SigmoidActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSigmoidFn(alfa * power);
        DerFunc = NFuncs.GetDerSigmoidFn();
    }
}