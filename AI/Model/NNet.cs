using AI.Exceptions;
using AI.Libraries;
using Model.Extensions;

namespace AI.Model;

public class NNet
{
    private readonly NOptions options;
    private Random rnd;
    private NGroup[] groups;
    private float alfa;
    private float nu;
    
    public NModel model;

    public NNet(NOptions options)
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
        model.nns = new N[][][]
        {
            [(options.NInput).Range(_=>CreateN()).ToArray()],
            (options.NHidden.nLayers).Range(_ => (options.NHidden.n).Range().Where(IsFilled).Select(_=>CreateN()).ToArray()).ToArray(),
            [(options.NOutput).Range(_=>CreateN()).ToArray()],
        }.ToSingleArray();

        bool IsOutputLayer(int nLayer) => nLayer == model.nns.Length - 2;
        bool IsLinked(N n, int nLayer) => IsOutputLayer(nLayer) || rnd.NextSingle() < options.LinkFactor;

        model.nns.SelectPair((aL, bL) => (aL, bL)).ForEach((p, k) =>
        {
            p.aL.ForEach(a =>
            {
                a.es = p.bL.Where(b => IsLinked(b, k)).Select(b => CreateE(a, b)).ToArray();
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

        var avgErr = data.Select(t => TrainCase(t.input, t.expected)).Average();
        model.trainError = avgErr;

        return avgErr;
    }

    private float TrainCase(float[] tInput, float[] tExpected)
    {
        if (tExpected.Length != options.NOutput)
            throw new InvalidExpectedDataException();

        model.ComputeOutputs(tInput);

        model.output.ForEach((n, k) =>
        {
            n.delta = -n.x * (1 - n.x) * (tExpected[k] - n.x);
        });

        model.nns.Reverse().Skip(1).ForEach(ns => ns.ForEach(n =>
        {
            n.delta = n.x * (1 - n.x) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                e.dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.x;
                e.w -= e.dw;
            });
        }));

        var err = 0.5f * model.output.Select((n, i) => (tExpected[i] - n.x).Pow2()).Sum();
        model.error = err;

        return err;
    }
}