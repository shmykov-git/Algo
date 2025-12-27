namespace AI.NBrain.Activators;

public class LineActivator : NActivator
{
    public LineActivator(NModel model) : base(model)
    {
        Func = n => a * n.xx;
        DerFunc = _ => a;
    }
}