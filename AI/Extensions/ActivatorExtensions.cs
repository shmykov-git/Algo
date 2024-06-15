using System.Runtime.CompilerServices;
using AI.Model;
using AI.NBrain.Activators;

namespace AI.Extensions;

public static class ActivatorExtensions
{
    public static NActivator ToActivator(this NAct act, NOptions options) => act switch
    {
        NAct.None => new NoneActivator(options),
        NAct.Sigmoid => new SigmoidActivator(options),
        NAct.Tanh => new TanhActivator(options),
        NAct.Sin => new SinActivator(options),
        NAct.SinC => new SinActivator(options),
        _ => throw new NotImplementedException(options.Act.ToString())
    };
}
