using System.Diagnostics;
using AI.Exceptions;
using Model;
using Model.Extensions;

namespace AI.Model;

public class NModel
{
    public float error;
    public float trainError;
    public (float, float) avgX;
    public float speed;
    public float trainDeviation;

    public N[][] nns;
    public IEnumerable<N> ns => nns.SelectMany(ns => ns);
    public N[] input => nns[0];
    public N[] output => nns[^1];

    public IEnumerable<E> es => nns.SelectMany(ns => ns.SelectMany(n => n.es));

    public NModel Clone()
    {
        N CloneN(N n) => new N() { sigmoidFn = n.sigmoidFn };

        var ns = nns.SelectMany(ns => ns).ToList();
        var esLinks = es.Select(e => (e, i: ns.IndexOf(e.a), j: ns.IndexOf(e.b))).ToArray();
        var newNs = ns.Select(CloneN).ToArray();
        var k = 0;
        var newNns = nns.Select(ns => ns.Select(n => newNs[k++]).ToArray()).ToArray();

        E CloneE(E e, int i, int j) => new E() { w = e.w, a = newNs[i], b = newNs[j] };

        esLinks.GroupBy(v => v.i).ForEach(gv => newNs[gv.Key].es = gv.Select(v => CloneE(v.e, v.i, v.j)).ToArray());

        return new NModel
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
        var maxCount = nns.Max(ns => ns.Length);

        double GetY(int count, int i)
        {
            return maxCount - 0.5 * (maxCount - count) - i * (maxCount * 1.0 / (count + 1));
        }

        double GetX(int lv)
        {
            return lv * maxCount;
        }

        var ns = nns.SelectMany(ns => ns).ToList();
        var convexes = es.Select(e => new int[] { ns.IndexOf(e.a), ns.IndexOf(e.b) }).ToArray();
        var ps = nns.SelectMany((ns, lv) => ns.Select((n, i) => new Vector2(GetX(lv), GetY(ns.Length, i)))).ToArray();

        return new Shape2()
        {
            Points = ps,
            Convexes = convexes
        };
    }

    public void SetInput(float[] vInput)
    {
        if (vInput.Length != input.Length)
            throw new InvalidInputDataException();

        (input.Length).Range().ForEach(i => input[i].x = vInput[i]);
    }

    public float[] Predict(float[] vInput)
    {        
        ComputeOutputs(vInput);

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
    public void ShowDebugE()
    {
        Debug.WriteLine($"=== avg={avgX} speed={speed} ===");

        nns.ForEach(ns =>
        {
            Debug.WriteLine(ns.Select(n => $"({n.es.SJoin(", ")})").SJoin(", "));
        });
    }

    public void ComputeOutputs(float[] vInput)
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
                    n.x = n.sigmoidFn(n.xx);
                    //n.y = n.dampingFn(n.y);
                }

            // pass signals from a to b
            es.ForEach(e => e.b.xx += e.w * e.a.x /*/ e.a.es.Length*/);
        }

        var v1 = nns.Skip(1).SelectMany(ns => ns.Select(n => n.x).Where(x => x > 0.5)).Any() ? nns.Skip(1).SelectMany(ns => ns.Select(n => n.x).Where(x => x > 0.5)).Average() : -1;
        var v2 = nns.Skip(1).SelectMany(ns => ns.Select(n => n.x).Where(x => x < 0.5)).Any() ? nns.Skip(1).SelectMany(ns => ns.Select(n => n.x).Where(x => x < 0.5)).Average() : -1;

        avgX = (v1, v2);
        //Debug.Write($"{oneCount*100/count}|");
    }
}
