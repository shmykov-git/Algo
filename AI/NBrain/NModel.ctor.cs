using System.Diagnostics;
using AI.Libraries;
using AI.Model;
using Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel
{
    public double error;
    public double trainError;
    public (double, double) avgX;
    public double avgDelta;

    private readonly NOptions options;
    private readonly Random rnd;
    public List<List<N>> nns;

    public List<N> input => nns[0];
    public List<N>[] hidden => nns.Skip(1).SkipLast(1).ToArray();
    public List<N> output => nns[^1];

    public IEnumerable<N> ns => nns.SelectMany(ns => ns);
    public IEnumerable<E> es => ns.SelectMany(n => n.es);

    public int maxLv => ns.Max(n => n.lv);
    public int size => es.Count() + ns.Count();
    public IEnumerable<E> GetBackEs(N n) => es.Where(e => e.b == n);
    public int[] GetNLevels() => ns.Select(n => n.lv).ToArray();

    public NModel(NOptions options, Random rnd)
    {
        this.options = options;
        this.rnd = rnd;
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

    private N CloneN(N n) => new N()
    {
        i = n.i,
        f = n.f,
        lv = n.lv,
        delta = n.delta,
        sigmoidFn = n.sigmoidFn,
        dampingFn = n.dampingFn,
    };

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
        Debug.WriteLine($"=== avgW=({avgX.Item1:F3}, {avgX.Item2:F3}) kDelta={avgDelta * 1000:F3} [lv={nns.Count} n={ns.Count()} e={es.Count()} ({input.Count}->{output.Count})]");
    }
}
