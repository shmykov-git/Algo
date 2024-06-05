using System.Diagnostics;
using AI.Exceptions;
using AI.Libraries;
using Model;
using Model.Extensions;
using Model.Graphs;

namespace AI.Model;

public class NModel
{
    public double error;
    public double trainError;
    public (double, double) avgX;
    public double avgDelta;

    private readonly NOptions options;
    private readonly Random rnd;
    public List<List<N>> nns;

    public List<N> input => nns[0];
    public List<N> output => nns[^1];

    public IEnumerable<N> ns => nns.SelectMany(ns => ns);
    public IEnumerable<E> es => ns.SelectMany(n => n.es);

    public int maxLv => ns.Max(n => n.lv);
    public int size => es.Count() + ns.Count();
    public IEnumerable<E> GetBackEs(N n) => es.Where(e => e.b == n);

    public NModel(NOptions options, Random rnd)
    {
        this.options = options;
        this.rnd = rnd;
    }

    public N CreateN(int lv) => new N()
    {
        lv = lv,
        dampingFn = NFuncs.GetDampingFn(options.DampingCoeff),
        sigmoidFn = NFuncs.GetSigmoidFn(options.Alfa),
        //g = groups[0]
    };

    public E CreateE(N a, N b, double w) => new E
    {
        dw = 0,
        w = w,
        a = a,
        b = b,
    };

    public void RestoreIndices() => ns.ForEach((n, i) => n.i = i);
    public void RestoreBackEs() => ns.ForEach(RestoreBackEs);
    public void RestoreBackEs(N n) => n.backEs = GetBackEs(n).ToArray();

    public void RemoveN(N n) 
    {
        n.backEs.ForEach(e => e.a.es.Remove(e));
        nns[n.lv].Remove(n);
        n.es.ForEach(e => RestoreBackEs(e.b));
        RestoreIndices();
    }

    public void AddN(N a, N b)
    {
        var lv = (a.lv + b.lv) / 2;
        Debug.WriteLine($"+N:{a.i}-{b.i} ({lv})");

        var c = CreateN(lv);
        nns[lv].Add(c);
        RestoreIndices();
        AddE(a, c);
        AddE(c, b);
    }

    public void AddE(N a, N b)
    {
        Debug.WriteLine($"+E:{a.i}-{b.i}");

        var e = CreateE(a, b, GetW());
        a.es.Add(e);
        RestoreBackEs(b);
    }

    private N CloneN(N n) => new N()
    {
        i = n.i,   
        f = n.f,
        delta = n.delta,
        sigmoidFn = n.sigmoidFn,
        dampingFn = n.dampingFn,
    };

    private double GetW()
    {
        //var w = -Math.Log((1 - a.avgF) / a.avgF) / (2 * options.Alfa * a.avgF * options.PowerFactor);
        var getBaseWeightFn = NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);

        return getBaseWeightFn(rnd.NextDouble());

        //var ws = a.es.Concat(a.backEs).Concat(b.es).Concat(b.backEs).Select(e => e.w).ToArray();
        //var countPlus = ws.Count(w => w > 0);
        //var countMinus = ws.Count(w => w <= 0);

        //return countPlus > countMinus
        //    ? ws.Where(w => w > 0).Average()
        //    : ws.Where(w => w <= 0).Average();
    }

    public NModel Clone()
    {
        var newNns = nns.Select(ns => ns.Select(CloneN).ToList()).ToList();
        var newNs = newNns.ToSingleArray();

        E CloneE(E e) => new E() 
        { 
            w = e.w, 
            a = newNs[e.a.i], 
            b = newNs[e.b.i] 
        };

        es.GroupBy(e => e.a.i).ForEach(gv => newNs[gv.Key].es = gv.Select(CloneE).ToList());

        var model = new NModel(options, rnd)
        {
            nns = newNns,
            error = error,
            trainError = trainError,
            avgX = avgX,
            avgDelta = avgDelta
        };

        model.RestoreIndices();
        model.RestoreBackEs();

        return model;
    }

    public Shape2 GetTopology()
    {
        var maxLv = ns.Max(n=>n.lv);
        double maxCount = ns.Max(n => nns[n.lv].Count);

        double GetY(N n)
        {
            double count = nns[n.lv].Count;
            var i = ns.Where(nn => nn.lv == n.lv).TakeWhile(nn => nn != n).Count();
            var step = maxCount / (count + 1);
            var oddShift = n.lv % 2 == 0 ? step / 5 : 0;

            return maxLv * step * (count - i) - oddShift;
        }

        double GetX(N n)
        {
            return n.lv * maxCount;
        }

        var convexes = es.Select(e => new int[] { e.a.i, e.b.i }).ToArray();
        var ps = ns.Select(n => new Vector2(GetX(n), GetY(n))).ToArray();

        return new Shape2()
        {
            Points = ps,
            Convexes = convexes
        };
    }

    public void SetInput(double[] vInput)
    {
        if (vInput.Length != input.Count)
            throw new InvalidInputDataException();

        (input.Count).Range().ForEach(i => input[i].f = vInput[i]);
    }

    public double[] Predict(double[] vInput)
    {        
        ComputeOutputs(vInput);

        return output.Select(n => n.f).ToArray();
    }

    public void ShowDebug()
    {
        ShowDebugInfo();

        nns.ForEach(lv => ns.ForEach(n =>
        {
            Debug.WriteLine($"{lv}| {ns.Select(n => n.es.Any() ? $"{n.f:F5} ({n.es.SJoin(", ")})" : $"{n.f:F5}").SJoin(", ")}");
        }));
    }

    public void ShowDebugE()
    {
        ShowDebugInfo();

        nns.ForEach(lv => ns.ForEach(n =>
        {
            Debug.WriteLine($"{lv}| {ns.Select(n => $"({n.es.SJoin(", ")})").SJoin(", ")}");
        }));
    }

    public void ShowDebugInfo()
    {
        Debug.WriteLine($"=== avgW=({avgX.Item1:F3}, {avgX.Item2:F3}) kDelta={avgDelta * 1000:F3} [n={ns.Count()} e={es.Count()} ({input.Count}->{output.Count})]");
    }

    private Queue<N> computeQueue = new();

    private void ComputeN(N n)
    {
        if (!n.isInput)
        {
            // compute output (f) from input (xx)
            n.f = n.sigmoidFn(n.xx * options.PowerFactor);
            n.f = n.dampingFn(n.f);
            n.avgF += n.f;
        }

        // pass signal from output a to input b
        n.es.ForEach(e => e.b.xx += e.w * n.f);

        n.computed = true;
    }

    public void ComputeOutputs(double[] vInput)
    {
        SetInput(vInput);

        // compute cleanup
        ns.ForEach(n => { n.xx = 0; n.computed = false; });

        input.ForEach(computeQueue.Enqueue);

        while (computeQueue.TryDequeue(out var n))
        {
            if (n.computed)
                continue;

            if (n.isInput || n.backEs.All(e => e.a.computed))
            {
                ComputeN(n);

                // pass to compute b
                n.es.ForEach(e => computeQueue.Enqueue(e.b));
            }
            else
                computeQueue.Enqueue(n);
        }

        double Avg(Func<N, bool> predicate) => ns.Any(predicate) ? ns.Where(predicate).Average(n => n.f) : -1;
        avgX = (Avg(n => !n.isInput && n.f > 0.5), Avg(n => !n.isInput && n.f < 0.5));
    }
}
