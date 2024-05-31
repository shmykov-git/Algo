using AI.Libraries;
using Model.Extensions;

namespace AI.Model;

public class NOptions
{
    public int Seed { get; set; } = 0;
    public int N { get; set; } = 10;
    public int DeepLevel { get; set; } = 10;
    public float Threshold { get; set; } = 0.5f;
    public float DampingCoeff { get; set; } = -0.0001f;
    public int MaxGroupCount { get; set; } = 10;
    public float MaxWeight { get; set; } = 1;

    public (float[] input, float[] expected)[] Training { get; set; }
}

public class NGroup
{
    //public float threshold;
}

public class N
{
    public E[] es = [];
    public NGroup g;
    public float x;
    public float y;
    public NFunc activatorFn;
    public NFunc dampingFn;
}

public class E
{
    public float dw;
    public float w;
    public N n;
}


public class NStore
{
    private readonly NOptions options;
    private Random rnd;
    private NGroup[] groups;
    private N[][] nns;
    private N[] input => nns[0];
    private N[] output => nns[^1];
    private IEnumerable<E> es => nns.SelectMany(ns => ns.SelectMany(n => n.es));

    public NStore(NOptions options) => this.options = options;

    public void Init()
    {
        rnd = new Random(options.Seed);

        NGroup CreateGroup(int i) => new NGroup()
        {
        };

        groups = [CreateGroup(0)];

        N CreateN(int i) => new N()
        {
            dampingFn = NFuncs.GetDampingFn(options.DampingCoeff),
            activatorFn = NFuncs.GetSigmoidFn(),
            g = groups[0]
        };

        E CreateE(N n) => new E
        {
            w = options.MaxWeight * (2 * rnd.NextSingle() - 1),
            n = n
        };

        nns = (options.DeepLevel + 1).Range(l => (options.N).Range(CreateN).ToArray()).ToArray();
        nns.SelectPair().ForEach(p=>p.a.ForEach(a => a.es = p.b.Select(CreateE).ToArray()));
    }

    // Calculate - change x
    // Learn - change w


    //float[] GetErr(float[] tExpected, float[] output) => (options.N).Range(i => (tExpected[i] - output[i]) * output[i] * (1 - output[i])).ToArray();

    public void Calculate()
    {
        var errFn = NFuncs.GetErrorFn();
        var n = options.N;
        es.ForEach(e => e.dw = 0);

        foreach(var (tInput, tExpected) in options.Training)
        {
            if (tInput.Length != n || tExpected.Length != n)
                throw new Exception("invalid training data");

            (n).Range().ForEach(i => input[i].y = tInput[i]);
            CalculateCase();

            var err = (options.N).Range(i => errFn(tExpected[i], output[i].y)).ToArray();
            // err <- output, tExpected
            // непонятно как менять все веса уровней lv x n x n
            nns.ForEach((ns, lv) => (n).ForEach(i => ns[i].es.ForEach((e, j) => { e.dw += 0; })));
        }
    }

    public void CalculateCase()
    {
        // square brains
        foreach (var ns in nns)
        {
            // calculate
            foreach (var n in ns)
            {
                n.y = n.activatorFn(n.x);
                n.y = n.dampingFn(n.y);
            }

            // calculate activation
            foreach (var n in ns)
                foreach (var e in n.es)
                    e.n.x += e.w * n.y;

            // next calc cleanup
            foreach (var n in ns)
            {
                n.x = 0;
            }
        }
    }
}