using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using AI.Model;
using AI.NBrain;
using AI.NBrain.Activators;

namespace AI.Extensions;

public static class ActivatorExtensions
{
    private static ConcurrentDictionary<(NModel, int), NActivator> layerActivators = new();

    public static NActivator ToActivator(this NAct act, NModel model, int lv) => act switch
    {
        NAct.Line => new LineActivator(model),
        NAct.Sigmoid => new SigmoidActivator(model),
        NAct.Tanh => new TanhActivator(model),
        NAct.Sin => new SinActivator(model),
        NAct.SinB => new SinBActivator(model),
        NAct.Sinc => new SincActivator(model),
        NAct.Gaussian => new GaussianActivator(model),
        NAct.Softmax => layerActivators.GetOrAdd((model, lv), _ => new SoftmaxActivator(lv, model)),
        _ => throw new NotImplementedException(act.ToString())
    };
}
