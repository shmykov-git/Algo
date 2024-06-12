using System.Linq;
using Model.Extensions;
using Model.Graphs;

namespace AI.Extensions;

public static class NGraphExtensions
{
    public static string ToGraphString(this (int i, int j)[][] graph) => 
        $"[{graph.Select(es => $"[{es.Select(e => $"({e.i}, {e.j})").SJoin(", ")}]").SJoin(", ")}]";

    public static string ToStateString(this (int i, int j, double w)[][] state) =>
        $"[{state.Select(es => $"[{es.Select(e => $"({e.i}, {e.j}, {e.w})").SJoin(", ")}]").SJoin(", ")}]";

    public static (int i, int j)[][] ToGraph(this (int i, int j, double w)[][] state) =>
        state.Select(es => es.Select(e => (e.i, e.j)).ToArray()).ToArray();

    public static Dictionary<(int i, int j), double> ToGraphWeights(this (int i, int j, double w)[][] state) =>
        state.SelectMany().ToDictionary(e => (e.i, e.j), e => e.w);

    public static (int i, int j)[][] ToOrdered(this (int i, int j)[][] graph) => graph.Select(l => l.OrderBy(v => v).ToArray()).ToArray();

    public static int[] GetNLevels(this (int i, int j)[][] graph)
    {
        var g = new Graph(graph.SelectMany());
        var distances = graph[0].Select(e=>e.i).Distinct().Select(g.DistanceMap).ToArray();
        var distance = distances[0].Index().Select(i => distances.Min(d => d[i])).ToArray();

        return distance;
    }
}
