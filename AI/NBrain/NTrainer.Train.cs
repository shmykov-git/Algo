using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
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

    /// <summary>
    /// Метод обратного распространения ошибки
    /// https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%BE%D0%B3%D0%BE_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%BE%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F_%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8
    /// </summary>
    private double TrainCase(NModel model, double[] tInput, double[] tExpected)
    {
        if (tExpected.Length != options.Topology[^1])
            throw new InvalidExpectedDataException();

        void LearnBackPropagationOutput(N n, int k)
        {
            n.delta = -n.activatorDerFFn(n.f) * (tExpected[k] - n.f); // f * (1 - f)
            n.learned = true;
        }

        void LearnBackPropagation(N n)
        {
            n.delta = n.activatorDerFFn(n.f) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                var dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.f;
                e.w -= dw;
                e.sumDw += dw;
                //e.dw = dw;
            });

            n.learned = true;
        }

        model.ComputeOutputs(tInput);

        // learn cleanup
        Queue<N> learnQueue = new();

        // skip no compute nodes to learn (to finish process)
        model.ns.ForEach(n => { n.learned = n.es.Count == 0; }); // todo: fix isOutput

        model.output.ForEach(LearnBackPropagationOutput);
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
