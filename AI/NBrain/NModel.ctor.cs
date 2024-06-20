using System.Diagnostics;
using AI.Libraries;
using AI.Model;
using AI.NBrain.Activators;
using Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel
{
    public readonly NOptions options;
    public double error;
    public double trainError;
    public int blLv = 0;
    public int upLv = 0;  // check level up

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

    public void BlowUp()
    {
        es.ForEach(e => e.w *= 0.9 + 0.2 * rnd.NextDouble());
    }

    public void MakeBelieved(int lv)
    {
        nns[lv - 1].ForEach(n =>
        {
            n.believed = true;
        });

        blLv = lv;
    }

    public NModel Clone()
    {
        var model = new NModel(options, rnd)
        {
            error = error,
            trainError = trainError,
            upLv = upLv,
            blLv = blLv
        };

        var newNns = nns.Select(ns => ns.Select(n => CloneN(model, n)).ToList()).ToList();
        model.nns = newNns;
        var newNs = newNns.ToSingleArray();

        es.GroupBy(e => e.a.i).ForEach(gv => newNs[gv.Key].es = gv.Select(e => CloneE(newNs, e)).ToList());

        model.ns.ForEach(n => n.model = model);

        model.RestoreIndices();
        model.RestoreBackEs();

        return model;
    }

    public string TopologyInfo => $"[lv={nns.Count} n={ns.Count()} e={es.Count()} ({input.Count}->{output.Count})]";
}
