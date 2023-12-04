using System.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Meta.Libraries;

public static class Debugs
{
    private static ConcurrentDictionary<string, (decimal p, DateTime t, int count)> maxPercents = new();
    private static ConcurrentDictionary<string, (Action a, DateTime t)> actions = new();

    static Debugs()
    {
        DebugInTimeProcess();
    }

    async static void DebugInTimeProcess()
    {
        while (true)
        {
            var now = DateTime.UtcNow;

            foreach (var action in actions.ToArray()) 
            {
                if (action.Value.t > now)
                    continue;

                action.Value.a();
                actions.TryRemove(action.Key, out var _);
            }

            await Task.Delay(100);
        }
    }

    private static void DoInTime(string key, Action a, DateTime t)
    {
        actions.AddOrUpdate(key, v => (a, t), (k, v) => (a, t));
    }

    public static void MaxPercent(string str, double value, TimeSpan? timeout = null, Func<bool> filterFn = null, double step = 5, bool useDelay = true)
    {
        var now = DateTime.UtcNow;
        var interval = timeout ?? TimeSpan.FromSeconds(1);
        var fFn = filterFn ?? (() => true);

        var percent = (decimal)(Math.Round(100 * Math.Abs(value) / step) * step);

        var (maxPercent, time, count) = maxPercents.TryGetValue(str, out var maxP) ? maxP : (0, now, 1);

        if (time + interval < now)
        {
            count++;
            maxPercent = 0;
            time = now;
        }

        if (percent > maxPercent && fFn())
        {
            maxPercent = percent;
            
            var debugValue = $"{string.Format(str, $"{percent} %")} ({count})";
            
            if (useDelay)
                DoInTime(str, () => Debug.WriteLine(debugValue), now + interval);
            else
                Debug.WriteLine(debugValue);

            maxPercents[str] = (maxPercent, now, count);
        }
    }
}
