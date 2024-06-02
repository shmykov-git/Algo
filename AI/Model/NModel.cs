using System.Diagnostics;
using AI.Exceptions;
using Model.Extensions;

namespace AI.Model;

public class NModel
{
    public float error;
    public float trainError;
    public N[][] nns;
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
            es.ForEach(e => e.b.xx += e.w * e.a.x);
        }
    }
}
