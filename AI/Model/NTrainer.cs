using System.Diagnostics;
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
        var weightFn = NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);
        model = new NModel(options, rnd);

        //NGroup CreateGroup(int i) => new NGroup()
        //{
        //};

        //groups = [CreateGroup(0)];

        var input = (options.NInput).Range(_ => model.CreateN(0)).ToList();
        var hidden = (options.NHidden.Length).Range(i => (options.NHidden[i]).Range().Select(_ => model.CreateN(i + 1)).ToList()).ToList();
        var output = (options.NOutput).Range(_ => model.CreateN(options.NHidden.Length + 1)).ToList();

        var nns = new List<List<N>>[]
        {
            [model.input],
            hidden,
            [model.output],
        }.ToSingleList();

        model.nns = nns;
        model.RestoreIndices();

        var maxHidden = options.NHidden.Max();

        nns.SelectPair((aL, bL) => (aL, bL)).ForEach(pair => 
        {
            var (aL, bL) = pair;

            (Math.Max(aL.Count, bL.Count)).ForEach(i => 
            {
                var a = aL[i % aL.Count];
                var b = bL[i % bL.Count];
                a.es.Add(model.CreateE(a, b, weightFn(rnd.NextDouble())));
            });

            aL.ForEach(a => bL.ForEach(b =>
            {
                if (!a.IsLinked(b) && rnd.NextDouble() < options.LinkFactor)
                    a.es.Add(model.CreateE(a, b, weightFn(rnd.NextDouble())));
            }));
        });

        nns.SkipLast(2).ForEach((aL, i) => nns.Skip(i + 2).SkipLast(1).ForEach(bL =>
        {
            aL.ForEach(a => bL.ForEach(b =>
            {
                if (!a.IsLinked(b) && rnd.NextDouble() < options.CrossLinkFactor)
                    a.es.Add(model.CreateE(a, b, weightFn(rnd.NextDouble())));
            }));
        }));
       
        model.RestoreBackEs();
    }


    public void GrowUp()
    {
        if (options.NHidden[0] > options.NHiddenUp[0] || options.NHidden.Length > options.NHiddenUp.Length)
            throw new NotSupportedException("grow up only");

        if (model.nns[1].Count < options.NHiddenUp[0])
        {
            var a = model.input[rnd.Next(model.input.Count)];
            var b = model.output[rnd.Next(model.output.Count)];

            //model.AddN(a, b, 1);
        }

        var destLv = options.NHiddenUp.Length + 2;
        var curLv = model.maxLv;

        if (destLv > curLv)
        {

        }
        else
        {

        }
    }

    //public void GrowUp1()
    //{
    //    var lvs = model.GetLevels();
    //    var max = lvs.Max();

    //    N GetLevelN(int lv)
    //    {
    //        var ns = model.ns.Where(n => lvs[n.i] == lv).ToArray();
    //        return ns[rnd.Next(ns.Length)];
    //    }

    //    int CountLv(int lv) => model.ns.Count(n => lvs[n.i] == lv);
    //    bool IsLinked(N a, N b) => a.es.Any(e => e.b == b);

    //    if (max == 2)
    //    {
    //        var a = GetLevelN(1);
    //        var b = GetLevelN(2);
    //        model.AddN(a, b, false);

    //        return;
    //    }

    //    if (max == 3)
    //    {
    //        if (CountLv(1) != CountLv(2))
    //        {
    //            var a = GetLevelN(1);
    //            var b = GetLevelN(3);
    //            model.AddN(a, b, false);

    //            return;
    //        }
    //        else
    //        {
    //            N a, b;
    //            do
    //            {
    //                a = GetLevelN(1);
    //                b = GetLevelN(2);
    //            } while (IsLinked(a, b));

    //            model.AddE(a, b);

    //            return;
    //        }
    //    }

    //    throw new NotSupportedException();
    //}

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