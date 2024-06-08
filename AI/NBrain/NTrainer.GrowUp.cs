using AI.Exceptions;
using AI.Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NTrainer
{
    public bool GrowUp()
    {
        if (!options.AllowGrowing)
            throw new NotAllowedException(nameof(options.AllowGrowing));

        if (model.nns[1].Count > options.UpTopology[1] || model.nns.Count > options.UpTopology.Length)
            throw new NotSupportedException("grow up only");

        return options.UpGraph is [] ? GrowUpByTopology() : GrowUpByGraph();
    }

    private bool GrowUpByGraph()
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
        // добавили лишнее ребро
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

            if (cs.Length < model.output.Count)
                return model.output.First(n => cs.All(c => c.i != n.i)).i;

            return FindDifJ(graph[lv - 1].Where(v => v.i == i).Select(v => v.j).ToHashSet(), cs);
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
            var i = graph[lv - 1].First(e => !upGraph[lv - 1].Contains(e)).j;
            var j = upGraph[lv - 1].First(e => !graph[lv - 1].Contains(e)).j;

            return (i, j);
        }

        int[] GetReverses()
        {
            var aa = graph[lv - 1].Select(v => v.j).Distinct().OrderBy(j => j).ToList();
            var lvCount = GetLvCount(graph, lv);
            var r = (lvCount).Range().ToArray();
            var graphLv = graph[lv - 1];

            while (!(graph[lv - 1], upGraph[lv - 1]).SelectBoth().All(v => v.a == v.b))
            {
                var (i, j) = FindRv();

                graphLv.Index().Where(k => graphLv[k].j == i).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, -1));
                graphLv.Index().Where(k => graphLv[k].j == j).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, i));
                graphLv.Index().Where(k => graphLv[k].j == -1).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, j));
                graphLv = graphLv.OrderBy(v => v).ToArray();
                graph[lv - 1] = graphLv;

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

    private bool GrowUpByTopology()
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

        while (lv < options.UpTopology.Length - 1)
        {
            if (nns[lv].Count < options.UpTopology[lv])
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

            if (lv < options.UpTopology.Length - 1)
                model.LevelUp();
        }

        return false;
    }
}
