using System.Diagnostics;
using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel
{
    private Queue<N> computeQueue = new();

    public double[] Predict(double[] vInput)
    {
        ComputeOutputs(vInput);

        return output.Select(n => n.f).ToArray();
    }

    private void SetInput(double[] vInput)
    {
        if (vInput.Length != input.Count)
            throw new InvalidInputDataException();

        input.Count.Range().ForEach(i => input[i].f = vInput[i]);
    }

    private void ComputeN(N n)
    {
        if (!n.isInput)
        {
            // compute output (f) from input (xx)
            n.f = n.activatorFn(n.xx);
            n.f = n.dampingFn(n.f);
        }

        // pass signal from output a to input b
        n.es.ForEach(e => e.b.xx += e.w * n.f);

        n.computed = true;
    }

    public void ComputeOutputs(double[] vInput)
    {
        SetInput(vInput);

        // compute cleanup
        ns.ForEach(n => { n.xx = 0; n.computed = false; });

        input.ForEach(computeQueue.Enqueue);

        var counter = 10000;

        while (computeQueue.TryDequeue(out var n))
        {
            if (counter-- == 0)
                throw new AlgorithmException("cannot compute");

            if (n.computed)
                continue;

            if (n.isInput || n.backEs.All(e => e.a.computed))
            {
                ComputeN(n);

                // pass to compute b
                n.es.ForEach(e => computeQueue.Enqueue(e.b));
            }
            else
                computeQueue.Enqueue(n);
        }

        double Avg(Func<N, bool> predicate) => ns.Any(predicate) ? ns.Where(predicate).Average(n => n.f) : -1;
        avgX = (Avg(n => !n.isInput && n.f > 0.5), Avg(n => !n.isInput && n.f < 0.5));
    }
}
