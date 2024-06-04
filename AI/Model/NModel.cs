using System.Diagnostics;
using AI.Exceptions;
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

    public List<N> ns = [];
    public N[] input = [];
    public N[] output = [];

    public IEnumerable<E> es => ns.SelectMany(n => n.es);
    public IEnumerable<E> GetBackEs(N n) => es.Where(e => e.b == n);

    public NModel(NOptions options)
    {
        this.options = options;
    }

    public void RestoreIndices() => ns.ForEach((n, i) => n.i = i);
    public void RestoreBackEs() => ns.ForEach(RestoreBackEs);
    public void RestoreBackEs(N n) => n.backEs = GetBackEs(n).ToArray();

    public void RemoveN(N n) 
    {
        n.backEs.ForEach(e => e.a.es.Remove(e));
        ns.Remove(n);
        n.es.ForEach(e => RestoreBackEs(e.b));
    }

    public NModel Clone()
    {
        RestoreIndices();

        N CloneN(N n) => new N() 
        { 
            i = n.i,
            sigmoidFn = n.sigmoidFn,
            dampingFn = n.dampingFn,
        };

        var newNs = ns.Select(CloneN).ToList();

        E CloneE(E e) => new E() 
        { 
            w = e.w, 
            a = newNs[e.a.i], 
            b = newNs[e.b.i] 
        };

        es.GroupBy(e => e.a.i).ForEach(gv => newNs[gv.Key].es = gv.Select(CloneE).ToList());

        var model = new NModel(options)
        {
            ns = newNs,
            input = input.Select(n => newNs[n.i]).ToArray(),
            output = output.Select(n => newNs[n.i]).ToArray(),
            error = error,
            trainError = trainError,
            avgX = avgX,
            avgDelta = avgDelta
        };

        model.RestoreIndices();
        model.RestoreBackEs();

        return model;
    }

    public int[] GetLevels() 
    {
        var g = new Graph(es.Select(e => (e.a.i, e.b.i)));
        var distances = input.Select(n => g.DistanceMap(n.i)).ToArray();
        var lvs = (g.nodes.Count).Range(i => distances.Min(d => d[i])).ToArray();
        
        var max = lvs.Max();
        
        if (output.Any(n => lvs[n.i] != max) || output.Count(n => lvs[n.i] == max) < lvs.Count(l=>l == max))
            output.ForEach(n => lvs[n.i] = max + 1);

        return lvs;
    }

    public Shape2 GetTopology()
    {
        RestoreIndices();

        var lvs = GetLevels();
        int GetLvCount(int lv) => ns.Count(n => lvs[n.i] == lv);

        double maxCount = lvs.Max(GetLvCount);

        double GetY(N n)
        {
            var lv = lvs[n.i];
            var count = GetLvCount(lv);
            var i = ns.Where(nn => lvs[nn.i] == lv).TakeWhile(nn => nn != n).Count();

            return maxCount - 0.5 * (maxCount - count) - i * (maxCount / (count + 1));
        }

        double GetX(N n)
        {
            return lvs[n.i] * maxCount;
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
        if (vInput.Length != input.Length)
            throw new InvalidInputDataException();

        (input.Length).Range().ForEach(i => input[i].f = vInput[i]);
    }

    public double[] Predict(double[] vInput)
    {        
        ComputeOutputs(vInput);

        return output.Select(n => n.f).ToArray();
    }

    public void ShowDebug()
    {
        Debug.WriteLine($"=== avg=({avgX.Item1:F3}, {avgX.Item2:F3}) kDelta={avgDelta * 1000:F3}");

        var lvs = GetLevels();
        lvs.ForEach(lv => ns.Where(n => lvs[n.i] == lv).ForEach(n =>
        {
            Debug.WriteLine($"{lv}| {ns.Select(n => n.es.Any() ? $"{n.f:F5} ({n.es.SJoin(", ")})" : $"{n.f:F5}").SJoin(", ")}");
        }));
    }

    public void ShowDebugE()
    {
        Debug.WriteLine($"=== avg=({avgX.Item1:F3}, {avgX.Item2:F3}) kDelta={avgDelta * 1000:F3}");

        var lvs = GetLevels();
        lvs.ForEach(lv => ns.Where(n => lvs[n.i] == lv).ForEach(n =>
        {
            Debug.WriteLine($"{lv}| {ns.Select(n => $"({n.es.SJoin(", ")})").SJoin(", ")}");
        }));
    }

    private Queue<N> computeQueue = new();

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
                if (!n.isInput)
                {
                    // apply signals activator
                    n.f = n.sigmoidFn(n.xx * options.PowerFactor);
                    n.f = n.dampingFn(n.f);
                }

                n.computed = true;

                // pass signal from a to b
                n.es.ForEach(e => e.b.xx += e.w * n.f);

                // compute b
                n.es.ForEach(e => computeQueue.Enqueue(e.b));
            }
            else
                computeQueue.Enqueue(n);
        }

        double Avg(Func<N, bool> predicate) => ns.Any(predicate) ? ns.Where(predicate).Average(n => n.f) : -1;
        avgX = (Avg(n => !n.isInput && n.f > 0.5), Avg(n => !n.isInput && n.f < 0.5));
    }
}
