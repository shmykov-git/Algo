using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AI.Exceptions;
using AI.Libraries;
using MathNet.Numerics.Random;
using Model.Extensions;

namespace AI.Model;

public class NTrainer
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

        if (options.Graph is [])
        {
            CreateNetByTopology(model);
        }
        else
        {
            CreateNetByGraph(model);
        }

        model.RestoreBackEs();
    }

    private void CreateNetByGraph(NModel model)
    {
        var weightFn = options.PowerWeight0.HasValue
            ? NFuncs.GetBaseWeight(options.PowerWeight0.Value.a / options.PowerFactor, options.PowerWeight0.Value.b / options.PowerFactor)
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

        var ns = (n).Range(i => model.CreateN(lvs[i])).ToArray();
        var nns = ns.GroupBy(n => n.lv).Select(gn => gn.ToList()).ToList();
        model.nns = nns;
        model.RestoreIndices();
        ns.Where(n => n.lv != maxLv).ForEach(nI => nI.es = graph[nI.lv].Where(e => nI.i == e.i).Select(e => model.CreateE(nI, ns[e.j], weightFn(rnd.NextDouble()))).ToList());
        model.RestoreBackEs();
    }

    private void CreateNetByTopology(NModel model)
    {
        var weightFn = options.PowerWeight0.HasValue
            ? NFuncs.GetBaseWeight(options.PowerWeight0.Value.a / options.PowerFactor, options.PowerWeight0.Value.b / options.PowerFactor)
            : NFuncs.GetBaseWeight(options.Weight0.a, options.Weight0.b);

        var input = (options.NInput).Range(_ => model.CreateN(0)).ToList();
        var hidden = (options.NHidden.Length).Range(i => (options.NHidden[i]).Range().Select(_ => model.CreateN(i + 1)).ToList()).ToList();
        var output = (options.NOutput).Range(_ => model.CreateN(options.NHidden.Length + 1)).ToList();
        var nns = new[] { [input], hidden, [output] }.ToSingleList();

        model.nns = nns;
        model.RestoreIndices();

        var maxHidden = options.NHidden.Max();

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
                if (!a.IsLinked(b) && rnd.NextDouble() < options.LinkFactor)
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

    public bool GrowUp()
    {
        if (!options.AllowGrowing)
            throw new NotAllowedException(nameof(options.AllowGrowing));

        if (model.nns[1].Count > options.NHiddenUp[0] || model.nns.Count - 2 > options.NHiddenUp.Length)
            throw new NotSupportedException("grow up only");

        return options.UpGraph is [] ? GrowUpByTopology() : GrowUpByGraph();
    }

    public bool GrowUpByGraph()
    {
        var upGraph = options.UpGraph.Select(l => l.OrderBy(v => v).ToArray()).ToArray();
        var ns = model.ns.ToArray();
        var graph = model.GetGraph();
        var lv = model.nns.Count - 2;

        int GetLvCount((int i, int j)[][] graph, int lv) => lv == 0 ? graph[lv].Select(v => v.i).Distinct().Count() : graph[lv - 1].Select(v => v.j).Distinct().Count();

        (int i, int count)[] GetLvCountsI((int i, int j)[][] graph, int lv) => lv == graph.Length
            ? throw new NotImplementedException()
            : graph[lv].GroupBy(e => e.i).Select(gv => (i: gv.Key, c: gv.Count())).OrderBy(v => (v.i, v.c)).ToArray();

        (int i, int count)[] GetLvCountsJ((int i, int j)[][] graph, int lv) => lv == 0
            ? throw new NotImplementedException()
            : graph[lv - 1].GroupBy(e => e.j).Select(gv => (i: gv.Key, c: gv.Count())).OrderBy(v => (v.c, v.i)).ToArray();

        (bool success, bool add, int i) FindDifI((int i, int count)[] cs, (int i, int count)[] upCs) => 
            (cs, upCs).SelectBoth().Where(v => v.a.count != v.b.count && v.a.i == v.b.i).Select(v => (true, v.a.count < v.b.count, v.a.i)).FirstOrDefault();

        int FindDifJ(HashSet<int> except, (int i, int count)[] cs/*, (int i, int count)[] upCs*/) =>
            cs.Where(v => !except.Contains(v.i)).Select(v => v.i).First();
            //(cs, upCs).SelectBoth().Where(v => v.a.count != v.b.count && !except.Contains(v.a.i)).Select(v => (true, v.a.count < v.b.count, v.a.i)).FirstOrDefault();

        (bool success, int i) GetLvI(int lv, int upLv)
        {
            var cs = GetLvCountsI(graph, lv);
            var upCs = GetLvCountsI(upGraph, upLv);

            if (cs.Length != upCs.Length)
                throw new AlgorithmException("incorrect graph level counts");

            var (success, add, i) = FindDifI(cs, upCs);

            if (success && !add)
                throw new NotSupportedException("only grow up");

            return (success, i);
        }

        int GetLvJ(int i, int lv)
        {
            var cs = GetLvCountsJ(graph, lv);

            return FindDifJ(graph[lv-1].Where(v=>v.i == i).Select(v=>v.j).ToHashSet(), cs);
        }

        (bool, N, N) GetLevelPairN(int lv)
        {
            var (success, i) = GetLvI(lv - 1, lv - 1);

            if (!success)
                return (false, null!, null!);

            var j = GetLvJ(i, graph.Length);

            return (true, ns[i], ns[j]);
        }

        (bool, N, N) GetLevelPairE(int lv)
        {
            var (success, i) = GetLvI(lv - 1, lv - 1);

            if (!success)
                return (false, null!, null!);

            var j = GetLvJ(i, lv);

            return (true, ns[i], ns[j]);
        };

        void ReverseLevel(int lv)
        {
            var cs = GetLvCountsJ(graph, lv);
            var upCs = GetLvCountsJ(upGraph, lv);

            if (cs.Select(v => v.count).SJoin("|") != upCs.Select(v => v.count).SJoin("|"))
                throw new AlgorithmException("level is not ready to reverse");

            // todo: reverse 0 level (0 -- 1)

            var reverses = GetReverses();
            model.ReverseLevelNodes(lv, reverses);

            var checkGraph = model.GetGraph();

            if (!(checkGraph[lv - 1], graph[lv - 1]).SelectBoth().All(v => v.a == v.b))
                throw new AlgorithmException("incorrect level items");

            ns = model.ns.ToArray();
        }

        bool AreGraphsSame() => graph.Length == upGraph.Length &&
            (graph.Length).Range().All(AreGraphsSameLine);

        bool AreGraphsSameLine(int i) => graph[i].Length == upGraph[i].Length &&
                (graph[i], upGraph[i]).SelectBoth().All(v => v.a == v.b);

        bool AreGraphsSameLevel(int lv) => lv == 0
            ? throw new NotImplementedException()
            : AreGraphsSameLine(lv - 1);

        (int, int) FindRv()
        {
            var i = (graph[lv-1], upGraph[lv-1]).SelectBoth().First(v => !upGraph[lv-1].Contains(v.a)).a.j;
            var j = (graph[lv-1], upGraph[lv-1]).SelectBoth().First(v => !graph[lv-1].Contains(v.b)).b.j;

            return (i, j);
        }

        int[] GetReverses()
        {
            var aa = graph[lv-1].Select(v => v.j).Distinct().OrderBy(j => j).ToList();
            var r = (9).Range().ToArray();
            var graphLv = graph[lv-1];

            while (!(graph[lv-1], upGraph[lv-1]).SelectBoth().All(v => v.a == v.b))
            {
                var (i, j) = FindRv();

                graphLv.Index().Where(k => graphLv[k].j == i).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, -1));
                graphLv.Index().Where(k => graphLv[k].j == j).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, i));
                graphLv.Index().Where(k => graphLv[k].j == -1).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, j));
                graphLv = graphLv.OrderBy(v => v).ToArray();
                graph[lv-1] = graphLv;

                var ii = aa.IndexOf(i);
                var jj = aa.IndexOf(j);
                (r[ii], r[jj]) = (r[jj], r[ii]);
            }

            return r;
        }

        while (true)
        {
            N a, b;
            var lvCount = GetLvCount(graph, lv);
            var lvUpCount = GetLvCount(upGraph, lv);

            // add level nodes
            if (GetLvCount(graph, lv) < GetLvCount(upGraph, lv))
            {
                (var canAddN, a, b) = GetLevelPairN(lv);

                if (!canAddN)
                    throw new AlgorithmException("cannot find n to add e");
                
                model.TryRemoveE(a, b);
                model.AddN(a, b);

                return true;
            }

            // add level edges
            (var canAdd, a, b) = GetLevelPairE(lv);

            if (canAdd)
            {
                model.AddE(a, b);

                return true;
            }

            // todo не работают, оправить venus потом (1, 4) -> (0, 4)
            ReverseLevel(lv);

            // todo: cross level linked

            if (AreGraphsSame())
                break;

            lv++;

            model.LevelUp();
            graph = model.GetGraph();
        }

        return false;
    }

    public bool GrowUpByTopology()
    {
        var nns = model.nns;

        int LevelLinkMinCount(int lv) => Math.Max(nns[lv - 1].Count, nns[lv].Count);
        int LevelLinkCount(int lv) => nns[lv - 1].SelectMany(a => nns[lv].Where(b => a.IsLinked(b))).Count();
        int LevelLinkMaxCount(int lv) => nns[lv - 1].Count * nns[lv].Count;
        bool IsLevelLinked(int lv) => options.LinkFactor * (LevelLinkMaxCount(lv) - LevelLinkMinCount(lv)) < LevelLinkCount(lv) - LevelLinkMinCount(lv);

        (N, N) GetLevelPairN(int lv)
        {
            var i = nns[lv].Count;

            var a = i < nns[lv - 1].Count
                ? nns[lv - 1][i]
                : nns[lv - 1][rnd.Next(nns[lv - 1].Count)];

            var b = nns[lv + 1][rnd.Next(nns[lv + 1].Count)];

            return (a, b);
        }

        (N, N) GetLevelPairE(int lv) 
        {
            N a, b;
            do
            {
                (a, b) = (nns[lv - 1][rnd.Next(nns[lv - 1].Count)], nns[lv][rnd.Next(nns[lv].Count)]);
            } while (a.IsLinked(b));

            return (a, b);
        };

        var lv = nns.Count - 2;

        while (lv < options.NHiddenUp.Length + 1)
        {
            if (nns[lv].Count < options.NHiddenUp[lv - 1])
            {
                var (a, b) = GetLevelPairN(lv);                
                model.TryRemoveE(a, b);
                model.AddN(a, b);

                return true;
            }

            if (!IsLevelLinked(lv))
            {
                var (a, b) = GetLevelPairE(lv);
                model.AddE(a, b);

                return true;
            }

            // todo: cross level linked

            lv++;

            if (lv < options.NHiddenUp.Length + 1)
                model.LevelUp();
        }

        return false;
    }

    public void CleanupTrainTails() => model.es.ForEach(e => e.dw = 0);

    public double Train()
    {
        var data = options.Training.ToArray();

        if (options.ShaffleFactor > 0)
            data.Shaffle((int)(options.ShaffleFactor * (3 * data.Length + 7)), rnd);

        if (options.CleanupTrainTails)
            CleanupTrainTails();

        var errs = data.Select(t => TrainCase(t.input, t.expected)).ToArray();
        
        var avgErr = errs.Average();
        model.trainError = avgErr;

        return avgErr;
    }

    private Queue<N> learnQueue = new();

    /// <summary>
    /// Метод обратного распространения ошибки
    /// https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%BE%D0%B1%D1%80%D0%B0%D1%82%D0%BD%D0%BE%D0%B3%D0%BE_%D1%80%D0%B0%D1%81%D0%BF%D1%80%D0%BE%D1%81%D1%82%D1%80%D0%B0%D0%BD%D0%B5%D0%BD%D0%B8%D1%8F_%D0%BE%D1%88%D0%B8%D0%B1%D0%BA%D0%B8
    /// </summary>
    private double TrainCase(double[] tInput, double[] tExpected)
    {
        if (tExpected.Length != options.NOutput)
            throw new InvalidExpectedDataException();

        void LearnBackPropagationOutput(N n, int k)
        {
            n.delta = -n.f * (1 - n.f) * (tExpected[k] - n.f);
            n.learned = true;
        }

        void LearnBackPropagation(N n)
        {
            n.delta = n.f * (1 - n.f) * n.es.Sum(e => e.b.delta * e.w);

            n.es.ForEach(e =>
            {
                e.dw = alfa * e.dw + (1 - alfa) * nu * e.b.delta * e.a.f;
                e.w -= e.dw;
            });

            n.learned = true;
        }

        model.ComputeOutputs(tInput);

        // learn cleanup
        model.ns.ForEach(n => { n.learned = false; });

        model.output.ForEach(LearnBackPropagationOutput);
        model.output.SelectMany(n => n.backEs.Select(e => e.a)).Distinct().ForEach(learnQueue.Enqueue);

        while (learnQueue.TryDequeue(out var n))
        {
            if (n.learned)
                continue;

            if (n.es.All(e => e.b.learned))
            {
                LearnBackPropagation(n);
                n.backEs.Select(e => e.a).ForEach(learnQueue.Enqueue);
            }
            else
                learnQueue.Enqueue(n);
        }

        var err = 0.5f * model.output.Select((n, i) => (tExpected[i] - n.f).Pow2()).Sum();
        model.error = err;
        model.avgDelta = model.ns.Average(n=>n.delta.Abs());

        return err;
    }
}