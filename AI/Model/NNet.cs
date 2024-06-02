using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
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
    private N[][] nns;
    private N[] input => nns[0];
    private N[] output => nns[^1];

    private IEnumerable<E> es => nns.SelectMany(ns => ns.SelectMany(n => n.es));

    public NNet(NOptions options)
    {
        this.options = options;
        this.alfa = options.Alfa;
        this.nu = options.Nu;
    }

    public float[] Predict(float[] vInput)
    {
        SetInput(vInput);
        ComputeOutputs();

        return output.Select(n => n.x).ToArray();
    }

    public void ShowDebug()
    {
        Debug.WriteLine($"===");

        nns.ForEach(ns =>
        {
            Debug.WriteLine(ns.Select(n => n.es.Any() ? $"{n}: ({n.es.SJoin(", ")})" : $"{n}").SJoin(", "));
        });
    }

    public void Init()
    {
        rnd = new Random(options.Seed);
        var getBaseWeightFn = NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);

        NGroup CreateGroup(int i) => new NGroup()
        {
        };

        groups = [CreateGroup(0)];

        N CreateN() => new N()
        {
            //dampingFn = NFuncs.GetDampingFn(1 - options.DampingCoeff),
            activatorFn = NFuncs.GetSigmoidFn(),
            errorFn = NFuncs.GetSigmoidDerFn(),
            g = groups[0]
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
        nns = new N[][][]
        {
            [(options.NInput).Range(_=>CreateN()).ToArray()],
            (options.NHidden.nLayers).Range(_ => (options.NHidden.n).Range().Where(IsFilled).Select(_=>CreateN()).ToArray()).ToArray(),
            [(options.NOutput).Range(_=>CreateN()).ToArray()],
        }.ToSingleArray();

        bool IsOutputLayer(int nLayer) => nLayer == nns.Length - 2;
        bool IsLinked(N n, int nLayer) => IsOutputLayer(nLayer) || rnd.NextSingle() < options.LinkFactor;

        nns.SelectPair((aL, bL) => (aL, bL)).ForEach((p, k) =>
        {
            p.aL.ForEach(a =>
            {
                a.es = p.bL.Where(b => IsLinked(b, k)).Select(b => CreateE(a, b)).ToArray();
            });

            //p.bL.ForEach(b =>
            //{
            //    b.back = p.aL.SelectMany(n => n.es.Where(e => e.b == b)).ToArray();
            //});
        });
    }

    private void SetInput(float[] vInput)
    {
        if (vInput.Length != options.NInput)
            throw new InvalidInputDataException();

        (options.NInput).Range().ForEach(i => input[i].x = vInput[i]);
    }

    // todo: Shuffle(sequence); // visit each training data in random order
    public float Train()
    {
        var data = options.Training.ToArray();

        if (options.Shaffle > 0)
            data.Shaffle((int)(options.Shaffle * (3 * data.Length + 7)), rnd);

        if (options.CleanupPrevTrain)
            es.ForEach(e => e.dw = 0);

        var avgErr = data.Select(t => TrainCase(t.input, t.expected)).Average();

        return avgErr;
    }

    private float TrainCase(float[] tInput, float[] tExpected)
    {
        if (tExpected.Length != options.NOutput)
            throw new InvalidOutputTrainingDataException();

        SetInput(tInput);
        ComputeOutputs();

        output.ForEach((n, k) =>
        {
            n.delta = -n.x * (1 - n.x) * (tExpected[k] - n.x);
        });

        nns.Reverse().Skip(1).ForEach(ns => ns.ForEach(n =>
        {
            n.delta = n.x * (1 - n.x) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                e.dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.x;
                e.w -= e.dw;
            });
        }));

        return 0.5f * output.Select((n, i) => (tExpected[i] - n.x).Pow2()).Sum();
    }

    private void ComputeOutputs()
    {
        foreach (var (ns, lv) in nns.Select((ns, lv) => (ns, lv)))
        {
            if (lv > 0) // skip input
                foreach (var n in ns)
                {
                    // apply signals activator
                    n.x = n.activatorFn(n.xx);
                    //n.y = n.dampingFn(n.y);
                }

            // pass signals from a to b
            es.ForEach(e => e.b.xx += e.w * e.a.x);

            // cleanup
            ns.ForEach(n => n.xx = 0);
        }
    }
}