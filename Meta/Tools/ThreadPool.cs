using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Meta.Tools
{
    public class ThreadPool : IDisposable
    {
        public readonly int ThreadsCount;

        private bool running;
        private ConcurrentQueue<Action>[] queues;
        private uint currentTask;
        private Thread[] threads;

        public ThreadPool()
        {
            ThreadsCount = Environment.ProcessorCount;
            running = true;
            queues = Enumerable.Range(0, ThreadsCount).Select(_ => new ConcurrentQueue<Action>()).ToArray();
            threads = queues.Select((queue, i) => Polling(queue, $"Algo ThreadPool {i}")).ToArray();
        }

        public void Run(Action action)
        {
            var n = Interlocked.Increment(ref currentTask);
            var queue = queues[n % ThreadsCount];
            queue.Enqueue(action);
        }

        private Thread Polling(ConcurrentQueue<Action> queue, string name)
        {
            var thread = new Thread(() =>
            {
                var yieldCount = 0;

                while (running)
                {
                    if (queue.TryDequeue(out Action action))
                    {
                        action();
                        yieldCount = 100;
                    }
                    else
                    {
                        if (yieldCount == 0)
                        {
                            Thread.Sleep(1);
                        }
                        else
                        {
                            yieldCount--;
                            Thread.Yield();
                        }
                    }
                }
            })
            {
                Priority = ThreadPriority.AboveNormal,
                Name = name,
                IsBackground = true,
            };

            thread.Start();

            return thread;
        }

        public void Dispose()
        {
            running = false;
        }
    }
}
