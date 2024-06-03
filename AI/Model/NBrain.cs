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
    private float alfa;
    private float nu;
    
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
        model = new NModel();

        //NGroup CreateGroup(int i) => new NGroup()
        //{
        //};

        //groups = [CreateGroup(0)];

        N CreateN() => new N()
        {
            //dampingFn = NFuncs.GetDampingFn(1 - options.DampingCoeff),
            sigmoidFn = NFuncs.GetSigmoidFn(options.Alfa * options.ScaleFactor),
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
        model.nns = new N[][][]
        {
            [(options.NInput).Range(_=>CreateN()).ToArray()],
            (options.NHidden.nLayers).Range(_ => (options.NHidden.n).Range().Where(IsFilled).Select(_=>CreateN()).ToArray()).ToArray(),
            [(options.NOutput).Range(_=>CreateN()).ToArray()],
        }.ToSingleArray();

        bool IsLinked(N n) => rnd.NextSingle() < options.LinkFactor;

        model.nns.SelectPair((aL, bL) => (aL, bL)).ForEach((p, lv) =>
        {
            p.aL.ForEach(a =>
            {
                a.es = p.bL.Where(IsLinked).Select(b => CreateE(a, b)).ToArray();

                if (a.es.Length == 0)
                    a.es = [CreateE(a, p.bL[rnd.Next(p.bL.Length)])];
            });

            p.bL.Where(b => !p.aL.Any(a => a.es.Any(e => e.b == b))).ToArray()
                .ForEach(b =>
                {
                    var a = p.aL[rnd.Next(p.aL.Length)];
                    var e = CreateE(a, b);
                    a.es = a.es.Concat([e]).ToArray();
                });
        });
    }

    public float Train()
    {
        var data = options.Training.ToArray();

        if (options.Shaffle > 0)
            data.Shaffle((int)(options.Shaffle * (3 * data.Length + 7)), rnd);

        if (options.CleanupPrevTrain)
            model.es.ForEach(e => e.dw = 0);

        var errs = data.Select(t => TrainCase(t.input, t.expected)).ToArray();
        var avgErr = errs.Average();
        model.trainError = avgErr;

        return avgErr;
    }

    float[] prev = null;
    private float TrainCase(float[] tInput, float[] tExpected)
    {
        if (tExpected.Length != options.NOutput)
            throw new InvalidExpectedDataException();

        model.ComputeOutputs(tInput);

        float X(float x)
        {
            //if (model.trainError > 0.04)
            //    return x.Sgn() * (x.Abs() + 0.001f);
            //else
                return x;
        }

        model.output.ForEach((n, k) =>
        {
            n.delta = -X(n.x) * X(1 - n.x) * (tExpected[k] - n.x);
        });

        model.nns.Reverse().Skip(1).ForEach(ns => ns.ForEach(n =>
        {
            n.delta = X(n.x) * X(1 - n.x) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                e.dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.x;
                e.w -= e.dw;
            });
        }));

        var output = model.output.Select(n => n.x).ToArray();

        var err = 0.5f * model.output.Select((n, i) => (tExpected[i] - n.x).Pow2()).Sum();
        model.error = err;
        model.speed = model.ns.Average(n=>n.delta.Abs());

        if (prev != null)
            model.trainDeviation = output.Select((v, i) => (prev[i] - v).Pow2()).Sum();

        prev = output;

        return err;
    }
}