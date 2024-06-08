using Model.Extensions;

namespace AI.Extensions;

public static class NGraphExtensions
{
    public static string ToGraphString(this (int i, int j)[][] graph) => 
        $"[{graph.Select(es => $"[{es.Select(e => $"({e.i}, {e.j})").SJoin(", ")}]").SJoin(", ")}]";
}
