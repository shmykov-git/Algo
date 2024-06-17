using AI.Model;

namespace AI.NBrain.Activators;

public class LineActivator : NActivator
{
    public LineActivator(NOptions options) : base(options)
    {
        Func = x => a * x;
        DerFunc = (_, _) => a;
    }
}