using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AI.Model;
using AI.NBrain;
using AI.NBrain.Activators;

namespace AI.Extensions;

public static class ActivatorExtensions
{
    private static ConcurrentDictionary<(NModel, int), NActivator> layerActivators = new();

    public static NActivator ToActivator(this NOptions options, NModel model, int lv)
    {
        var act = options.Activators.Length > 0 ? options.Activators[lv] : options.Activator;

        return act switch
        {
            NAct.Line => new LineActivator(model),
            NAct.Sigmoid => new SigmoidActivator(model),
            NAct.Tanh => new TanhActivator(model),
            NAct.Sin => new SinActivator(model),
            NAct.SinB => new SinBActivator(model),
            NAct.Sinc => new SincActivator(model),
            NAct.Gaussian => new GaussianActivator(model),
            NAct.Softmax => layerActivators.GetOrAdd((model, lv), _ => new SoftmaxActivator(lv, model)), // todo: check
            NAct.ReLU => new ReLUActivator(model),
            NAct.LReLU => new LReLUActivator(model),
            _ => throw new NotImplementedException(act.ToString())
        };
    }
}
