using System.Linq;
using AI.Exceptions;
using AI.Libraries;
using MathNet.Numerics.Random;
using Model.Extensions;

namespace AI.Model;

public class NBrain
{
    private readonly NOptions options;
    private Random rnd;
    private NGroup[] groups;
    private double alfa;
    private double nu;
    
    public NModel model;

    public NBrain(NOptions options)
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

        N CreateN() => new N()
        {
            dampingFn = NFuncs.GetDampingFn(options.DampingCoeff),
            sigmoidFn = NFuncs.GetSigmoidFn(options.Alfa),
            //g = groups[0]
        };

        E CreateE(N a, N b) => new E
        {
            dw = 0,
            w = getBaseWeightFn(rnd.NextSingle()),
            a = a,
            b = b,
        };

        bool IsFilled(int i) => rnd.NextSingle() < options.FillFactor;

        // brains
        model.nns = new List<List<N>>[]
        {
            [(options.NInput).Range(_=>CreateN()).ToList()],
            (options.NHidden.nLayers).Range(_ => (options.NHidden.n).Range().Where(IsFilled).Select(_=>CreateN()).ToList()).ToList(),
            [(options.NOutput).Range(_=>CreateN()).ToList()],
        }.ToSingleList();

        bool IsLinked(N n) => rnd.NextSingle() < options.LinkFactor;

        model.nns.SelectPair((aL, bL) => (aL, bL)).ForEach((p, lv) =>
        {
            p.aL.ForEach(a =>
            {
                a.es = p.bL.Where(IsLinked).Select(b => CreateE(a, b)).ToList();

                if (a.es.Count == 0)
                    a.es = [CreateE(a, p.bL[rnd.Next(p.bL.Count)])];
            });

            p.bL.Where(b => !p.aL.Any(a => a.es.Any(e => e.b == b))).ToArray()
                .ForEach(b =>
                {
                    var a = p.aL[rnd.Next(p.aL.Count)];
                    var e = CreateE(a, b);
                    a.es = a.es.Concat([e]).ToList();
                });
        });

        if (options.DuplicatorsCount.HasValue)
        {
            model.RestoreIndices();

            string GetNUniqueKey(N n) => model.GetBackEs(n).Select(e => e.a.i).OrderBy(i => i).SJoin("|") + "|_|" + n.es.Select(e=>e.b.i).OrderBy(i => i).SJoin("|");

            var removeNs = model.ns
                .GroupBy(GetNUniqueKey)
                .Where(gn => gn.Count() > options.DuplicatorsCount.Value)
                .SelectMany(gn => gn.Skip(options.DuplicatorsCount.Value))
                .ToHashSet();

            var removeEs = removeNs.SelectMany(n=>n.es.Concat(model.GetBackEs(n))).ToHashSet();

            model.nns.Select((ns, i) => (ns, i))
                .ToArray()
                .ForEach(v => model.nns[v.i] = v.ns.Where(n => !removeNs.Contains(n)).ToList());

            model.ns.ForEach(n => n.es = n.es.Where(e => !removeEs.Contains(e)).ToList());            
        }
    }

    public double Train()
    {
        var data = options.Training.ToArray();

        if (options.ShaffleFactor > 0)
            data.Shaffle((int)(options.ShaffleFactor * (3 * data.Length + 7)), rnd);

        if (options.CleanupPrevTrain)
            model.es.ForEach(e => e.dw = 0);

        var errs = data.Select(t => TrainCase(t.input, t.expected)).ToArray();
        var avgErr = errs.Average();
        model.trainError = avgErr;

        return avgErr;
    }

    double[] prev = null;
    /// <summary>
    /// Метод обратного распространения ошибки
    /// https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%BE%D0%B3%D0%BE_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%BE%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F_%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8
    /// </summary>
    private double TrainCase(double[] tInput, double[] tExpected)
    {
        if (tExpected.Length != options.NOutput)
            throw new InvalidExpectedDataException();

        model.ComputeOutputs(tInput);

        model.output.ForEach((n, k) =>
        {
            n.delta = -n.f * (1 - n.f) * (tExpected[k] - n.f);
        });

        model.nns.ReverseList().Skip(1).ForEach(ns => ns.ForEach(n =>
        {
            n.delta = n.f * (1 - n.f) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                e.dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.f;
                e.w -= e.dw;
            });
        }));

        var output = model.output.Select(n => n.f).ToArray();

        var err = 0.5f * model.output.Select((n, i) => (tExpected[i] - n.f).Pow2()).Sum();
        model.error = err;
        model.speed = model.ns.Average(n=>n.delta.Abs());

        if (prev != null)
            model.trainDeviation = output.Select((v, i) => (prev[i] - v).Pow2()).Sum();

        prev = output;

        return err;
    }
}