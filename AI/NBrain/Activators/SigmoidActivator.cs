using AI.Libraries;
using AI.Model;

namespace AI.NBrain.Activators;

public class SigmoidActivator : NActivator
{
    public SigmoidActivator(NModel model) : base(model)
    {
        Func = NFuncs.GetSigmoidFn(a);
        DerFunc = NFuncs.GetDerSigmoidFn(a);
    }
}
