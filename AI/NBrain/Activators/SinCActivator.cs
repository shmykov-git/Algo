using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SincActivator : NActivator
{
    public SincActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetSincFn(alfa * power);
        DerFunc = NFuncs.GetDerSincFn(alfa * power);
    }
}
