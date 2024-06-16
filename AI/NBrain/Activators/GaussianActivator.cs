using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class GaussianActivator : NActivator
{
    public GaussianActivator(NOptions options) : base(options)
    {
        Func = NFuncs.GetGaussianFn(alfa * power);
        DerFunc = NFuncs.GetDerGaussianFn(alfa * power);
    }
}
