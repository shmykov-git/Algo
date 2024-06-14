using System.Runtime.CompilerServices;
using AI.Model;

namespace AI.NBrain.Activators;

public static class ActivatorExtensions
{
    public static NActivator ToActivator(this NActivatorType type, NOptions options) => type switch
    {
        NActivatorType.Sigmoid => new SigmoidActivator(options),
        NActivatorType.Tanh => new TanhActivator(options),
        NActivatorType.Sin => new SinActivator(options),
        _ => throw new NotImplementedException(options.Activator.ToString())
    };
}
