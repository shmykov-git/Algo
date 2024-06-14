using AI.Libraries;
using AI.Model;
using AI.NBrain.Activators;
using Model.Extensions;
using System.Diagnostics;

namespace AI.NBrain;

public partial class NModel // Dynamic
{
    public N CreateN(int lv, NOptions o, NActivatorType t) => new N()
    {
        lv = lv,
        act = t.ToActivator(o),
        //g = groups[0]
    };

    public E CreateE(N a, N b, double w) => new E
    {
        dw = 0,
        w = w,
        a = a,
        b = b,
    };

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

        var c = CreateN(lv, options, ns.Count() % 2 == 0 ? NActivatorType.Sigmoid : NActivatorType.Sin);
        nns[lv].Add(c);
        RestoreIndices();
        AddE(a, c, GetAvgW(a));
        AddE(c, b, GetAvgW(b));
    }

    public void AddE(N a, N b) => AddE(a, b, GetAvgW(a, b));
    public void AddE(N a, N b, double w)
    {
        Debug.WriteLine($"+E:{a.i}-{b.i}");

        if (a.IsLinked(b))
            throw new Exception("cannot link twice");

        var e = CreateE(a, b, w);
        a.es.Add(e);
        RestoreBackEs(b);
    }

    public bool TryRemoveE(N a, N b)
    {
        var e = a.GetLink(b);

        if (e == null)
            return false;

        a.es.Remove(e);
        RestoreBackEs(b);

        return true;
    }

    public void RemoveE(N a, N b)
    {
        var e = a.GetLink(b);

        if (e == null)
            throw new ArgumentException("a and b are not linked");

        a.es.Remove(e);
        RestoreBackEs(b);
    }

    public void LevelUp()
    {
        nns.Insert(nns.Count - 1, new List<N>());
        output.ForEach(n => n.lv++);
    }

    public void ReverseLevelNodes(int lv, int[] reverses)
    {
        var a = nns[lv].ToArray();
        nns[lv] = nns[lv].ToArray().ReverseForward(reverses).ToList();
        var b = nns[lv].ToArray();

        RestoreIndices();
    }

    public void RestoreIndices() => ns.ForEach((n, i) => n.i = i);
    public void RestoreBackEs() => ns.ForEach(RestoreBackEs);
    public void RestoreBackEs(N n) => n.backEs = GetBackEs(n).ToArray();

    public double GetAvgW(N n) => n.es.Concat(n.backEs).Average(e => e.w);
    public double GetAvgW(N a, N b) => a.es.Concat(a.backEs).Concat(b.es).Concat(b.backEs).Average(e => e.w);
}
