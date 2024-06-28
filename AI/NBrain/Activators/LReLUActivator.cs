namespace AI.NBrain.Activators;

public class LReLUActivator : NActivator
{
    public LReLUActivator(NModel model) : base(model)
    {
        var b = model.options.ReLUBias;
        var aa = a * model.options.LReLUAlfa;
        Func = n => n.xx > 0 ? a * n.xx + b : aa * n.xx + b;
        DerFunc = n => n.xx > 0 ? a : aa;
    }
}
