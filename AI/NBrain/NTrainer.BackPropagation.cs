using AI.Model;
using Model.Extensions;
using Model.Libraries;

namespace AI.NBrain;

public partial class NTrainer
{
    private void LearnBackPropagationOutput(N n, double fExpected)
    {
        n.delta = -n.act.DerFunc(n) * (fExpected - n.f);
    }

    private void LearnBackPropagation(N n)
    {
        n.delta = n.act.DerFunc(n) * n.es.Sum(e => e.b.delta * e.w);

        n.es.ForEach(e =>
        {
            if (e.unwanted)
                LearnBackPropagationUnwantedE(e);
            else
                LearnBackPropagationE(e);
        });
    }

    private void LearnBackPropagationE(E e)
    {
        var dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.f;
        e.w -= dw;
        e.sumDw += dw; // e.dw = dw;
    }

    private void LearnBackPropagationUnwantedE(E e)
    {
        (e.w, var dw) = NextUnwantedW(e.uW0, e.w);
        e.sumDw += dw;
    }

    private (double w, double dw) NextUnwantedW(double w0, double w)
    {
        if (w.Abs() < Values.Epsilon9)
            return (0, 0);

        var dw = w0 / (options.EpochUnwanted * options.TrainData.Length);

        return (w - dw, dw);
    }
}
