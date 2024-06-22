using AI.Extensions;
using AI.Libraries;
using AI.Model;
using Model.Extensions;
using System.Diagnostics;

namespace AI.NBrain;

public partial class NModel // Dynamic
{
    public N CreateN(NModel model, int lv) => new N()
    {
        model = model,
        lv = lv,
        act = model.options.ToActivator(model, lv),
    };

    private N CloneN(NModel model, N n) => new N()
    {
        model = model,
        i = n.i,
        lv = n.lv,
        act = model.options.ToActivator(model, n.lv),
    };

    public E CreateE(N a, N b, double w) => new E
    {
        dw = 0,
        w = w,
        a = a,
        b = b,
    };

    E CloneE(N[] ns, E e) => new E()
    {
        w = e.w,
        dw = e.dw,
        a = ns[e.a.i],
        b = ns[e.b.i]
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

        var c = CreateN(this, lv);
        nns[lv].Add(c);
        RestoreIndices();
        AddE(a, c, GetDynamicW0(a));
        AddE(c, b, GetDynamicW0(b));
    }

    public void AddE(N a, N b) => AddE(a, b, GetDynamicW0(a, b));
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

        RemoveE(e);
    }

    public void RemoveE(E e)
    {
        e.a.es.Remove(e);
        RestoreBackEs(e.b);
    }

    public void MarkUnwantedE(N a, N b)
    {
        var e = a.GetLink(b);

        if (e == null)
            throw new ArgumentException("a and b are not linked");

        MarkUnwantedE(e);
    }

    public void MarkUnwantedE(E e)
    {
        if (e.unwanted)
            return;

        e.unwanted = true;
        e.uW0 = e.w;
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

    public void RestoreIndices()
    {
        ns.ForEach((n, i) => n.i = i);
        nns.ForEach((n, _, ii) => n.ii = ii);
    }

    public void RestoreBackEs() => ns.ForEach(RestoreBackEs);
    public void RestoreBackEs(N n) => n.backEs = GetBackEs(n).ToArray();

    public double GetDynamicW0(N n) => options.DynamicW0Factor * n.es.Concat(n.backEs).Average(e => e.w);
    public double GetDynamicW0(N a, N b) => options.DynamicW0Factor * a.es.Concat(a.backEs).Concat(b.es).Concat(b.backEs).Average(e => e.w);
}
