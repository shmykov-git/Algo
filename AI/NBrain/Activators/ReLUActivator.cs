namespace AI.NBrain.Activators;

public class ReLUActivator : NActivator
{
    public ReLUActivator(NModel model) : base(model)
    {
        var b = model.options.ReLUBias;
        Func = n => n.xx > 0 ? a * n.xx + b : b;
        DerFunc = n => n.xx > 0 ? a : 0;
    }
}
