
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Aspose.ThreeD.Utilities;
using Model;
using Model.Libraries;
using Model3D.Extensions;
using Model3D.Systems;
using Model3D.Systems.Model;
using Model3D.Tools;
using ViewMotion.Models;

namespace ViewMotion;

class SceneMotion
{
    #region ctor

    private readonly Vectorizer vectorizer;

    public SceneMotion(Vectorizer vectorizer)
    {
        this.vectorizer = vectorizer;
    }

    #endregion

    public async Task<Motion> Scene()
    {
        var queue = new ConcurrentQueue<(int, TaskCompletionSource)>();
        Shape shape = null;
        
        var th = new Thread(async () =>
        {
            var motion = WaterSystem.FountainMotion(new FountainOptions()
            {
                SceneSize = new Vector3(12, 18, 12),
                ParticleCount = 10000,
                ParticlePerEmissionCount = 2,
                EmissionAnimations = 1,
                ParticleSpeed = new Vector3(0.002, 0.12, 0.004),
                WaterPosition = new Vector3(0, 0.3, 0),
                Gravity = new Vector3(0, -1, 0),
                GravityPower = 0.001,
                LiquidPower = 0.0001,
                SkipAnimations = 0,
                StepAnimations = 10,
                SceneMotionSteps = 1000,
                JustAddShamrock = false
            }).GetEnumerator();

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
                await Task.Delay(10);

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

        shape = vectorizer.GetText("Fountain").Perfecto(10).ApplyColor(Color.Blue);
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