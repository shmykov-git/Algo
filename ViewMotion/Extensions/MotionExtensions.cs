using System;
using System.Collections;
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
    public static Task<Motion> ToMotion(this Shape shape, MotionOptions options)
    {
        IEnumerable<Shape> Animate()
        {
            yield return shape;
        }

        return Animate().ToMotion(options);
    }
    public static Task<Motion> ToMotion(this Shape shape, double? cameraDistance = null, Shape? startShape = null,
        TimeSpan? stepDelay = null)
    {
        IEnumerable<Shape> Animate()
        {
            yield return shape;
        }

        return Animate().ToMotion(cameraDistance, startShape);
    }

    public static Task<Motion> ToMotion(this IEnumerable<Shape> shapes, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        return ToMotion(shapes, new MotionOptions()
        {
            CameraDistance = cameraDistance,
            StartShape = startShape,
            StepDelay = stepDelay
        });
    }

    public static async Task<Motion> ToMotion(this IEnumerable<Shape> shapes, MotionOptions options)
    {
        var stepDelay = options.StepDelay ?? TimeSpan.FromMilliseconds(1);

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
                await Task.Delay(stepDelay);

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

        shape = options.StartShape;

        return new Motion
        {
            CameraMotionOptions= options.CameraMotionOptions,
            CameraDistance = options.CameraDistance,
            Shape = shape,
            Step = Step
        };
    }
}