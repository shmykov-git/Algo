using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NTrainer
{
    public async Task<double> Train()
    {
        var data = options.Training.ToArray();
        var pN = options.ParallelCount;
        var pNs = (pN).Range().ToArray();
        NModel[] models = [model];
        var es = model.es.ToArray();
        var symmetryLen = (int)Math.Round(options.SymmetryFactor * data.Length / pN);
        
        if (symmetryLen == 0)
            symmetryLen = 1;

        double ModelStep(int mI, (double[] input, double[] expected) t, int k)
        {
            if (mI != 0 && k % symmetryLen == 0)
            {
                models[mI].es.ForEach((e, i) =>
                {
                    Interlocked.Exchange(ref es[i].sumDw, e.sumDw);
                    e.sumDw = 0;
                });
            }

            return TrainCase(models[mI], t.input, t.expected);
        }

        if (options.ShaffleFactor > 0)
            data.Shaffle((int)(options.ShaffleFactor * (3 * data.Length + 7)), rnd);

        if (options.CleanupTrainTails)
            CleanupTrainTails();

        if (data.Length % options.ParallelCount != 0)
            throw new NotImplementedException();


        double sumError = 0;

        for(var k =0; k< data.Length / pN; k++)
        {
            if (k % symmetryLen == 0)
            {
                model.es.ForEach(e =>
                {
                    e.dw = e.sumDw / (symmetryLen * pN);
                    e.sumDw = 0;
                });
            }

            if (pN == 1)
            {
                sumError += ModelStep(0, data[k], k);
            }
            else
            {
                // медленнее в 100!! раз
                models = (pN).Range().Select(i => i == 0 ? model : model.Clone()).ToArray();
                var errors = await pNs.SelectInParallelAsync(j => ModelStep(j, data[k * pN + j], k));
                sumError += errors.Sum();
            }
        }

        return sumError / data.Length;
    }

    public void LearnBackPropagationOutput(N n, double fExpected)
    {
        n.delta = -n.act.DerFunc(n.xx, n.f) * (fExpected - n.f);
    }

    public void LearnBackPropagation(N n)
    {
        n.delta = n.act.DerFunc(n.xx, n.f) * n.es.Sum(e => e.b.delta * e.w);

        n.es.ForEach(e =>
        {
            var dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.f;
            e.w -= dw;
            e.sumDw += dw; // e.dw = dw;
        });
    }

    private double TrainCase(NModel model, double[] tInput, double[] tExpected)
    {
        if (tExpected.Length != options.Topology[^1])
            throw new InvalidExpectedDataException();

        model.ComputeOutputs(tInput);

        // learn cleanup
        Queue<N> learnQueue = new();

        // skip no compute nodes to finish learn process in any cases
        model.ns.ForEach(n => { n.learned = n.es.Count == 0; });

        model.output.ForEach((n, i) => 
        {
            LearnBackPropagationOutput(n, tExpected[i]);
            n.learned = true;
        });

        model.output.SelectMany(n => n.backEs.Select(e => e.a)).Distinct().ForEach(learnQueue.Enqueue);

        var counter = 10000;

        while (learnQueue.TryDequeue(out var n))
        {
            if (counter-- == 0)
                throw new AlgorithmException("cannot learn");

            if (n.learned)
                continue;

            if (n.es.All(e => e.b.learned))
            {
                LearnBackPropagation(n);
                n.learned = true;
                n.backEs.Select(e => e.a).ForEach(learnQueue.Enqueue);
            }
            else
                learnQueue.Enqueue(n);
        }

        var err = 0.5f * model.output.Select((n, i) => (tExpected[i] - n.f).Pow2()).Sum();
        model.error = err;
        model.avgDelta = model.ns.Average(n => n.delta.Abs());

        return err;
    }
}
