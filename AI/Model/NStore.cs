using AI.Libraries;
using Model.Extensions;

namespace AI.Model;

public class NOptions
{
    public int Seed { get; set; } = 0;
    public int N { get; set; } = 10;
    public int DeepLevel { get; set; } = 10;
    //public float Threshold { get; set; } = 0.5f;
    public float FillFactor { get; set; } = 0.3f;
    public float InfluenceFactor { get; set; } = 0.2f;
    public float SpeedFactor { get; set; } = 0.2f;
    public int SpeedUpCount { get; set; } = 2;
    public float DampingCoeff { get; set; } = -0.0001f;
    public int MaxGroupCount { get; set; } = 10;
    public float MaxWeight { get; set; } = 1;

    public (float[] input, float[] expected)[] Training { get; set; }
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
            w0 = options.MaxWeight * (2 * rnd.NextSingle() - 1),
            dw = options.SpeedFactor,
            influence = (options.N).Range().Where(_ => rnd.NextSingle() < options.InfluenceFactor).ToArray(),
            n = n
        };

        nns = (options.DeepLevel + 1).Range(l => (options.N).Range(CreateN).ToArray()).ToArray();
        nns.SelectPair().ForEach(p => p.a.Where(_ => rnd.NextSingle() < options.FillFactor).ForEach(a => a.es = p.b.Select(CreateE).ToArray()));

        es.ForEach(e => e.w = e.w0);
        Calculate(e => { }, e => { });
    }

    // Calculate - change x
    // Learn - change w


    //float[] GetErr(float[] tExpected, float[] output) => (options.N).Range(i => (tExpected[i] - output[i]) * output[i] * (1 - output[i])).ToArray();

    public void Train() => Calculate(
        e => (e.wPrev, e.fPrev, e.w) = (e.w, e.f, e.w + e.dw), 
        e =>
        {
            if (e.fPrev < e.f)
            {
                e.dw = -0.5f * e.dw;
                e.dwCount = 0;
            }
            else
            {
                e.dwCount++;

                if (e.dwCount >= options.SpeedUpCount)
                {
                    e.dw = 2f * e.dw;
                    e.dwCount = 0;
                }
            }
        });

    private void Calculate(Action<E> before, Action<E> after)
    {
        es.ForEach(e => 
        {
            e.f = 0;
            before(e); 
        });

        Calculate();
        es.ForEach(after);
    }

    private void Calculate()
    {
        //var errFn = NFuncs.GetErrorFn();
        var n = options.N;

        foreach(var (tInput, tExpected) in options.Training)
        {
            if (tInput.Length != n || tExpected.Length != n)
                throw new Exception("invalid training data");

            (n).Range().ForEach(i => input[i].y = tInput[i]);
            CalculateCase();

            var err2 = (options.N).SelectRange(i => (tExpected[i] - output[i].y).Pow2()).ToArray();
            es.ForEach(e => e.f += e.influence.Select(i => err2[i]).Sum());
        }
    }

    private void CalculateCase()
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