using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SinCActivator : NActivator
{
    public SinCActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSinCFn(alfa, power, options.SinC);
        DerFunc = NFuncs.GetDerSinFn(alfa, power);
    }
}
