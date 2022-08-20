using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Meta.Extensions;
using Model;
using ViewMotion.Models;

namespace ViewMotion.Extensions;

static class MotionExtensions
{
    public static async Task<Motion> ToMotion(this IEnumerable<Shape> shapes, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        stepDelay ??= TimeSpan.FromMilliseconds(1);

        var queue = new ConcurrentQueue<(int, TaskCompletionSource)>();
        Shape? shape = null;

        var th = new Thread(async () =>
        {
            var motion = shapes.GetEnumerator();

            bool CalculateNextShape()
            {
                if (!motion.MoveNext())
                {
                    shape = null;

                    return false;
                }

                var s = motion.Current;
                
                shape = new Shape()
                {
                    Points = s.Points.ToArray(),
                    Convexes = s.Convexes.ToArray(),
                    Materials = s.Materials?.ToArray(),
                    TexturePoints = s.TexturePoints?.ToArray(),
                };

                return true;
            }

            while (true)
            {
                await Task.Delay(stepDelay.Value);

                if (queue.TryDequeue(out (int num, TaskCompletionSource t) v))
                {
                    var hasNewShape = CalculateNextShape();
                    v.t.SetResult();

                    if (!hasNewShape)
                        break;
                }
            }
        }) {IsBackground = true};

        th.Start();

        async Task<bool> Step(int num, Action<Shape> update)
        {
            var t = new TaskCompletionSource();
            queue.Enqueue((num, t));
            await t.Task;

            if (shape != null)
            {
                update?.Invoke(shape);

                return true;
            }
            else
            {
                return false;
            }
        }

        shape = startShape;

        return new Motion
        {
            Shape = shape,
            Step = Step
        };
    }
}