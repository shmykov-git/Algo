using MathNet.Numerics;
using Model.Graphs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Model.Extensions
{
    public static class GraphExtensions
    {
        public static Graph.Edge GetEdge(this (Graph.Node a, Graph.Node b) p) => p.a.ToEdge(p.b);

        public static Graph MinimizeConnections(this Graph g, int seed = 0)
        {
            var r = new Random(seed);

            var exclude = new List<Graph.Edge>();

            while (true)
            {
                var edges = g.edges.Where(edge => !exclude.Contains(edge)).ToArray();

                if (edges.Length == 0)
                    break;

                var edge = edges[r.Next(edges.Length)];

                g.RemoveEdge(edge);

                if (g.Visit().Count() != g.nodes.Count)
                {
                    g.AddEdge(edge);
                    exclude.Add(edge);
                }
            }

            return g;
        }


        public static Graph.Node[][] SplitToMinCircles(this Graph graph, Func<Graph.Edge, double> lenFn)
        {
            var main = graph.Clone();

            var res = new List<Graph.Node[]>();

            bool IsEmpty(Graph g) => g.nodes.All(n => n.edges.Count == 0);

            bool IsCorrect(Graph g) => g.nodes.All(n => n.edges.Count != 1) &&
                                       g.nodes.Count(n => n.edges.Count.IsOdd()).IsEven();

            Graph.Node[] ExcludePathGraph(Graph g)
            {
                // тут где-то ошибка !!
                var edge = g.edges.Where(e => e.a.edges.Count == 2 || e.b.edges.Count == 2).OrderBy(lenFn).First();

                g.RemoveEdge(edge);
                var path = g.FindPath(edge.a, edge.b).ToArray();
                g.AddEdge(edge);

                var pathEdges = path.SelectCirclePair().Select(GetEdge).ToArray();

                var removeEdges = new List<Graph.Edge>();


                foreach (var e in pathEdges)
                {
                    // нашли путь, теперь нужно правильно удалить эджи из этого пути. как?..
                    // точки с числом связей == 3 важны - это входы других путей, которые нельзя игнорировать
                    // эджи не могут быть удалены по одному, только удаленная группа сохранит корректность графа
                    // тут непросто, т.к. решение об удалении части пути может зависет от одного дальнего нода в графе (есть нод -можно оставить, нет - нужно удалить, т.к. тут нет пути)

                    // вернуть алгоритм удаления между 3ками - получить список соединений этих 3чек
                    // какие из этих связей можно удалять?

                    // логическое объединение еджей в один путь
                    // находить просто пары в логическом графе и убирать их длинный путь
                    // ну т.е. продолжить идею поиска малых циклов 0, 1, 2... - не нужно обходить весь граф, только перебирать вершины


                    removeEdges.Add(e); // todo: ...
                }

                Debug.WriteLine($"path edges: {path.SelectCirclePair().Select(p => (p.a.i, p.b.i)).SJoin()}");
                Debug.WriteLine($"remove edges: {removeEdges.Select(e => e.e).SJoin()}");

                removeEdges.ForEach(g.RemoveEdge);

                return path;
            }

            main.WriteToDebug();
            Debug.WriteLine($"{(IsCorrect(main) ? "Correct" : "Incorrect")}");

            while (!IsEmpty(main))
            {
                var circle = ExcludePathGraph(main);
                res.Add(circle);

                Debug.WriteLine(string.Join(", ", circle.Select(n => $"{n.i}")));
                main.WriteToDebug();
                Debug.WriteLine($"{(IsCorrect(main) ? "Correct" : "Incorrect")} {main.nodes.Where(n => n.edges.Count == 1).Select(n => n.i).SJoin()}");

                if (!IsCorrect(main))
                    Debugger.Break();
            }

            return res.ToArray();
        }
    }
}
