using System.Diagnostics;
using AI.Exceptions;
using Model;
using Model.Extensions;

namespace AI.Model;

public class NModel
{
    public double error;
    public double trainError;
    public (double, double) avgX;
    public double speed;
    public double trainDeviation;

    public List<List<N>> nns;
    private readonly NOptions options;

    public IEnumerable<N> ns => nns.SelectMany(ns => ns);
    public List<N> input => nns[0];
    public List<N> output => nns[^1];

    public IEnumerable<E> es => nns.SelectMany(ns => ns.SelectMany(n => n.es));
    public IEnumerable<E> GetBackEs(N n) => es.Where(e => e.b == n);

    public NModel(NOptions options)
    {
        this.options = options;
    }

    public void RestoreIndices() => ns.ForEach((n, i) => n.i = i);

    public NModel Clone()
    {
        RestoreIndices();

        N CloneN(N n) => new N() 
        { 
            i = n.i,
            sigmoidFn = n.sigmoidFn,
            dampingFn = n.dampingFn,
        };

        var esLinks = es.Select(e => (e, e.a.i, j: e.b.i)).ToArray();
        var newNns = nns.Select(ns => ns.Select(CloneN).ToList()).ToList();
        var newNs = newNns.ToSingleArray();

        E CloneE(E e) => new E() 
        { 
            w = e.w, 
            a = newNs[e.a.i], 
            b = newNs[e.b.i] 
        };

        es.GroupBy(e => e.a.i).ForEach(gv => newNs[gv.Key].es = gv.Select(CloneE).ToList());

        return new NModel(options)
        {
            nns = newNns,
            error = error,
            trainError = trainError,
            avgX = avgX,
            speed = speed
        };
    }

    public Shape2 GetTopology()
    {
        RestoreIndices();

        double maxCount = nns.Max(ns => ns.Count);

        double GetY(int count, int i)
        {
            return maxCount - 0.5 * (maxCount - count) - i * (maxCount / (count + 1));
        }

        double GetX(int lv)
        {
            return lv * maxCount;
        }

        var convexes = es.Select(e => new int[] { e.a.i, e.b.i }).ToArray();
        var ps = nns.SelectMany((ns, lv) => ns.Select((n, i) => new Vector2(GetX(lv), GetY(ns.Count, i)))).ToArray();

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
        Debug.WriteLine($"===");

        nns.ForEach(ns =>
        {
            Debug.WriteLine(ns.Select(n => n.es.Any() ? $"{n}: ({n.es.SJoin(", ")})" : $"{n}").SJoin(", "));
        });
    }
    public void ShowDebugE()
    {
        Debug.WriteLine($"=== avg={avgX} speed={speed} ===");

        nns.ForEach(ns =>
        {
            Debug.WriteLine(ns.Select(n => $"({n.es.SJoin(", ")})").SJoin(", "));
        });
    }

    public void ComputeOutputs(double[] vInput)
    {
        SetInput(vInput);

        // cleanup
        nns.Skip(1).ForEach(ns => ns.ForEach(n => n.xx = 0));

        foreach (var (ns, lv) in nns.Select((ns, lv) => (ns, lv)))
        {
            if (lv > 0) // skip input
                foreach (var n in ns)
                {
                    // apply signals activator
                    n.f = n.sigmoidFn(n.xx * options.PowerFactor);
                    n.f = n.dampingFn(n.f);
                }

            // pass signals from a to b
            es.ForEach(e => e.b.xx += e.w * e.a.f /*/ e.a.es.Length*/);
        }

        var v1 = nns.Skip(1).SelectMany(ns => ns.Select(n => n.f).Where(x => x > 0.5)).Any() ? nns.Skip(1).SelectMany(ns => ns.Select(n => n.f).Where(x => x > 0.5)).Average() : -1;
        var v2 = nns.Skip(1).SelectMany(ns => ns.Select(n => n.f).Where(x => x < 0.5)).Any() ? nns.Skip(1).SelectMany(ns => ns.Select(n => n.f).Where(x => x < 0.5)).Average() : -1;

        avgX = (v1, v2);
        //Debug.Write($"{oneCount*100/count}|");
    }
}
