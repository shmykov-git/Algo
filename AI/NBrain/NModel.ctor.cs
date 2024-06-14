using System.Diagnostics;
using AI.Libraries;
using AI.Model;
using AI.NBrain.Activators;
using Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel
{
    public double error;
    public double trainError;
    public (double, double) avgX;
    public double avgDelta;
    public int blLv = 0;
    public int upLv = 0;

    private readonly NOptions options;
    private readonly Random rnd;
    public List<List<N>> nns;

    public List<N> input => nns[0];
    public List<N>[] hidden => nns.Skip(1).SkipLast(1).ToArray();
    public List<N> output => nns[^1];

    public IEnumerable<N> unbelievedNs => nns.Skip(blLv).SelectMany(ns => ns.Where(n => !n.believed));
    public int unbelievedCapacity => nns.Skip(blLv).Sum(ns => ns.Count);
    public IEnumerable<N> ns => nns.SelectMany(ns => ns);
    public IEnumerable<E> es => ns.SelectMany(n => n.es);

    public int maxLv => nns.Count - 1;
    public int size => es.Count() + ns.Count();
    public IEnumerable<E> GetBackEs(N n) => es.Where(e => e.b == n);
    public int[] GetNLevels() => ns.Select(n => n.lv).ToArray();

    public NModel(NOptions options, Random rnd)
    {
        this.options = options;
        this.rnd = rnd;
    }

    public void MakeBelieved(int lv)
    {
        nns[lv].ForEach(n =>
        {
            n.believed = true;
        });

        blLv = lv;
    }

    public NModel Clone()
    {
        var newNns = nns.Select(ns => ns.Select(CloneN).ToList()).ToList();
        var newNs = newNns.ToSingleArray();

        es.GroupBy(e => e.a.i).ForEach(gv => newNs[gv.Key].es = gv.Select(e => CloneE(newNs, e)).ToList());

        var model = new NModel(options, rnd)
        {
            nns = newNns,
            error = error,
            trainError = trainError,
            avgX = avgX,
            avgDelta = avgDelta
        };

        model.ns.ForEach(n => n.model = model);

        model.RestoreIndices();
        model.RestoreBackEs();

        return model;
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
        Debug.WriteLine($"=== avgW=({avgX.Item1:F3}, {avgX.Item2:F3}) kDelta={avgDelta * 1000:F3} [lv={nns.Count} n={ns.Count()} e={es.Count()} ({input.Count}->{output.Count})]");
    }
}
