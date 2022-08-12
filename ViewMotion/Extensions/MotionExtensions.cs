using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Model;
using Model3D.Extensions;
using ViewMotion.Models;

namespace ViewMotion.Extensions;

static class MotionExtensions
{
    public static async Task<Motion> ToMotion(this IEnumerable<Shape> shapes, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        startShape ??= Shape.Empty;
        stepDelay ??= TimeSpan.FromMilliseconds(1);

        var queue = new ConcurrentQueue<(int, TaskCompletionSource)>();
        Shape shape = null;

        var th = new Thread(async () =>
        {
            var motion = shapes.GetEnumerator();

            void Step(int num)
            {
                if (!motion.MoveNext())
                    return;

                var s = motion.Current;
                if (shape == null)
                {
                    shape = s;
                }
                else
                {
                    shape.Points = s.Points;
                    shape.Convexes = s.Convexes;
                    shape.Materials = s.Materials;
                }
            }

            while (true)
            {
                await Task.Delay(stepDelay.Value);

                if (queue.TryDequeue(out (int num, TaskCompletionSource t) v))
                {
                    Step(v.num);
                    v.t.SetResult();
                }
            }
        });

        th.Start();

        async Task Step(int num, Action<Shape> update)
        {
            var t = new TaskCompletionSource();
            queue.Enqueue((num, t));
            await t.Task;

            update?.Invoke(shape);
        }

        shape = startShape;
        //await Step(0, null);

        return new Motion
        {
            Shape = new[]
            {
                shape
                //Shapes.CoodsWithText
            }.ToSingleShape(),
            Step = Step
        };
    }
}