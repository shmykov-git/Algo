using AI.Model;
using Model;
using Model.Extensions;

namespace AI.NBrain;

public partial class NModel // Info
{
    public (int i, int j)[][] GetGraph() => nns.SkipLast(1).Select(ns => ns.SelectMany(n => n.es.Select(e => (e.a.i, e.b.i))).OrderBy(v => v).ToArray()).ToArray();

    public Shape GetTopology()
    {
        var maxLv = ns.Max(n => n.lv);
        double maxCount = ns.Max(n => nns[n.lv].Count);

        double GetY(N n)
        {
            double count = nns[n.lv].Count;
            var i = ns.Where(nn => nn.lv == n.lv).TakeWhile(nn => nn != n).Count();
            var step = maxCount / (count + 1);
            var oddShift = n.lv % 2 == 0 ? step / Math.Sqrt(31) : 0;

            return maxLv * step * (count - i) - oddShift;
        }

        double GetX(N n)
        {
            return n.lv * maxCount;
        }

        var convexes = es.Select(e => new int[] { e.a.i, e.b.i }).ToArray();
        var ps = ns.Select(n => new Vector2(GetX(n), GetY(n))).ToArray();

        return new Shape()
        {
            Points2 = ps,
            Convexes = convexes
        };
    }
}
