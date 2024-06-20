using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SinActivator : NActivator
{
    public SinActivator(NModel model) : base(model)
    {
        Func = NFuncs.GetSinFn(a);
        DerFunc = NFuncs.GetDerSinFn(a);
    }
}
