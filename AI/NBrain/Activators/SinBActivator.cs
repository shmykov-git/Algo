using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SinBActivator : NActivator
{
    public SinBActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSinAFn(a, options.SinA);
        DerFunc = NFuncs.GetDerSinFn(a);
    }
}
