using AI.Model;

namespace AI.NBrain.Activators;

public class NoneActivator : NActivator
{
    public NoneActivator(NOptions options) : base(options)
    {
        Func = x => x;
        DerFunc = (_, _) => 1;
    }
}