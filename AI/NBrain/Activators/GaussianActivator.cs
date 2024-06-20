using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class GaussianActivator : NActivator
{
    public GaussianActivator(NModel model) : base(model)
    {
        Func = NFuncs.GetGaussianFn(a);
        DerFunc = NFuncs.GetDerGaussianFn(a);
    }
}
