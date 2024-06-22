using AI.Model;

namespace AI.NBrain.Activators;

public abstract class NLayerActivator : NActivator
{
    protected IEnumerable<N> layer => model.nns[lv];

    public NLayerActivator(int lv, NModel model) : base(model)
    {
        IsLayerActivator = true;
        layerPreComputed = () => layer.All(n => n.preComputed);
        this.lv = lv;
    }
}
