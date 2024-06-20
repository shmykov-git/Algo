using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SincActivator : NActivator
{
    public SincActivator(NModel model) : base(model)
    {
        Func = NFuncs.GetSincFn(a);
        DerFunc = NFuncs.GetDerSincFn(a);
    }
}
