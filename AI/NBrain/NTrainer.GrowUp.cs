using System.Diagnostics;
using System.Linq;
using AI.Exceptions;
using AI.Extensions;
using AI.Model;
using meta.Extensions;
using Model.Extensions;
using Model.Graphs;

namespace AI.NBrain;

public partial class NTrainer
{
    private (bool ready, bool levelReady) GrowUp()
    {
        if (!options.AllowGrowing)
            throw new NotAllowedException(nameof(options.AllowGrowing));

        var (ready, levelReady) = options.UpGraph is [] ? GrowUpByTopology() : GrowUpByGraph();

        return (ready, levelReady);
    }

    class GraphInfo
    {
        public (int i, int j)[][] g;
        public int[] lvs;
        public int[][] nns;
        public int n;
        public int lvN;
        public int lvMax;
        public HashSet<int> inputs;
        public HashSet<int> outputs;

        public (int i, int count)[][] csI;
        public (int count, int j)[][] csJ;
        public (int count, int j)[][] csJC;

        public IEnumerable<(int i, int j)> GetEs(int i) => g.SelectMany().Where(e => e.i == i);

        public bool Equals(GraphInfo other) => g.Length == other.g.Length &&
                g.Index().All(lv => g[lv].Length == other.g[lv].Length &&
                    g[lv].Index().All(i => g[lv][i] == other.g[lv][i]));

        public bool LevelJEquals(GraphInfo other, int lv) => g[lv - 1].Length == other.g[lv - 1].Length &&
                    g[lv - 1].Index().All(i => g[lv - 1][i] == other.g[lv - 1][i]);
    }

    private GraphInfo GetGraphInfo((int i, int j)[][] graph, int[] lvs)
    {
        var g = graph;
        var n = graph.SelectMany().Select(e => Math.Max(e.i, e.j)).Max() + 1;  
        var lvMax = lvs.Max();
        var lvN = lvMax + 1;
        var nns = (lvN).Range().Select(lv => (n).Range().Where(i => lvs[i] == lv).ToArray()).ToArray();
        var inputs = nns[0].ToHashSet();
        var outputs = nns[lvMax].ToHashSet();

        bool IsLevelLink((int i, int j) e) => lvs[e.j] - lvs[e.i] == 1;

        var csI = (lvN).Range().Select(k =>
            k == lvMax
            ? outputs.Select(i => (i, c: 0)).ToArray()
            : g[k].Where(IsLevelLink).GroupBy(e => e.i).Select(gv => (i: gv.Key, c: gv.Count())).OrderBy(v => (v.i, v.c)).ToArray()
            ).ToArray();

        var csJ = (lvN).Range().Select(k =>
            k == 0
            ? inputs.Select(j => (c: 0, j)).ToArray()
            : g[k-1].Where(IsLevelLink).GroupBy(e => e.j).Select(gv => (c: gv.Count(), j: gv.Key)).OrderBy(v => (v.j, v.c)).ToArray()
            ).ToArray();
        var csJC = csJ.Select(l => l.OrderBy(v => (v.c, v.j)).ToArray()).ToArray();

        return new GraphInfo
        {
            g = graph,
            n = n,
            lvs = lvs,
            nns = nns,
            lvN = lvN,
            lvMax = lvMax,
            inputs = inputs,
            outputs = outputs,
            csI = csI,
            csJ = csJ,
            csJC = csJC,
        };
    }

