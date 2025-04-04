﻿using AI.Extensions;
using AI.Libraries;
using AI.Model;
using AI.NBrain.Activators;
using Model.Extensions;

namespace AI.NBrain;

public partial class NTrainer
{
    private readonly NOptions options;
    private Random rnd;
    private NGroup[] groups;
    private double alfa;
    private double nu;
    
    public NModel model;

    public NTrainer(NOptions options)
    {
        this.options = options;
        this.alfa = options.Alfa;
        this.nu = options.Nu;
    }

    public void Init()
    {
        rnd = new Random(options.Seed);
        model = new NModel(options, rnd);

        if (options.Model is [])
        {
            if (options.Graph is [])
            {
                CreateNetByTopology(model);
            }
            else
            {
                CreateNetByGraph(model);
            }
        }
        else
        {
            options.Graph = options.Model.ToGraph();
            CreateNetByGraph(model);
            var weights = options.Model.ToGraphWeights();
            model.es.ForEach(e => e.w = weights[(e.a.i, e.b.i)]);
        }

        model.RestoreBackEs();
    }

    private void CreateNetByGraph(NModel model)
    {
        var weightFn = options.PowerWeight0.HasValue
            ? NFuncs.GetBaseWeight(options.PowerWeight0.Value.a / options.Power, options.PowerWeight0.Value.b / options.Power)
            : NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);

        var graph = options.Graph;
        var gns = graph.SelectMany(l=>l.SelectMany(e=>new[] { e.i, e.j })).Distinct().ToArray();
        var n = gns.Max() + 1;

        if (gns.Length != n)
            throw new ArgumentException("invalid graph");

        // todo: cross graphs

        var lvs = graph.SelectMany((l, lv) => l.Select(e => e.i).Distinct().Select(i => (i, lv))).
            Concat(graph[^1].Select(e => e.j).Distinct().Select(i => (i, lv: graph.Length)))
            .OrderBy(v => v.i)
            .Select(v => v.lv)
            .ToArray();
        var maxLv = lvs.Max();

        var ns = (n).Range(i => model.CreateN(model, lvs[i])).ToArray();
        var nns = ns.GroupBy(n => n.lv).Select(gn => gn.ToList()).ToList();
        model.nns = nns;
        model.RestoreIndices();
        ns.Where(n => n.lv != maxLv).ForEach(nI => nI.es = graph[nI.lv].Where(e => nI.i == e.i).Select(e => model.CreateE(nI, ns[e.j], weightFn(rnd.NextDouble()))).ToList());
        model.RestoreBackEs();
    }

    private double GetLinkFactor(int lv) => options.LayerLinkFactors.Length > 0 ? options.LayerLinkFactors[lv] : options.LinkFactor;

    private void CreateNetByTopology(NModel model)
    {
        var weightFn = options.PowerWeight0.HasValue
            ? NFuncs.GetBaseWeight(options.PowerWeight0.Value.a / options.Power, options.PowerWeight0.Value.b / options.Power)
            : NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);

        var k = 0;
        var nns = (options.Topology.Length).Range(lv => (options.Topology[lv]).Range().Select(_ => model.CreateN(model, lv)).ToList()).ToList();
        model.nns = nns;
        model.RestoreIndices();
        //nns.ForEach((n, lv, ii) => 
        //{
        //    if (ii % 2 == 1)
        //        n.act = NAct.Sin.ToActivator(options);
        //});

        var maxHidden = options.Topology.Max();

        nns.SelectPair((aL, bL) => (aL, bL)).ForEach(pair =>
        {
            var (aL, bL) = pair;

            (Math.Max(aL.Count, bL.Count)).ForEach(i =>
            {
                var a = aL[i % aL.Count];
                var b = bL[i % bL.Count];
                a.es.Add(model.CreateE(a, b, weightFn(rnd.NextDouble())));
            });

            aL.ForEach(a => bL.ForEach(b =>
            {
                if (!a.IsLinked(b) && rnd.NextDouble() < GetLinkFactor(a.lv))
                    a.es.Add(model.CreateE(a, b, weightFn(rnd.NextDouble())));
            }));
        });

        nns.SkipLast(2).ForEach((aL, i) => nns.Skip(i + 2).SkipLast(1).ForEach(bL =>
        {
            aL.ForEach(a => bL.ForEach(b =>
            {
                if (!a.IsLinked(b) && rnd.NextDouble() < options.CrossLinkFactor)
                    a.es.Add(model.CreateE(a, b, weightFn(rnd.NextDouble())));
            }));
        }));
    }

    public void CleanupTrainTails() => model.es.ForEach(e => e.dw = 0);

}