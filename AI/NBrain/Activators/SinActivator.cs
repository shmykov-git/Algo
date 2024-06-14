using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SinActivator : NActivator
{
    public SinActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSinFn(alfa, power);
        DerFunc = NFuncs.GetDerSinFn(alfa, power);
    }
}