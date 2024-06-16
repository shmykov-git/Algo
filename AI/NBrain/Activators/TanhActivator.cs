using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class TanhActivator : NActivator
{
    public TanhActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetTanhFn(alfa * power);
        DerFunc = NFuncs.GetDerTanhFn(alfa * power);
    }
}
