﻿using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class TanhActivator : NActivator
{
    public TanhActivator(NModel model) : base(model)
    {
        Func = NFuncs.GetTanhFn(a);
        DerFunc = NFuncs.GetDerTanhFn(a);
    }
}
