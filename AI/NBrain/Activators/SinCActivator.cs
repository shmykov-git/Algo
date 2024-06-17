using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SincActivator : NActivator
{
    public SincActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSincFn(a);
        DerFunc = NFuncs.GetDerSincFn(a);
    }
}
