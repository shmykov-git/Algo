using System.Linq;
using AI.Exceptions;
using AI.Libraries;
using MathNet.Numerics.Random;
using Model.Extensions;

namespace AI.Model;

public class NTrainer
{
    private readonly NOptions options;
    private Random rnd;
    private NGroup[] groups;
    private double alfa;
    private double nu;
    
    public NModel model;

    public NTrainer(NOptions options)
    {
        this.options = options;
        this.alfa = options.Alfa;
        this.nu = options.Nu;
    }



    public void Init()
    {
        rnd = new Random(options.Seed);
        var getBaseWeightFn = NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);
        model = new NModel(options);

        //NGroup CreateGroup(int i) => new NGroup()
        //{
        //};

        //groups = [CreateGroup(0)];

        bool IsFilled(int i) => rnd.NextDouble() < options.FillFactor;

        model.input = (options.NInput).Range(_ => model.CreateN()).ToArray();
        var hidden = (options.NHidden.nLayers).Range(_ => (options.NHidden.n).Range().Where(IsFilled).Select(_ => model.CreateN()).ToArray()).ToArray();
        model.output = (options.NOutput).Range(_ => model.CreateN()).ToArray();

        var nns = new N[][][]
        {
            [model.input],
            hidden,
            [model.output],
        }.ToSingleArray();

        bool IsLinked(N n) => rnd.NextDouble() < options.LinkFactor;

        nns.SelectPair((aL, bL) => (aL, bL)).ForEach((p, lv) =>
        {
            p.aL.ForEach(a =>
            {
                a.es = p.bL.Where(IsLinked).Select(b => model.CreateE(a, b, getBaseWeightFn(rnd.NextDouble()))).ToList();

                if (a.es.Count == 0)
                    a.es = [model.CreateE(a, p.bL[rnd.Next(p.bL.Length)], getBaseWeightFn(rnd.NextDouble()))];
            });

            p.bL.Where(b => !p.aL.Any(a => a.es.Any(e => e.b == b))).ToArray()
                .ForEach(b =>
                {
                    var a = p.aL[rnd.Next(p.aL.Length)];
                    var e = model.CreateE(a, b, getBaseWeightFn(rnd.NextDouble()));
                    a.es = a.es.Concat([e]).ToList();
                });
        });

        model.ns = nns.ToSingleList();
        model.RestoreBackEs();
        model.RestoreIndices();

        if (options.DuplicatorsCount.HasValue)
        {
            string GetNUniqueKey(N n) => n.backEs.Select(e => e.a.i).OrderBy(i => i).SJoin("|") + "|_|" + n.es.Select(e=>e.b.i).OrderBy(i => i).SJoin("|");

            var removeNs = model.ns
                .GroupBy(GetNUniqueKey)
                .Where(gn => gn.Count() > options.DuplicatorsCount.Value)
                .SelectMany(gn => gn.Skip(options.DuplicatorsCount.Value))
                .ToHashSet();

            model.ns.Where(removeNs.Contains).ToArray().ForEach(model.RemoveN);
        }
    }

    public void CleanupTrainTails() => model.es.ForEach(e => e.dw = 0);

    public double Train()
    {
        var data = options.Training.ToArray();

        if (options.ShaffleFactor > 0)
            data.Shaffle((int)(options.ShaffleFactor * (3 * data.Length + 7)), rnd);

        if (options.CleanupTrainTails)
            CleanupTrainTails();

        model.ns.ForEach(n => { n.avgF = 0; });

        var errs = data.Select(t => TrainCase(t.input, t.expected)).ToArray();
        
        model.ns.ForEach(n => { n.avgF /= data.Length; });

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
        if (tExpected.Length != options.NOutput)
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
        model.ns.ForEach(n => { n.learned = false; });

        model.output.ForEach(LearnBackPropagationOutput);
        model.output.SelectMany(n => n.backEs.Select(e => e.a)).Distinct().ForEach(learnQueue.Enqueue);

        while (learnQueue.TryDequeue(out var n))
        {
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
        model.avgDelta = model.ns.Average(n=>n.delta.Abs());

        return err;
    }
}