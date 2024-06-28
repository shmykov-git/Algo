using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SinBActivator : NActivator
{
    public SinBActivator(NModel model) : base(model)
    {
        Func = NFuncs.GetSinBFn(a, model.options.SinBias);
        DerFunc = NFuncs.GetDerSinFn(a);
    }
}
