using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Meta.Extensions;
using Model;
using Model.Extensions;
using Model.Libraries;
using Model3D.Actives;
using Model3D.Extensions;
using ViewMotion.Models;

namespace ViewMotion.Extensions;

static class MotionExtensions
{
    public static Task<Motion> ToMotion(this ActiveWorld world, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null) =>
        world.Animate().ToMotion(new MotionOptions()
        {
            CameraDistance = cameraDistance,
            StartShape = startShape,
            StepDelay = stepDelay
        });

    public static Task<Motion> ToMotion(this ActiveWorld world, MotionOptions options) => world.Animate().ToMotion(options);

    public static Task<Motion> ToMotion(this Shape shape, MotionOptions options)
    {
        IEnumerable<Shape> Animate()
        {
            yield return shape;
        }

        return Animate().ToMotion(options);
    }

    public static Task<Motion> ToMotion(this Shape shape, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null) =>
        new[] { shape }.ToMotion(cameraDistance, startShape, stepDelay);

    public static Task<Motion> ToMotion2D(this Shape shape, double cameraDistance) => new[] { shape }.ToMotion(new Vector3(0, 0, cameraDistance));

    public static Task<Motion> ToWorldMotion(this Shape shape, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null) =>
        new[] { shape }.ToWorldMotion(cameraDistance, startShape, stepDelay);

    public static Task<Motion> ToWorldMotion(this ActiveShape shape, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null) =>
        (new[] { shape }, new Shape[0]).ToWorldMotion(cameraDistance, startShape, stepDelay);

    public static Task<Motion> ToMotion(this IEnumerable<Shape> shapes, Vector3 cameraPosition)
    {
        return ToMotion(shapes, new MotionOptions()
        {
            CameraMotionOptions = new CameraMotionOptions()
            {
                CameraStartOptions = new CameraOptions
                {
                    Position = cameraPosition,
                    LookDirection = -cameraPosition.Normalize(),
                    UpDirection = Vector3.YAxis,
                }
            },
        });
    }

    public static Task<Motion> ToMotion(this IAsyncEnumerable<Shape> shapes, Vector3 cameraPosition)
    {
        return ToMotion(shapes, new MotionOptions()
        {
            CameraMotionOptions = new CameraMotionOptions()
            {
                CameraStartOptions = new CameraOptions
                {
                    Position = cameraPosition,
                    LookDirection = -cameraPosition.Normalize(),
                    UpDirection = Vector3.YAxis,
                }
            },
        });
    }

    public static Task<Motion> ToMotion2D(this IEnumerable<Shape> shapes, double cameraDistance) => shapes.ToMotion(new Vector3(0, 0, cameraDistance));

    public static Task<Motion> ToMotion(this IEnumerable<Shape> shapes, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        return ToMotion(shapes, new MotionOptions()
        {
            CameraDistance = cameraDistance,
            StartShape = startShape,
            StepDelay = stepDelay
        });
    }

    public static Task<Motion> ToMotion(this IAsyncEnumerable<Shape> shapes, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        return ToMotion(shapes, new MotionOptions()
        {
            CameraDistance = cameraDistance,
            StartShape = startShape,
            StepDelay = stepDelay
        });
    }

    public static Task<Motion> ToWorldMotion(this IEnumerable<Shape> shapes, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        var world = new ActiveWorld();
        world.AddActiveShapes(shapes);

        return world.ToMotion(new MotionOptions()
        {
            CameraDistance = cameraDistance,
            StartShape = startShape,
            StepDelay = stepDelay
        });
    }

    public static Task<Motion> ToWorldMotion(this (Shape[] actives, Shape[] statics) shapes, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null) =>
        (shapes.actives.Select(a => a.ToActiveShape()).ToArray(), shapes.statics).ToWorldMotion(cameraDistance, startShape, stepDelay);

    public static Task<Motion> ToWorldMotion(this (ActiveShape[] actives, Shape[] statics) shapes, double? cameraDistance = null, Shape? startShape = null, TimeSpan? stepDelay = null)
    {
        var world = new ActiveWorld();
        world.AddActiveShapes(shapes.actives);
        world.AddShapes(shapes.statics);

        return world.ToMotion(new MotionOptions()
        {
            CameraDistance = cameraDistance,
            StartShape = startShape,
            StepDelay = stepDelay
        });
    }

    public static ActiveWorld ToWorld(this Shape shape, Action<ActiveWorldOptions>? modifyFn = null) =>
        (new[] { shape.ToActiveShape() }, new Shape[0]).ToWorld(modifyFn);

    public static ActiveWorld ToWorld(this ActiveShape active, Action<ActiveWorldOptions>? modifyFn = null) =>
        (new[] {active}, new Shape[0]).ToWorld(modifyFn);

    public static ActiveWorld ToWorld(this IEnumerable<ActiveShape> actives, Action<ActiveWorldOptions>? modifyFn = null) =>
        (actives.ToArray(), new Shape[0]).ToWorld(modifyFn);

    public static ActiveWorld ToWorld(this IEnumerable<Shape> actives, Action<ActiveWorldOptions>? modifyFn = null) =>
        (actives.Select(a => a.ToActiveShape()).ToArray(), new Shape[0]).ToWorld(modifyFn);

    public static ActiveWorld ToWorld(this (Shape[] actives, Shape[] statics) shapes, Action<ActiveWorldOptions>? modifyFn = null) =>
        (shapes.actives.Select(a => a.ToActiveShape()).ToArray(), shapes.statics).ToWorld(modifyFn);

    public static ActiveWorld ToWorld(this (ActiveShape[] actives, Shape[] statics) shapes, Action<ActiveWorldOptions>? modifyFn = null)
    {
        var options = ActiveWorldValues.DefaultActiveWorldOptions;
        modifyFn?.Invoke(options);

        var world = new ActiveWorld(options);
        world.AddActiveShapes(shapes.actives);
        world.AddShapes(shapes.statics);

        return world;
    }

    public static Task<Motion> ToMotion(this IAsyncEnumerable<Shape> shapes, MotionOptions options) =>
        shapes.ToNoWaitSync().ToMotion(options);

    public static async Task<Motion> ToMotion(this IEnumerable<Shape> shapes, MotionOptions options)
    {
        var stepDelay = options.StepDelay ?? TimeSpan.FromMilliseconds(50);

        var queue = new ConcurrentQueue<(int, TaskCompletionSource)>();
        Shape? shape = null;

        var th = new Thread(() =>
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
                Thread.Sleep(stepDelay);

                if (queue.TryDequeue(out (int num, TaskCompletionSource t) v))
                {
                    var hasNewShape = CalculateNextShape();
                    Task.Run(() => v.t.SetResult());

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