using System.Runtime.CompilerServices;
using AI.Model;

namespace AI.NBrain.Activators;

public static class ActivatorExtensions
{
    public static NActivator ToActivator(this NAct act, NOptions options) => act switch
    {
        NAct.None => new NoneActivator(options),
        NAct.Sigmoid => new SigmoidActivator(options),
        NAct.Tanh => new TanhActivator(options),
        NAct.Sin => new SinActivator(options),
        _ => throw new NotImplementedException(options.Act.ToString())
    };
}
