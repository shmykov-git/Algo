using AI.Model;

namespace AI.NBrain.Activators;

public class SoftmaxActivator : NLayerActivator
{
    private N? _maxN = null;
    private N maxN => _maxN ??= layer.MaxBy(nn => nn.f)!;

    private double? _sumE;
    private double sumFF => _sumE ??= layer.Sum(n => n.ff);

    private double DerLayerFunc(N n)
    {
        var d = maxN == n ? 0 : 1;

        return a * n.f * (d - maxN.f);
    }

    private double LayerFunc(N n)
    {
        return n.ff / sumFF;
    }

    public SoftmaxActivator(int lv, NModel model) : base(lv, model)
    {
        PreFunc = n => Math.Exp(a * n.xx);
        Func = LayerFunc;
        DerFunc = DerLayerFunc;
    }
}
