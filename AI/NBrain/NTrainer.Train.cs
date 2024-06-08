using System.Diagnostics.Metrics;
using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NTrainer
{
    public double Train()
    {
        var data = options.Training.ToArray();

        if (options.ShaffleFactor > 0)
            data.Shaffle((int)(options.ShaffleFactor * (3 * data.Length + 7)), rnd);

        if (options.CleanupTrainTails)
            CleanupTrainTails();

        var errs = data.Select(t => TrainCase(t.input, t.expected)).ToArray();

        var avgErr = errs.Average();
        model.trainError = avgErr;

        return avgErr;
    }

    private Queue<N> learnQueue = new();

    /// <summary>
    /// Метод обратного распространения ошибки
    /// https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%BE%D0%B3%D0%BE_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%BE%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F_%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8
    /// </summary>
    private double TrainCase(double[] tInput, double[] tExpected)
    {
        if (tExpected.Length != options.Topology[^1])
            throw new InvalidExpectedDataException();

        void LearnBackPropagationOutput(N n, int k)
        {
            n.delta = -n.f * (1 - n.f) * (tExpected[k] - n.f);
            n.learned = true;
        }

        void LearnBackPropagation(N n)
        {
            n.delta = n.f * (1 - n.f) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                e.dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.f;
                e.w -= e.dw;
            });

            n.learned = true;
        }

        model.ComputeOutputs(tInput);

        // learn cleanup
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