    private (bool ready, bool levelReady) GrowUpByGraph()
    {
        var graph = GetGraphInfo(model.GetGraph(), model.GetNLevels());
        var upGraph = GetGraphInfo(options.UpGraph.ToOrdered(), options.UpGraph.GetNLevels());
        var ns = model.ns.ToArray();
        var lv = graph.lvMax - 1;

        (bool success, bool add, int i) FindDifI((int i, int count)[] csI, (int i, int count)[] upCsI) =>
            (csI, upCsI).SelectBoth().Where(v => v.a.i == v.b.i && v.a.count != v.b.count).Select(v => (true, v.a.count < v.b.count, v.a.i)).FirstOrDefault();

        (bool success, bool add, int j) FindDifJ(HashSet<int> js, (int count, int j)[] csJ, (int count, int j)[] upCsJ) =>
            (csJ, upCsJ).SelectBoth().Where(v => v.a.count != v.b.count && v.a.j == v.b.j && js.Contains(v.a.j)).Select(v => (true, v.a.count < v.b.count, v.a.j)).FirstOrDefault();

        (bool success, int i) GetLvI(int lv, int upLv)
        {
            var cs = graph.csI[lv];
            var upCs = upGraph.csI[upLv];

            if (cs.Length < upCs.Length)
            {
                var newI = upCs.Select(v => v.i).Where(i => cs.All(v => v.i != i)).First();
                return (true, newI);
            }

            if (!(cs, upCs).SelectBoth().All(v => v.a.i == v.b.i))
                throw new AlgorithmException("incorrect graph level indices");

            var (success, add, i) = FindDifI(cs, upCs);

            if (success && !add)
                throw new NotSupportedException("grow up only");

            return (success, i);
        }

        int GetLvNJ(int i, int lv)
        {
            var csC = graph.csJC[lv];

            if (csC.Length < model.output.Count)
                return model.output.First(n => csC.All(c => c.j != n.i)).i;

            var j = csC.Select(v => v.j).First();

            return j;
        }

        // same counts
        (bool success, int j) GetLvEJ(int i, int lv)
        {
            var onlyJs = upGraph.GetEs(i).Except(graph.GetEs(i)).Select(v => v.j).ToHashSet();

            var cs = graph.csJ[lv];
            var upCs = upGraph.csJ[lv];

            var (success, add, j) = FindDifJ(onlyJs, cs, upCs);

            if (success && !add)
                throw new NotSupportedException("grow up only");

            return (success, j);
        }

        // i совпали
        (bool, N, N) GetLevelPairN(int lv)
        {
            var (success, i) = GetLvI(lv - 1, lv - 1);

            if (!success)
                return (false, null!, null!);

            var j = GetLvNJ(i, lv + 1);

            return (true, ns[i], ns[j]);
        }

        // i, j совпали
        (bool, N, N) GetLevelPairE(int lv)
        {
            var (hasI, i) = GetLvI(lv - 1, lv - 1);

            if (!hasI)
                return (false, null!, null!);

            var (hasJ, j) = GetLvEJ(i, lv);

            if (!hasJ)
                throw new AlgorithmException("i and j always have pair");

            return (true, ns[i], ns[j]);
        }

        // cross level not single output link
        (bool, E?) GetLevelPairRemoveE(int lv, bool mark)
        {
            return (graph.nns[lv - 1], graph.nns[lv + 1]).SelectCross()
                .Select(v => (true, e: ns[v.a].GetLink(ns[v.b])))
                .Where(v => v.e != null && (!mark || !v.e.unwanted))
                .FirstOrDefault();
        }

        #region Reverses
        void ReverseLevel(int lv)
        {
            var csI = graph.csI[lv - 1];
            var upCsI = upGraph.csI[lv - 1];
            var csJ = graph.csJC[lv];
            var upCsJ = upGraph.csJC[lv];

            if (csJ.Select(v => v.count).SJoin("|") != upCsJ.Select(v => v.count).SJoin("|"))
                throw new AlgorithmException("level is not ready to reverse");

            var reverses = GetReverses();
            model.ReverseLevelNodes(lv, reverses);

            var checkGraph = model.GetGraph();

            if (!(checkGraph[lv - 1], graph.g[lv - 1]).SelectBoth().All(v => v.a == v.b))
                throw new AlgorithmException("incorrect level items");

            ns = model.ns.ToArray();
        }

        (int, int) FindRv()
        {
            var i = graph.g[lv - 1].First(e => !upGraph.g[lv - 1].Contains(e)).j;
            var j = upGraph.g[lv - 1].First(e => !graph.g[lv - 1].Contains(e)).j;

            return (i, j);
        }

        int[] GetReverses()
        {
            var aa = graph.g[lv - 1].Select(v => v.j).Distinct().OrderBy(j => j).ToList();
            var lvCount = graph.nns[lv].Length;
            var r = (lvCount).Range().ToArray();
            var graphLv = graph.g[lv - 1];

            var counter = 10000;

            while (!graph.LevelJEquals(upGraph, lv))
            {
                if (counter-- == 0)
                {
                    Debug.WriteLine($"Graph: {graph.g.ToGraphString()}");
                    Debug.WriteLine($"UpGraph: {upGraph.g.ToGraphString()}");

                    throw new AlgorithmException("reverses circle found, incorrect graphs");
                }

                var (i, j) = FindRv();

                graphLv.Index().Where(k => graphLv[k].j == i).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, -1));
                graphLv.Index().Where(k => graphLv[k].j == j).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, i));
                graphLv.Index().Where(k => graphLv[k].j == -1).ToArray().ForEach(k => graphLv[k] = (graphLv[k].i, j));
                graphLv = graphLv.OrderBy(v => v).ToArray();
                graph.g[lv - 1] = graphLv; // todo: check calcs

                var ii = aa.IndexOf(i);
                var jj = aa.IndexOf(j);
                (r[ii], r[jj]) = (r[jj], r[ii]);
            }

            return r;
        }
        #endregion

        while (true)
        {
            model.es.Where(e => e.canBeRemoved)
                .ToArray().ForEach(e => { model.RemoveE(e); state.upLevelChangesCount++; });

            N a, b;

            // fill level nodes
            if (graph.nns[lv].Length < upGraph.nns[lv].Length)
            {                
                (var canAddN, a, b) = GetLevelPairN(lv);

                if (!canAddN)
                    throw new AlgorithmException("cannot find n to add e");

                model.MarkUnwantedE(a, b);
                model.AddN(a, b);
                state.upLevelChangesCount += 3;
                return (false, false);
            }

            // fill level edges
            (var canAdd, a, b) = GetLevelPairE(lv);

            if (canAdd)
            {
                model.AddE(a, b);
                state.upLevelChangesCount++;
                return (false, false);
            }

            // remove cross level output links
            (var canMark, var e) = GetLevelPairRemoveE(lv, true);

            if (canMark)
            {
                model.MarkUnwantedE(e);
                return (false, false);
            }

            (var needRemove, _) = GetLevelPairRemoveE(lv, false);

            // wait for all unwated links removed
            if (needRemove)
                return (false, false);

            // можно удалить это, если делать вставку нодов
            if (lv == 1)
                ReverseLevel(lv);

            // todo: cross level linked

            // if level is ready then return True ones
            if (model.upLv != lv)
            {
                model.upLv = lv;
                return (false, true);
            }

            lv++;
            state.upLevelChangesCount = 0;

            if (lv == upGraph.lvMax)
            {
                if (!graph.Equals(upGraph))
                    throw new AlgorithmException("graphs should be equals");

                break;
            }

            model.LevelUp();
            graph = GetGraphInfo(model.GetGraph(), model.GetNLevels());
            ns = model.ns.ToArray();
        }

        return (true, true);
    }

    private (bool ready, bool levelReady) GrowUpByTopology()
    {
        if (model.nns[1].Count > options.UpTopology[1] || model.nns.Count > options.UpTopology.Length)
            throw new NotSupportedException("grow up only");

        var nns = model.nns;

        int LevelLinkMinCount(int lv) => Math.Max(nns[lv - 1].Count, nns[lv].Count);
        int LevelLinkCount(int lv) => nns[lv - 1].SelectMany(a => nns[lv].Where(b => a.IsLinked(b))).Count();
        int LevelLinkMaxCount(int lv) => nns[lv - 1].Count * nns[lv].Count;
        bool IsLevelLinked(int lv) => GetLinkFactor(lv) * (LevelLinkMaxCount(lv) - LevelLinkMinCount(lv)) < LevelLinkCount(lv) - LevelLinkMinCount(lv);

        (N, N) GetLevelPairN(int lv)
        {
            var i = nns[lv].Count;

            var a = i < nns[lv - 1].Count
                ? nns[lv - 1][i]
                : nns[lv - 1][rnd.Next(nns[lv - 1].Count)];

            var b = i < nns[lv + 1].Count
                ? nns[lv + 1][i]
                : nns[lv + 1][rnd.Next(nns[lv + 1].Count)];

            return (a, b);
        }

        (bool, E?) GetLevelPairRemoveE(int lv, bool mark)
        {
            return (nns[lv - 1], nns[lv + 1]).SelectCross()
                .Select(v => (true, e: v.a.GetLink(v.b)))
                .Where(v => v.e != null && (!mark || !v.e.unwanted))
                .FirstOrDefault();
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
            model.es.Where(e => e.canBeRemoved)
                .ToArray().ForEach(e => { model.RemoveE(e); state.upLevelChangesCount++; });

            if (nns[lv].Count < options.UpTopology[lv])
            {
                var (a, b) = GetLevelPairN(lv);
                
                if (a.IsLinked(b))
                    model.MarkUnwantedE(a, b);

                model.AddN(a, b);
                state.upLevelChangesCount += 3;

                return (false, false);
            }

            if (!IsLevelLinked(lv))
            {
                var (a, b) = GetLevelPairE(lv);
                model.AddE(a, b);
                state.upLevelChangesCount++;

                return (false, false);
            }

            // mark links to remove as unwanted
            var (needMark, e) = GetLevelPairRemoveE(lv, true);

            if (needMark)
            {
                model.MarkUnwantedE(e);
                return (false, false);
            }

            // wait for all unwanted links are removed
            var (needRemove, _) = GetLevelPairRemoveE(lv, false);

            if (needRemove)
                return (false, false);

            // todo: cross level linked

            // if level is ready then return True ones
            if (model.upLv != lv)
            {
                model.upLv = lv;
                return (false, true);
            }

            lv++;
            state.upLevelChangesCount = 0;

            if (lv < options.UpTopology.Length - 1)
                model.LevelUp();
        }

        return (true, true);
    }
}
